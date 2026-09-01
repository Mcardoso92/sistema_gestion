# Módulo CobroVenta

Última actualización: 01/09/2026

---

# 1. Objetivo

CobroVenta representa cada ingreso financiero aplicado a una Venta.

Su objetivo es separar claramente:

```text
Venta = operación comercial
CobroVenta = ingreso de dinero asociado
```

Esta separación permite que una Venta sea cobrada mediante uno o varios pagos y diferentes MediosPago/Cajas.

---

# 2. Modelo actual

`CobroVenta` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| VentaId | Venta asociada |
| EmpresaId | Empresa propietaria |
| CajaId | Caja que recibe el dinero |
| MedioPagoId | Medio de pago utilizado |
| TurnoCajaId | Turno asociado cuando corresponde |
| UsuarioId | Usuario que registró el cobro |
| Fecha | Fecha/hora del cobro |
| Importe | Importe cobrado |
| Estado | Activo/Anulado |
| FechaAnulacion | Fecha/hora de anulación |
| UsuarioAnulacionId | Usuario que anuló |
| MotivoAnulacion | Motivo, máximo 500 caracteres |

Relaciones actuales:

- Venta.
- Empresa.
- Caja.
- MedioPago.
- TurnoCaja opcional.
- Usuario.
- MovimientoCaja.
- UsuarioAnulacion.

---

# 3. Autorización

`CobroVentaController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Las validaciones multiempresa y operativas se realizan adicionalmente dentro de cada acción.

---

# 4. Multiempresa

AdminEmpresa sólo puede operar CobrosVenta pertenecientes a:

```text
usuario.EmpresaId
```

La Venta, Caja, MedioPago y Turno utilizados deben corresponder a la misma Empresa.

SuperAdmin puede consultar/operar otras Empresas según el contexto permitido por el controller.

---

# 5. Venta válida para cobrar

No se puede registrar un CobroVenta si la Venta:

- no existe;
- pertenece a otra Empresa fuera del alcance del Usuario;
- está anulada (`Estado == false`);
- no posee saldo pendiente.

Estas condiciones se validan tanto al cargar el formulario como al registrar.

---

# 6. Saldo pendiente

El saldo pendiente de una Venta se calcula mediante `VentaSaldoService`.

Conceptualmente:

```text
SaldoPendiente = TotalVenta - CobrosActivos + ajustes derivados de reintegros según reglas vigentes
```

La lógica exacta se centraliza en el servicio y no debe duplicarse en controllers o vistas.

---

# 7. Cobros parciales

Una Venta puede recibir un CobroVenta por un importe menor al saldo pendiente.

Ejemplo:

```text
Venta total: $100.000
Cobro 1: $40.000
Saldo pendiente: $60.000
```

Posteriormente puede registrarse otro CobroVenta hasta completar el saldo.

---

# 8. Pagos múltiples

El modelo actual soporta múltiples CobroVenta sobre una misma Venta.

Esto permite combinar:

- distintos importes;
- distintos MediosPago;
- distintas Cajas válidas.

Ejemplo:

```text
Venta: $50.000

Cobro 1
Efectivo: $20.000

Cobro 2
Tarjeta: $30.000
```

Por lo tanto, el soporte de múltiples formas de pago ya está implementado en el dominio.

---

# 9. Límite del importe

El Importe debe ser mayor a cero.

El modelo utiliza actualmente:

```text
Range(0.01, 999999999.99)
```

Además:

```text
Importe <= SaldoPendiente
```

No se admite sobrecobro.

---

# 10. Revalidación por concurrencia

El registro del CobroVenta utiliza una transacción con:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se recalcula nuevamente el saldo pendiente.

Esto evita que dos solicitudes simultáneas cobren el mismo saldo restante.

Si el saldo cambió, la operación se rechaza y se informa el nuevo saldo actual.

---

# 11. Caja válida

La Caja elegida debe:

```text
pertenecer a venta.EmpresaId
Estado == true
```

Una Caja de otra Empresa o inactiva no puede utilizarse.

---

# 12. MedioPago válido

El MedioPago no se acepta de forma aislada.

Debe existir una relación activa:

```text
CajaMedioPago
```

entre la Caja seleccionada y el MedioPago.

También se valida que:

- Caja pertenezca a la Empresa.
- MedioPago pertenezca a la Empresa.
- Caja esté activa.
- MedioPago esté activo.

---

# 13. Selección dinámica Caja-MedioPago

El controller expone actualmente un endpoint para recuperar Cajas habilitadas para un MedioPago dentro de la Empresa de la Venta.

Esto permite que la UI filtre combinaciones inválidas.

Sin embargo, la combinación se vuelve a validar en el POST; el filtrado visual no constituye una garantía de seguridad.

---

# 14. TurnoCaja

Si la Caja seleccionada posee:

```text
PermiteTurnos == true
```

el Usuario debe tener un TurnoCaja propio abierto para esa misma Caja.

Se valida:

```text
Turno.EmpresaId == Venta.EmpresaId
Turno.UsuarioAperturaId == usuario.Id
Turno.Estado == Abierto
Turno.CajaId == caja.Id
```

Si no se cumple, el cobro se rechaza.

---

# 15. Cajas sin Turno

Si la Caja no utiliza turnos:

```text
PermiteTurnos == false
```

el CobroVenta puede registrarse sin `TurnoCajaId`.

Por eso el campo es nullable:

```text
int? TurnoCajaId
```

---

# 16. Registro del CobroVenta

Al registrar correctamente se persiste:

```text
VentaId
EmpresaId
CajaId
MedioPagoId
TurnoCajaId
UsuarioId
Fecha = DateTime.Now
Importe
Estado = Activo
```

Los campos de anulación comienzan en null.

---

# 17. MovimientoCaja automático

Cada CobroVenta genera también un MovimientoCaja.

Actualmente se registra como:

```text
Tipo = CobroVenta
Direccion = Ingreso
Importe = cobro.Importe
CajaId = caja seleccionada
MedioPagoId = medio seleccionado
TurnoCajaId = turno cuando aplica
CobroVentaId = cobro.Id
```

El Concepto generado es equivalente a:

```text
Cobro de venta #<VentaId>
```

---

# 18. Atomicidad financiera

CobroVenta y MovimientoCaja se crean dentro de la misma transacción.

La operación debe quedar completa o no quedar registrada.

No debería existir normalmente:

```text
CobroVenta sin MovimientoCaja correspondiente
```

porque rompería la coherencia financiera.

---

# 19. Estado

`CobroVenta` utiliza `EstadoCobro`.

Los estados operativos actuales incluyen:

```text
Activo
Anulado
```

La anulación no elimina físicamente el CobroVenta.

---

# 20. Anulación

Un CobroVenta activo puede anularse mediante el flujo específico de Anular.

No se edita ni se elimina directamente.

La anulación conserva:

- registro original;
- importe original;
- Caja original;
- MedioPago original;
- fecha original;
- usuario original.

Además agrega trazabilidad de la anulación.

---

# 21. Datos de anulación

Al anular se establece:

```text
Estado = Anulado
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
MotivoAnulacion = motivo ingresado
```

El motivo posee máximo 500 caracteres en el modelo.

---

# 22. Movimiento original requerido

Para anular un CobroVenta debe existir su MovimientoCaja original de tipo:

```text
CobroVenta
```

Si no existe, la anulación es rechazada porque la operación financiera estaría inconsistente.

---

# 23. Reversión financiera

La anulación NO elimina ni modifica el MovimientoCaja original.

En cambio genera un nuevo MovimientoCaja:

```text
Tipo = ReversionCobroVenta
Direccion = Egreso
Importe = movimientoOriginal.Importe
MovimientoOrigenId = movimientoOriginal.Id
CobroVentaId = cobro.Id
```

Esto preserva trazabilidad completa.

---

# 24. Prevención de doble reversión

Antes de anular se verifica que no exista ya otro MovimientoCaja con:

```text
MovimientoOrigenId == movimientoOriginal.Id
```

Si ya existe, el sistema bloquea la operación.

Un mismo cobro no puede revertirse dos veces.

---

# 25. Reintegros activos

La anulación de un CobroVenta puede bloquearse si existen reintegros activos asociados a la Venta que dependen del importe cobrado.

La regla se centraliza mediante:

```text
VentaSaldoService.PuedeAnularCobro(...)
```

Esto evita dejar una Venta con reintegros superiores al dinero efectivamente cobrado luego de la anulación.

---

# 26. Revalidación de anulación

La anulación también utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se vuelve a ejecutar la validación de `PuedeAnularCobro`.

Esto protege contra cambios concurrentes en reintegros o saldos.

---

# 27. Caja disponible al anular

La Caja vinculada al MovimientoCaja original debe continuar existiendo, pertenecer a la misma Empresa y estar activa.

Si ya no está disponible, la anulación se rechaza.

---

# 28. Turno requerido para reversión

Si la Caja del movimiento original utiliza Turnos, el Usuario que anula debe tener un TurnoCaja propio abierto para esa misma Caja.

La reversión impacta el Turno operativo actual del Usuario, no necesariamente el Turno histórico original del cobro.

Esto es importante porque una anulación puede ocurrir en un momento posterior al cobro original.

---

# 29. Saldo disponible de Caja

Antes de generar la reversión se calcula el saldo disponible mediante:

```text
CajaSaldoService.CalcularSaldoDisponible(...)
```

Si:

```text
importe del cobro > saldo disponible de Caja
```

la anulación se bloquea.

Esto evita generar un egreso de reversión que deje una Caja sin fondos suficientes según las reglas actuales.

---

# 30. Cobro anulado y saldo de Venta

Una vez anulado, el CobroVenta deja de computar como cobro activo.

Por lo tanto, la Venta puede volver a presentar saldo pendiente.

Ejemplo:

```text
Venta: $100
Cobro activo: $100
Saldo: $0

Se anula el cobro

Cobros activos: $0
Saldo: $100
```

---

# 31. No edición de cobros

CobroVenta no funciona como CRUD administrativo tradicional.

No debe permitirse modificar libremente:

- Importe.
- Caja.
- MedioPago.
- Venta.
- Fecha.
- Usuario.

Si una operación fue incorrecta, debe anularse y registrar una nueva operación correcta.

Esto preserva trazabilidad financiera.

---

# 32. Relación con ReintegroVenta

CobroVenta y ReintegroVenta participan conjuntamente en el saldo financiero de una Venta.

El sistema debe evitar estados imposibles como:

```text
Reintegros activos > Cobros activos compatibles
```

Por eso la anulación de cobros consulta las reglas del VentaSaldoService.

---

# 33. Relación con TurnoCaja

Los CobroVenta activos asociados a un Turno se agrupan en el detalle de TurnoCaja por MedioPago.

Esto permite conocer:

- total cobrado por medio;
- cantidad de cobros;
- composición de la recaudación del Turno.

---

# 34. Seguridad

Las reglas críticas deben validarse en backend:

- alcance de Empresa;
- Venta activa;
- saldo pendiente;
- Importe válido;
- Caja válida;
- MedioPago válido para la Caja;
- Turno propio cuando corresponde;
- saldo actualizado bajo transacción;
- existencia del movimiento original al anular;
- ausencia de reversión previa;
- compatibilidad con reintegros activos;
- saldo disponible de Caja para la reversión.

La UI no constituye fuente de verdad.

---

# 35. Reglas de negocio actuales

1. Un CobroVenta pertenece a una Venta y Empresa.
2. Puede existir más de un CobroVenta por Venta.
3. Esto permite pagos parciales y múltiples MediosPago.
4. No se puede cobrar una Venta anulada.
5. No se puede cobrar una Venta sin saldo pendiente.
6. El Importe debe ser mayor a cero.
7. El Importe no puede superar el saldo pendiente.
8. El saldo se recalcula dentro de una transacción Serializable.
9. Caja y MedioPago deben pertenecer a la Empresa.
10. Caja y MedioPago deben estar activos.
11. MedioPago debe estar asociado a la Caja mediante CajaMedioPago.
12. Si Caja.PermiteTurnos, el Usuario necesita un Turno propio abierto en esa Caja.
13. Cada CobroVenta genera un MovimientoCaja de Ingreso.
14. CobroVenta y MovimientoCaja se registran atómicamente.
15. Un CobroVenta no se elimina físicamente.
16. La corrección se realiza mediante Anulación.
17. La anulación registra Usuario, Fecha y Motivo.
18. La anulación genera un MovimientoCaja de Egreso de tipo ReversionCobroVenta.
19. La reversión referencia al movimiento original mediante MovimientoOrigenId.
20. No puede existir doble reversión.
21. La anulación puede bloquearse por reintegros activos.
22. La anulación se revalida dentro de transacción Serializable.
23. La Caja debe disponer de saldo suficiente para revertir.
24. Si la Caja usa Turnos, la reversión requiere Turno propio abierto.
25. Cobros históricos no deben editarse libremente.
26. AdminEmpresa queda restringido a su Empresa.

---

# 36. Evolución futura

Posibles mejoras futuras:

- conciliación externa por cobro;
- referencia/número de operación de tarjetas o transferencias;
- integración directa con Mercado Pago;
- estados pendientes de acreditación;
- comisiones por MedioPago;
- cuotas/tarjetas con detalle adicional;
- recibos digitales;
- permisos granulares para registrar/anular cobros;
- auditoría/reportes avanzados por usuario, Caja y MedioPago.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 37. Estado actual

✅ Cobros parciales implementados.

✅ Múltiples cobros por Venta implementados.

✅ Múltiples MediosPago implementados.

✅ Validación Caja-MedioPago implementada.

✅ Integración con TurnoCaja implementada.

✅ MovimientoCaja automático implementado.

✅ Transacciones Serializable implementadas.

✅ Anulación lógica implementada.

✅ Reversión financiera mediante nuevo MovimientoCaja implementada.

✅ Prevención de doble reversión implementada.

✅ Validación contra reintegros activos implementada.

✅ Validación de saldo disponible de Caja en anulación implementada.

✅ Trazabilidad de Usuario/Fecha/Motivo de anulación implementada.

🚧 Conciliación externa pendiente.

🚧 Integraciones directas con proveedores de pago pendientes.

🚧 Permisos granulares pendientes.