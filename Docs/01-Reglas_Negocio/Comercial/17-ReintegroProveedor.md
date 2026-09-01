# Módulo ReintegroProveedor

Última actualización: 01/09/2026

---

# 1. Objetivo

ReintegroProveedor representa el dinero que un Proveedor devuelve a la Empresa asociado a una Compra.

No representa la devolución física de productos. Esa responsabilidad corresponde a `DevolucionCompra`.

Conceptualmente:

```text
DevolucionCompra = corrección física/comercial
ReintegroProveedor = recuperación financiera
```

---

# 2. Modelo actual

`ReintegroProveedor` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| CompraId | Compra asociada |
| EmpresaId | Empresa propietaria |
| CajaId | Caja donde ingresa el dinero |
| MedioPagoId | Medio por el cual se recibe |
| TurnoCajaId | Turno asociado cuando corresponde |
| UsuarioId | Usuario que registra |
| Fecha | Fecha/hora del reintegro |
| Importe | Importe reintegrado |
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

`ReintegroProveedorController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Además se aplican controles multiempresa dentro de cada acción.

---

# 4. Multiempresa

AdminEmpresa sólo puede operar reintegros pertenecientes a:

```text
usuario.EmpresaId
```

Compra, Caja, MedioPago y Turno deben pertenecer a la misma Empresa.

SuperAdmin puede operar globalmente según el contexto permitido por el controller.

---

# 5. Compra válida

No se puede registrar un ReintegroProveedor si la Compra:

- no existe;
- pertenece a otra Empresa fuera del alcance del Usuario;
- está anulada.

Estas condiciones se validan antes de mostrar el formulario y nuevamente al registrar.

---

# 6. Importe pendiente de recuperar

El importe disponible para reintegrar se obtiene mediante:

```text
CompraSaldoService.ObtenerPendienteRecuperar(...)
```

La lógica económica se mantiene centralizada en `CompraSaldoService`.

Conceptualmente representa cuánto dinero puede recuperar todavía la Empresa del Proveedor luego de considerar:

- total de la Compra;
- pagos activos al Proveedor;
- devoluciones activas;
- reintegros ya registrados.

---

# 7. Condición para registrar

Debe cumplirse:

```text
PendienteRecuperar > 0
```

Si no existe importe pendiente, el sistema bloquea el registro.

---

# 8. Reintegros parciales

Se permiten reintegros parciales.

Ejemplo:

```text
Pendiente recuperar: $30.000

Reintegro 1: $10.000
Pendiente restante: $20.000
```

Pueden registrarse nuevos ReintegroProveedor hasta completar el importe disponible.

---

# 9. Importe máximo

El Importe debe cumplir:

```text
Importe > 0
Importe <= PendienteRecuperar
```

El modelo utiliza:

```text
Range(0.01, 999999999.99)
```

No se permite recuperar más dinero del que corresponde económicamente.

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

El MedioPago debe estar asociado a la Caja mediante `CajaMedioPago`.

Se valida que:

- Caja pertenezca a la Empresa.
- MedioPago pertenezca a la Empresa.
- Caja esté activa.
- MedioPago esté activo.
- exista asociación Caja-MedioPago.

---

# 12. Selección dinámica Caja-MedioPago

El controller expone un endpoint para obtener Cajas válidas según el MedioPago seleccionado dentro de la Empresa de la Compra.

Esto mejora la UX, pero el POST vuelve a validar toda la combinación.

La UI no es fuente de verdad.

---

# 13. TurnoCaja

Si la Caja posee:

```text
PermiteTurnos == true
```

el Usuario debe tener un TurnoCaja propio abierto para esa misma Caja.

Se valida:

```text
Turno.EmpresaId == Compra.EmpresaId
Turno.UsuarioAperturaId == usuario.Id
Turno.Estado == Abierto
Turno.CajaId == caja.Id
```

---

# 14. Caja sin Turno

Si:

```text
Caja.PermiteTurnos == false
```

`TurnoCajaId` puede quedar null.

---

# 15. Registro transaccional

El registro utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- existencia de Compra;
- Estado de Compra;
- pendiente actual a recuperar;
- Importe;
- Caja activa;
- asociación Caja-MedioPago;
- Turno operativo.

Esto protege frente a operaciones concurrentes.

---

# 16. Registro de ReintegroProveedor

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

# 17. MovimientoCaja automático

Cada ReintegroProveedor genera un MovimientoCaja con:

```text
Tipo = ReintegroProveedor
Direccion = Ingreso
Importe = reintegro.Importe
CajaId = caja seleccionada
MedioPagoId = medio seleccionado
TurnoCajaId = turno cuando aplica
ReintegroProveedorId = reintegro.Id
```

El Concepto generado es equivalente a:

```text
Reintegro de proveedor por compra #<CompraId>
```

---

# 18. Sentido financiero

El ReintegroProveedor es un Ingreso porque el dinero vuelve desde el Proveedor hacia la Empresa.

Conceptualmente:

```text
PagoProveedor
    -> Egreso

ReintegroProveedor
    -> Ingreso
```

---

# 19. Atomicidad financiera

ReintegroProveedor y MovimientoCaja se registran dentro de la misma transacción.

No debería quedar normalmente un ReintegroProveedor sin su MovimientoCaja correspondiente.

---

# 20. Estado

ReintegroProveedor utiliza `EstadoReintegro`.

Los estados operativos actuales incluyen:

```text
Activo
Anulado
```

La anulación es lógica y no elimina el registro.

---

# 21. Anulación

Un reintegro incorrecto no se edita ni elimina.

Se utiliza el flujo específico de Anular.

Esto conserva:

- Importe original.
- Caja original.
- MedioPago original.
- Usuario original.
- Fecha original.
- MovimientoCaja original.

---

# 22. Datos de anulación

Al anular:

```text
Estado = Anulado
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
MotivoAnulacion = motivo
```

El Motivo posee máximo 500 caracteres.

---

# 23. MovimientoCaja original requerido

La anulación requiere encontrar el MovimientoCaja original con:

```text
ReintegroProveedorId == reintegro.Id
Tipo == ReintegroProveedor
```

Si no existe, la anulación se rechaza por inconsistencia financiera.

---

# 24. Prevención de doble reversión

Antes de anular se verifica que no exista otro MovimientoCaja con:

```text
MovimientoOrigenId == movimientoOriginal.Id
Tipo == ReversionReintegroProveedor
```

Si ya existe, no se permite una segunda reversión.

---

# 25. Caja original activa

Para anular, la Caja asociada al reintegro debe seguir:

```text
existiendo
activa
perteneciendo a la misma Empresa
```

Si la Caja ya no está disponible, la operación se rechaza.

---

# 26. Turno requerido al anular

Si la Caja original utiliza Turnos, el Usuario que anula debe poseer un TurnoCaja propio abierto para esa misma Caja.

La reversión se asocia al Turno operativo actual.

No necesariamente reutiliza el Turno histórico original.

---

# 27. Saldo disponible al anular

Anular un ReintegroProveedor implica retirar nuevamente de Caja el dinero que previamente había ingresado.

Por eso se calcula:

```text
CajaSaldoService.CalcularSaldoDisponible(...)
```

Debe cumplirse:

```text
Reintegro.Importe <= SaldoDisponibleCaja
```

Si la Caja no tiene fondos suficientes, la anulación se bloquea.

---

# 28. Motivo del control de saldo

Ejemplo:

```text
Reintegro proveedor: +$20.000

Luego ese dinero se utiliza en otras operaciones.
Saldo actual de Caja: $5.000
```

No puede anularse inmediatamente el reintegro por $20.000 porque la reversión requiere generar un Egreso real por ese importe.

---

# 29. Movimiento de reversión

La anulación genera un nuevo MovimientoCaja:

```text
Tipo = ReversionReintegroProveedor
Direccion = Egreso
Importe = reintegro.Importe
MovimientoOrigenId = movimientoOriginal.Id
```

También conserva:

- EmpresaId.
- CajaId.
- MedioPagoId.
- UsuarioId.
- TurnoCajaId cuando aplica.
- Motivo en Observaciones.

---

# 30. Movimiento original conservado

El MovimientoCaja original de Ingreso no se elimina ni se modifica para ocultar la operación.

La trazabilidad queda:

```text
ReintegroProveedor
    -> Ingreso

Anulación
    -> ReversionReintegroProveedor
    -> Egreso
    -> MovimientoOrigenId = movimiento original
```

---

# 31. Efecto económico al anular

Un ReintegroProveedor anulado deja de contar como dinero recuperado.

Por lo tanto puede volver a aumentar el importe pendiente de recuperar del Proveedor, según las demás condiciones de la Compra.

---

# 32. Relación con DevolucionCompra

Una DevolucionCompra reduce el total económico neto que la Empresa debería conservar como costo de la Compra.

Si ya se había pagado más que ese nuevo total neto, aparece potencialmente un importe recuperable del Proveedor.

Ejemplo:

```text
Compra: $100
Pagado: $100
Devolución: $30

Total neto: $70
Exceso pagado potencialmente recuperable: $30
```

---

# 33. Relación con PagoProveedor

El reintegro sólo puede estar justificado si existe una diferencia económica entre lo pagado y lo que finalmente corresponde conservar como costo neto.

Por eso PagoProveedor, DevolucionCompra y ReintegroProveedor forman un mismo circuito financiero.

---

# 34. Circuito completo

Conceptualmente:

```text
Compra
    ↓
PagoProveedor
    Egreso Caja

DevolucionCompra
    ↓
Reduce stock y total neto

ReintegroProveedor
    Ingreso Caja
```

Si se anula el reintegro:

```text
ReversionReintegroProveedor
    Egreso Caja
```

---

# 35. No edición histórica

ReintegroProveedor no funciona como CRUD editable.

No debe modificarse libremente:

- Compra.
- Importe.
- Caja.
- MedioPago.
- Fecha.
- Usuario.

Si fue incorrecto:

```text
Anular reintegro
+
Registrar reintegro correcto
```

---

# 36. Seguridad

Las reglas críticas se validan en backend:

- Empresa correcta.
- Compra activa.
- pendiente de recuperar actual.
- Importe válido.
- Caja activa.
- MedioPago válido para Caja.
- Turno propio cuando corresponde.
- revalidación transaccional Serializable.
- movimiento original requerido al anular.
- prevención de doble reversión.
- Caja original disponible.
- saldo suficiente para generar la reversión.

---

# 37. Reglas de negocio actuales

1. ReintegroProveedor pertenece a una Compra y Empresa.
2. Sólo puede registrarse sobre una Compra activa.
3. Debe existir importe pendiente de recuperar.
4. Se permiten reintegros parciales.
5. Importe debe ser mayor a cero.
6. Importe no puede superar el pendiente de recuperar.
7. Caja y MedioPago deben pertenecer a la Empresa.
8. Caja y MedioPago deben estar activos.
9. MedioPago debe estar asociado a Caja mediante CajaMedioPago.
10. Si Caja utiliza Turnos, el Usuario necesita Turno propio abierto.
11. El pendiente se revalida dentro de transacción Serializable.
12. Cada ReintegroProveedor genera MovimientoCaja de Ingreso.
13. ReintegroProveedor y MovimientoCaja se registran atómicamente.
14. Los reintegros no se eliminan físicamente.
15. La corrección se realiza mediante Anulación.
16. La anulación registra Fecha, Usuario y Motivo.
17. La anulación requiere el MovimientoCaja original.
18. No se permite doble reversión.
19. Si Caja usa Turnos, la anulación requiere Turno propio abierto.
20. La Caja debe tener saldo suficiente para anular.
21. La anulación genera MovimientoCaja de tipo ReversionReintegroProveedor.
22. La reversión tiene Dirección Egreso.
23. La reversión referencia al movimiento original.
24. Los registros históricos no deben editarse libremente.
25. AdminEmpresa sólo opera dentro de su Empresa.

---

# 38. Evolución futura

Posibles mejoras futuras:

- referencia a nota de crédito del proveedor;
- número de comprobante externo;
- conciliación bancaria;
- reintegros pendientes de acreditación;
- seguimiento por transferencia o cheque;
- cuenta corriente avanzada de Proveedor;
- reportes de recuperos por período/proveedor;
- permisos granulares para registrar/anular reintegros.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 39. Estado actual

✅ Reintegros parciales implementados.

✅ Cálculo de pendiente a recuperar centralizado implementado.

✅ Validación Caja-MedioPago implementada.

✅ Integración con TurnoCaja implementada.

✅ MovimientoCaja automático de Ingreso implementado.

✅ Transacción Serializable implementada.

✅ Anulación lógica implementada.

✅ Reversión mediante MovimientoCaja de Egreso implementada.

✅ Control de saldo de Caja para anular implementado.

✅ Prevención de doble reversión implementada.

✅ Trazabilidad de Usuario/Fecha/Motivo implementada.

✅ Integración económica con PagoProveedor y DevolucionCompra implementada.

🚧 Conciliación bancaria pendiente.

🚧 Comprobantes externos/nota de crédito pendiente.

🚧 Permisos granulares pendientes.