# Módulo TransferenciaCaja

Última actualización: 01/09/2026

---

# 1. Objetivo

TransferenciaCaja representa el traslado interno de dinero entre dos Cajas pertenecientes a la misma Empresa.

No genera ingreso ni egreso económico para la Empresa en términos globales.

Conceptualmente:

```text
Caja Origen
    - Importe

Caja Destino
    + Importe

Empresa
    variación neta = 0
```

---

# 2. Modelo actual

`TransferenciaCaja` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| EmpresaId | Empresa propietaria |
| CajaOrigenId | Caja desde donde sale el dinero |
| CajaDestinoId | Caja que recibe el dinero |
| UsuarioId | Usuario que registra |
| TurnoCajaId | Turno asociado a Caja Origen cuando corresponde |
| Fecha | Fecha/hora de la transferencia |
| Importe | Importe transferido |
| Motivo | Obligatorio, máximo 250 caracteres |
| Estado | Activa/Anulada |
| FechaAnulacion | Fecha/hora de anulación |
| UsuarioAnulacionId | Usuario que anuló |
| MotivoAnulacion | Motivo, máximo 500 caracteres |

Relaciones actuales:

- Empresa.
- CajaOrigen.
- CajaDestino.
- Usuario.
- TurnoCaja opcional.
- UsuarioAnulacion.
- MovimientoCaja.

---

# 3. Autorización

`TransferenciaCajaController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Sin embargo, el flujo operativo de creación está restringido a un Usuario que opere dentro de una Empresa concreta.

---

# 4. SuperAdmin y creación

SuperAdmin puede consultar transferencias globalmente y filtrarlas por Empresa.

Pero actualmente no puede crear TransferenciaCaja directamente.

GET Create informa:

```text
Para realizar una transferencia debe operar dentro de una empresa específica.
```

POST Create devuelve Forbid para SuperAdmin.

---

# 5. Multiempresa

AdminEmpresa sólo puede operar con:

```text
usuario.EmpresaId
```

CajaOrigen y CajaDestino deben pertenecer a esa misma Empresa.

No se permiten transferencias entre Empresas diferentes.

---

# 6. Cajas activas

CajaOrigen y CajaDestino deben:

```text
existir
pertenecer a usuario.EmpresaId
Estado == true
```

---

# 7. Origen y destino distintos

Debe cumplirse:

```text
CajaOrigenId != CajaDestinoId
```

No tiene sentido transferir dinero desde una Caja hacia sí misma.

---

# 8. Importe

El modelo exige:

```text
Importe >= 0.01
```

El importe máximo está limitado por el saldo disponible de CajaOrigen.

---

# 9. Motivo

Motivo es obligatorio.

Posee máximo:

```text
250 caracteres
```

Antes de persistir se aplica:

```text
Motivo = Motivo.Trim()
```

---

# 10. Turno de Caja Origen

Si:

```text
CajaOrigen.PermiteTurnos == true
```

el Usuario debe tener un TurnoCaja propio abierto para esa Caja.

Se valida:

```text
Turno.CajaId == CajaOrigen.Id
Turno.UsuarioAperturaId == usuario.Id
Turno.Estado == Abierto
```

---

# 11. Caja Destino y Turno

La CajaDestino no requiere un TurnoCaja asociado a la Transferencia.

El diseño actual considera que el turno pertenece solamente al lado operativo de salida.

Por eso:

```text
Movimiento TransferenciaSalida
    TurnoCajaId = turnoOrigen

Movimiento TransferenciaEntrada
    TurnoCajaId = null
```

---

# 12. Saldo disponible de origen

Antes de registrar se calcula:

```text
CajaSaldoService.CalcularSaldoDisponible(
    cajaOrigen,
    usuario.Id)
```

Debe cumplirse:

```text
Importe <= SaldoDisponibleOrigen
```

---

# 13. Consulta dinámica de saldo

El controller expone `GetSaldoCaja` para consultar el saldo disponible de una Caja válida dentro de la Empresa del Usuario.

SuperAdmin no utiliza este endpoint operativo.

La respuesta de UI es informativa; el saldo vuelve a validarse en POST.

---

# 14. Transacción Serializable

El registro utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se vuelve a calcular el saldo disponible de CajaOrigen.

Si cambió y ya no alcanza, la transferencia se rechaza.

---

# 15. Registro de TransferenciaCaja

Al registrar correctamente se persiste:

```text
EmpresaId
CajaOrigenId
CajaDestinoId
Importe
Fecha = DateTime.Now
Motivo
UsuarioId
TurnoCajaId = turno origen cuando aplica
Estado = Activa
```

Los campos de anulación comienzan en null.

---

# 16. Movimiento de salida

Se genera un MovimientoCaja para CajaOrigen:

```text
Tipo = TransferenciaSalida
Direccion = Egreso
Importe = transferencia.Importe
CajaId = CajaOrigenId
TurnoCajaId = turno origen cuando aplica
TransferenciaCajaId = transferencia.Id
```

No utiliza MedioPago ni CategoriaGasto.

---

# 17. Movimiento de entrada

Se genera otro MovimientoCaja para CajaDestino:

```text
Tipo = TransferenciaEntrada
Direccion = Ingreso
Importe = transferencia.Importe
CajaId = CajaDestinoId
TurnoCajaId = null
TransferenciaCajaId = transferencia.Id
```

Tampoco utiliza MedioPago ni CategoriaGasto.

---

# 18. Neutralidad financiera global

Los dos movimientos tienen el mismo importe y direcciones opuestas:

```text
Origen:  -$X
Destino: +$X
```

Por lo tanto:

```text
Saldo total Empresa antes == Saldo total Empresa después
```

---

# 19. Atomicidad

Dentro de la misma transacción se registran:

```text
TransferenciaCaja
MovimientoCaja TransferenciaSalida
MovimientoCaja TransferenciaEntrada
```

No debería existir normalmente una transferencia con sólo uno de los dos movimientos.

---

# 20. Index

El listado permite filtrar por:

- CajaOrigen.
- CajaDestino.
- Estado.
- FechaDesde.
- FechaHasta.
- Empresa para SuperAdmin.

---

# 21. Rango de fechas

FechaDesde se aplica desde el inicio del día.

FechaHasta utiliza límite exclusivo del día siguiente para incluir correctamente todo el día seleccionado.

---

# 22. Paginación

Index utiliza:

```text
20 registros por página
```

Ordenados por Fecha descendente.

---

# 23. Estado

TransferenciaCaja utiliza `EstadoTransferenciaCaja`.

Estados actuales:

```text
Activa
Anulada
```

La corrección se realiza mediante Anulación, no eliminación física.

---

# 24. Restricción por Turno cerrado

Si la Transferencia posee `TurnoCajaId` y ese Turno ya está:

```text
Estado == Cerrado
```

no se permite anular directamente la Transferencia.

El sistema indica que debe realizarse una corrección administrativa posterior.

---

# 25. Motivo de la restricción

Una transferencia asociada a un Turno cerrado ya formó parte del cierre histórico de esa Caja.

Modificarla después mediante una anulación automática alteraría retroactivamente la consistencia del Turno cerrado.

Por eso se preserva su inmutabilidad histórica.

---

# 26. Motivo de anulación

Para anular es obligatorio indicar un motivo.

Debe cumplir:

```text
no vacío
máximo 500 caracteres
```

Se normaliza con Trim.

---

# 27. Movimientos originales requeridos

La anulación requiere encontrar ambos movimientos originales:

```text
TransferenciaSalida
TransferenciaEntrada
```

Además deben ser movimientos originales:

```text
MovimientoOrigenId == null
```

Si falta alguno, la transferencia no puede anularse normalmente.

---

# 28. Prevención de doble reversión

Antes de anular se verifica que no existan movimientos que referencien como `MovimientoOrigenId` a cualquiera de los dos movimientos originales.

Si ya existen reversiones, se bloquea una nueva anulación.

---

# 29. Transacción de anulación

La anulación también utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- Estado de Transferencia.
- ausencia de reversiones concurrentes.
- saldo actual de CajaDestino.

---

# 30. Saldo requerido en CajaDestino

Anular una transferencia implica sacar nuevamente de CajaDestino el dinero recibido originalmente.

Por eso debe cumplirse:

```text
ImporteTransferencia <= SaldoDisponibleDestino
```

---

# 31. Cálculo de saldo para anulación

Actualmente, para la anulación se calcula el saldo de CajaDestino directamente sumando MovimientoCaja:

```text
Ingresos - Egresos
```

filtrados por Caja y Empresa.

No utiliza en ese punto `CajaSaldoService`.

---

# 32. Ejemplo de bloqueo por saldo

```text
Transferencia original: $50.000 hacia CajaDestino

Luego CajaDestino gasta $45.000
Saldo actual: $5.000
```

No puede anularse la transferencia de $50.000 porque para revertirla deberían salir $50.000 de CajaDestino.

---

# 33. Reversión de la salida original

La salida original de CajaOrigen fue:

```text
TransferenciaSalida
Direccion = Egreso
```

Al anular se crea:

```text
Tipo = ReversionTransferenciaSalida
Direccion = Ingreso
CajaId = CajaOrigenId
Importe = transferencia.Importe
MovimientoOrigenId = movimientoSalida.Id
```

El dinero vuelve conceptualmente a CajaOrigen.

---

# 34. Reversión de la entrada original

La entrada original en CajaDestino fue:

```text
TransferenciaEntrada
Direccion = Ingreso
```

Al anular se crea:

```text
Tipo = ReversionTransferenciaEntrada
Direccion = Egreso
CajaId = CajaDestinoId
Importe = transferencia.Importe
MovimientoOrigenId = movimientoEntrada.Id
```

El dinero sale conceptualmente de CajaDestino.

---

# 35. Turno en la reversión

La reversión de CajaOrigen utiliza:

```text
TurnoCajaId = transferencia.TurnoCajaId
```

La reversión de CajaDestino utiliza:

```text
TurnoCajaId = null
```

Esto conserva el criterio actual de que el Turno se asocia únicamente con el lado origen.

---

# 36. Estado al anular

Luego de generar ambas reversiones:

```text
Estado = Anulada
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
MotivoAnulacion = motivo
```

---

# 37. Trazabilidad

La transferencia conserva los movimientos originales y agrega movimientos inversos.

Conceptualmente:

```text
TransferenciaCaja
├── TransferenciaSalida
│   └── ReversionTransferenciaSalida
└── TransferenciaEntrada
    └── ReversionTransferenciaEntrada
```

---

# 38. No edición histórica

TransferenciaCaja no funciona como CRUD editable.

No debe modificarse libremente:

- CajaOrigen.
- CajaDestino.
- Importe.
- Fecha.
- Usuario.
- Turno.

Si fue incorrecta y todavía puede anularse:

```text
Anular transferencia
+
Registrar transferencia correcta
```

---

# 39. Seguridad

Las reglas críticas se validan en backend:

- Usuario autenticado.
- rol autorizado.
- SuperAdmin bloqueado para creación operativa.
- CajaOrigen y CajaDestino de la Empresa del Usuario.
- Cajas activas.
- origen distinto de destino.
- Turno propio abierto en CajaOrigen cuando corresponde.
- saldo suficiente en CajaOrigen.
- revalidación Serializable.
- ambos movimientos originales requeridos al anular.
- prevención de doble reversión.
- bloqueo si Turno original está cerrado.
- saldo suficiente en CajaDestino para anular.

---

# 40. Reglas de negocio actuales

1. TransferenciaCaja pertenece a una Empresa.
2. CajaOrigen y CajaDestino deben pertenecer a la misma Empresa.
3. CajaOrigen y CajaDestino deben estar activas.
4. CajaOrigen y CajaDestino deben ser distintas.
5. Importe debe ser mayor a cero.
6. Motivo es obligatorio y posee máximo 250 caracteres.
7. SuperAdmin no puede crear transferencias directamente en el flujo actual.
8. Si CajaOrigen utiliza Turnos, se requiere Turno propio abierto.
9. CajaDestino no recibe TurnoCajaId en la transferencia.
10. Importe no puede superar saldo disponible de CajaOrigen.
11. El saldo se revalida dentro de transacción Serializable.
12. Registrar genera un MovimientoCaja de Egreso en CajaOrigen.
13. Registrar genera un MovimientoCaja de Ingreso en CajaDestino.
14. Ambos movimientos tienen el mismo importe.
15. La transferencia es financieramente neutra a nivel Empresa.
16. Transferencia y movimientos se registran atómicamente.
17. No se elimina físicamente una transferencia histórica.
18. La corrección se realiza mediante Anulación.
19. Si el Turno original está cerrado no se permite anulación directa.
20. Motivo de anulación es obligatorio y máximo 500 caracteres.
21. La anulación requiere ambos movimientos originales.
22. No se permite doble reversión.
23. CajaDestino debe tener saldo suficiente para anular.
24. Anular crea ReversionTransferenciaSalida como Ingreso en CajaOrigen.
25. Anular crea ReversionTransferenciaEntrada como Egreso en CajaDestino.
26. Cada reversión referencia su MovimientoCaja original.
27. La transferencia queda Estado Anulada con Fecha, Usuario y Motivo.
28. AdminEmpresa opera únicamente dentro de su Empresa.

---

# 41. Evolución futura

Posibles mejoras futuras:

- flujo de aprobación para transferencias altas.
- transferencias pendientes/confirmadas entre responsables de Caja.
- recepción explícita por Usuario destino.
- comprobante interno de transferencia.
- conciliación con cuentas bancarias.
- transferencias entre futuras Sucursales.
- motivo tipificado.
- límites por Usuario/Rol.
- permisos granulares para transferir y anular.
- correcciones administrativas formalizadas para Turnos cerrados.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 42. Estado actual

✅ Transferencias entre Cajas de la misma Empresa implementadas.

✅ Validación origen/destino distintos implementada.

✅ Validación de saldo origen implementada.

✅ Integración con TurnoCaja de origen implementada.

✅ Movimiento TransferenciaSalida implementado.

✅ Movimiento TransferenciaEntrada implementado.

✅ Atomicidad mediante transacción Serializable implementada.

✅ Historial y filtros implementados.

✅ Paginación implementada.

✅ Anulación lógica implementada.

✅ Doble movimiento de reversión implementado.

✅ Control de saldo en CajaDestino al anular implementado.

✅ Prevención de doble reversión implementada.

✅ Bloqueo de anulación cuando el Turno original está cerrado implementado.

✅ Trazabilidad de Usuario/Fecha/Motivo implementada.

🚧 Flujo de aprobación pendiente.

🚧 Transferencias entre Sucursales pendientes hasta que exista Sucursal.

🚧 Permisos granulares pendientes.