# Módulo PagoProveedor

Última actualización: 01/09/2026

---

# 1. Objetivo

PagoProveedor representa cada egreso financiero aplicado a una Compra.

Su objetivo es separar claramente:

```text
Compra = operación comercial
PagoProveedor = salida de dinero asociada
```

Esta separación permite pagar una Compra en uno o varios pagos, utilizando diferentes MediosPago y Cajas válidas.

---

# 2. Modelo actual

`PagoProveedor` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| CompraId | Compra asociada |
| EmpresaId | Empresa propietaria |
| CajaId | Caja desde la cual se paga |
| MedioPagoId | Medio de pago utilizado |
| TurnoCajaId | Turno asociado cuando corresponde |
| UsuarioId | Usuario que registró el pago |
| Fecha | Fecha/hora del pago |
| Importe | Importe pagado |
| Estado | Activo/Anulado |
| FechaAnulacion | Fecha/hora de anulación |
| UsuarioAnulacionId | Usuario que anuló |
| MotivoAnulacion | Motivo, máximo 500 caracteres |

Relaciones actuales:

- Compra.
- Empresa.
- Caja.
- MedioPago.
- TurnoCaja opcional.
- Usuario.
- MovimientoCaja.
- UsuarioAnulacion.

---

# 3. Autorización

`PagoProveedorController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Las validaciones multiempresa y operativas se aplican adicionalmente en cada acción.

---

# 4. Multiempresa

AdminEmpresa sólo puede operar pagos pertenecientes a:

```text
usuario.EmpresaId
```

Compra, Caja, MedioPago y Turno deben pertenecer a la misma Empresa.

SuperAdmin puede operar globalmente según el contexto permitido por el controller.

---

# 5. Compra válida para pagar

No se puede registrar un PagoProveedor si la Compra:

- no existe;
- pertenece a otra Empresa fuera del alcance del Usuario;
- está anulada (`Estado == false`);
- no posee saldo pendiente.

Estas condiciones se validan antes de mostrar el formulario y nuevamente al registrar.

---

# 6. Saldo pendiente de Compra

El saldo pendiente se obtiene mediante `CompraSaldoService`.

La lógica se mantiene centralizada en ese servicio y no debe duplicarse en vistas o controllers.

Conceptualmente:

```text
SaldoPendiente = obligación financiera neta de la Compra - PagosProveedor activos
```

considerando también las reglas vigentes de devoluciones y reintegros del proveedor.

---

# 7. Pagos parciales

Una Compra puede recibir un pago inferior a su saldo pendiente.

Ejemplo:

```text
Compra total: $100.000
Pago 1: $30.000
Saldo pendiente: $70.000
```

Posteriormente pueden registrarse nuevos PagoProveedor hasta completar el saldo.

---

# 8. Múltiples pagos

El modelo permite múltiples PagoProveedor para una misma Compra.

Esto permite combinar:

- distintos importes;
- distintos MediosPago;
- distintas Cajas válidas.

Ejemplo:

```text
Compra: $80.000

Pago 1
Transferencia: $50.000

Pago 2
Efectivo: $30.000
```

---

# 9. Importe válido

El modelo exige:

```text
Importe > 0
```

mediante:

```text
Range(0.01, 999999999.99)
```

Además:

```text
Importe <= SaldoPendiente
```

No se permite pagar por encima del saldo pendiente.

---

# 10. Caja válida

La Caja seleccionada debe:

```text
pertenecer a compra.EmpresaId
Estado == true
```

Una Caja inactiva o de otra Empresa no puede utilizarse.

---

# 11. MedioPago válido

El MedioPago debe estar habilitado para la Caja mediante:

```text
CajaMedioPago
```

Se valida que:

- Caja pertenezca a la Empresa.
- MedioPago pertenezca a la Empresa.
- Caja esté activa.
- MedioPago esté activo.
- exista la asociación Caja-MedioPago.

---

# 12. Selección dinámica Caja-MedioPago

El controller expone un endpoint que obtiene las Cajas válidas para un MedioPago dentro de la Empresa de la Compra.

Esto mejora la UX, pero la combinación se vuelve a validar en el POST.

La UI nunca reemplaza la validación de backend.

---

# 13. TurnoCaja

Si la Caja utilizada posee:

```text
PermiteTurnos == true
```

el Usuario debe tener un TurnoCaja propio abierto para esa misma Caja.

Se verifica:

```text
Turno.EmpresaId == Compra.EmpresaId
Turno.UsuarioAperturaId == usuario.Id
Turno.Estado == Abierto
Turno.CajaId == caja.Id
```

Si no se cumple, el pago se rechaza.

---

# 14. Cajas sin Turno

Si:

```text
Caja.PermiteTurnos == false
```

el PagoProveedor puede registrarse sin `TurnoCajaId`.

Por eso:

```text
TurnoCajaId
```

es nullable.

---

# 15. Saldo disponible de Caja

A diferencia de un CobroVenta, un PagoProveedor representa una salida de dinero.

Antes de registrar se calcula:

```text
CajaSaldoService.CalcularSaldoDisponible(...)
```

Si:

```text
Importe > saldo disponible de Caja
```

la operación se rechaza.

Veltika no permite registrar un pago que deje a la Caja sin fondos suficientes según las reglas actuales.

---

# 16. Revalidación por concurrencia

El registro utiliza una transacción:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- que la Compra siga existiendo;
- que continúe activa;
- saldo pendiente actual;
- Caja activa;
- asociación Caja-MedioPago;
- Turno operativo;
- saldo disponible de Caja.

Esto protege la operación frente a cambios concurrentes.

---

# 17. Registro del PagoProveedor

Al registrar correctamente se persiste:

```text
CompraId
EmpresaId
CajaId
MedioPagoId
TurnoCajaId
UsuarioId
Fecha = DateTime.Now
Importe
Estado = Activo
```

Los datos de anulación comienzan en null.

---

# 18. MovimientoCaja automático

Cada PagoProveedor genera un MovimientoCaja.

Actualmente:

```text
Tipo = PagoProveedor
Direccion = Egreso
Importe = pago.Importe
CajaId = caja seleccionada
MedioPagoId = medio seleccionado
TurnoCajaId = turno cuando aplica
PagoProveedorId = pago.Id
```

El Concepto generado es equivalente a:

```text
Pago de compra #<CompraId>
```

---

# 19. Atomicidad financiera

PagoProveedor y MovimientoCaja se registran dentro de la misma transacción.

La operación debe completarse integralmente o revertirse.

No debería quedar normalmente un:

```text
PagoProveedor sin MovimientoCaja asociado
```

---

# 20. Estado

PagoProveedor utiliza `EstadoPago`.

Los estados operativos actuales incluyen:

```text
Activo
Anulado
```

La anulación no elimina físicamente el pago.

---

# 21. Anulación

Un pago incorrecto no se edita ni elimina.

Se utiliza el flujo específico de Anular.

Esto conserva:

- pago original;
- Importe original;
- Caja original;
- MedioPago original;
- Usuario original;
- Fecha original.

Y agrega la trazabilidad de la anulación.

---

# 22. Datos de anulación

Al anular se establece:

```text
Estado = Anulado
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
MotivoAnulacion = motivo
```

El Motivo puede tener hasta 500 caracteres.

---

# 23. Movimiento original requerido

Para anular debe existir el MovimientoCaja original de tipo:

```text
PagoProveedor
```

Si no existe, la operación se rechaza porque existe una inconsistencia financiera.

---

# 24. Prevención de doble reversión

Se verifica que no exista previamente un MovimientoCaja donde:

```text
MovimientoOrigenId == movimientoOriginal.Id
```

Si ya existe, el pago no puede volver a anularse/revertirse.

---

# 25. Reintegros del proveedor

Antes de anular un pago se verifica que los reintegros activos del proveedor continúen siendo justificables después de quitar ese pago.

El controller calcula:

- total pagado activo;
- total pagado luego de anular;
- devoluciones de Compra activas;
- reintegros de proveedor activos;
- total neto de la Compra;
- máximo reintegrable después de la anulación.

---

# 26. Compra neta

El total neto utilizado durante esta validación se calcula conceptualmente como:

```text
TotalNetoCompra = max(0, Compra.Total - DevolucionesCompraActivas)
```

Esto reconoce que una devolución reduce la obligación económica real de la Compra.

---

# 27. Máximo reintegrable

Luego de simular la anulación del PagoProveedor:

```text
MaximoReintegrable = max(
    0,
    TotalPagadoLuegoDeAnular - TotalNetoCompra
)
```

Si los ReintegroProveedor activos superan ese máximo, la anulación se bloquea.

El Usuario debe anular primero los reintegros correspondientes.

---

# 28. Reversión financiera

Al anular, el MovimientoCaja original NO se elimina.

Se crea otro MovimientoCaja:

```text
Tipo = ReversionPagoProveedor
Direccion = Ingreso
Importe = movimientoOriginal.Importe
MovimientoOrigenId = movimientoOriginal.Id
PagoProveedorId = pago.Id
```

La Dirección es Ingreso porque el pago original fue un Egreso y su reversión devuelve ese importe a la Caja.

---

# 29. Turno requerido al revertir

Si la Caja original utiliza Turnos, el Usuario que anula debe tener un TurnoCaja propio abierto para esa Caja.

La reversión se asocia al Turno operativo actual.

No necesariamente utiliza el Turno histórico del pago original.

---

# 30. Efecto sobre saldo de Compra

Un PagoProveedor anulado deja de computar como pago activo.

Por lo tanto la Compra puede volver a presentar saldo pendiente.

Ejemplo:

```text
Compra: $100
Pago activo: $100
Saldo pendiente: $0

Se anula el pago

Pagos activos: $0
Saldo pendiente: $100
```

---

# 31. Efecto sobre saldo de Caja

Pago original:

```text
Direccion = Egreso
```

Reversión:

```text
Direccion = Ingreso
```

Por lo tanto la anulación reincorpora financieramente el importe a la Caja mediante un nuevo movimiento trazable.

---

# 32. No edición histórica

PagoProveedor no funciona como CRUD tradicional.

No debe modificarse libremente:

- Importe.
- Compra.
- Caja.
- MedioPago.
- Fecha.
- Usuario.

Si el pago fue incorrecto:

```text
Anular pago
+
Registrar pago correcto
```

---

# 33. Relación con DevolucionCompra

Las DevolucionCompra activas reducen el total neto de la Compra.

Esto afecta las reglas económicas necesarias para validar reintegros y anulaciones de PagoProveedor.

---

# 34. Relación con ReintegroProveedor

Un ReintegroProveedor representa dinero devuelto por el proveedor luego de que la Empresa pagó más de lo que corresponde mantener como costo neto.

Por eso la anulación de PagoProveedor debe comprobar que los reintegros existentes sigan respaldados financieramente.

---

# 35. Seguridad

Las reglas críticas se validan en backend:

- Empresa correcta.
- Compra activa.
- saldo pendiente.
- Importe válido.
- Caja activa.
- MedioPago válido para Caja.
- Turno propio cuando corresponde.
- saldo disponible de Caja.
- revalidación dentro de transacción Serializable.
- existencia del movimiento original al anular.
- ausencia de doble reversión.
- coherencia con DevolucionCompra.
- coherencia con ReintegroProveedor.

---

# 36. Reglas de negocio actuales

1. PagoProveedor pertenece a una Compra y Empresa.
2. Una Compra puede poseer múltiples pagos.
3. Se permiten pagos parciales.
4. No puede pagarse una Compra anulada.
5. No puede pagarse una Compra sin saldo pendiente.
6. Importe debe ser mayor a cero.
7. Importe no puede superar el saldo pendiente.
8. Caja y MedioPago deben pertenecer a la Empresa.
9. Caja y MedioPago deben estar activos.
10. MedioPago debe estar asociado a Caja mediante CajaMedioPago.
11. Si Caja utiliza Turnos, el Usuario necesita Turno propio abierto.
12. La Caja debe tener saldo suficiente antes de registrar el pago.
13. La operación se revalida dentro de transacción Serializable.
14. Cada PagoProveedor genera un MovimientoCaja de Egreso.
15. PagoProveedor y MovimientoCaja se registran atómicamente.
16. Los pagos no se eliminan físicamente.
17. La corrección se realiza mediante Anulación.
18. La anulación registra Fecha, Usuario y Motivo.
19. La anulación genera MovimientoCaja de tipo ReversionPagoProveedor.
20. La reversión posee Dirección Ingreso.
21. La reversión referencia el movimiento original.
22. No se permite doble reversión.
23. La anulación debe preservar la coherencia con ReintegroProveedor activos.
24. DevolucionCompra reduce el total neto considerado en esta validación.
25. Si la Caja usa Turnos, la reversión requiere Turno propio abierto.
26. Los registros históricos no deben editarse libremente.
27. AdminEmpresa queda restringido a su Empresa.

---

# 37. Evolución futura

Posibles mejoras futuras:

- cuenta corriente avanzada de Proveedor;
- vencimientos y condiciones de pago;
- planificación de pagos;
- órdenes de pago;
- conciliación bancaria;
- comprobantes/recibos de proveedor;
- referencias externas por transferencia/cheque;
- reportes de pagos por período y proveedor;
- permisos granulares para registrar/anular pagos.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 38. Estado actual

✅ Pagos parciales implementados.

✅ Múltiples pagos por Compra implementados.

✅ Múltiples MediosPago implementados.

✅ Validación Caja-MedioPago implementada.

✅ Validación de saldo disponible de Caja implementada.

✅ Integración con TurnoCaja implementada.

✅ MovimientoCaja automático implementado.

✅ Transacción Serializable implementada.

✅ Anulación lógica implementada.

✅ Reversión mediante MovimientoCaja de Ingreso implementada.

✅ Prevención de doble reversión implementada.

✅ Validación contra ReintegroProveedor activos implementada.

✅ Consideración de DevolucionCompra en la coherencia financiera implementada.

✅ Trazabilidad de Usuario/Fecha/Motivo implementada.

🚧 Cuenta corriente avanzada de proveedor pendiente.

🚧 Conciliación bancaria pendiente.

🚧 Permisos granulares pendientes.