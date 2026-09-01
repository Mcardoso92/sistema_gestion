# Módulo Movimiento de Caja

Última actualización: 01/09/2026

---

# 1. Objetivo

`MovimientoCaja` registra las variaciones financieras que afectan a una Caja dentro de Veltika.

Cada registro representa un ingreso o egreso y permite reconstruir el saldo financiero de la Caja a partir de su historial.

Los movimientos pueden originarse en operaciones comerciales, operaciones manuales, transferencias, diferencias de cierre o reversiones.

---

# 2. Principio general

La arquitectura actual utiliza:

```text
Caja -> recurso financiero
MovimientoCaja -> variación financiera
TurnoCaja -> período operativo cuando la Caja utiliza turnos
```

`Caja` no posee un `SaldoActual` persistido.

El saldo se obtiene a partir de los movimientos registrados.

---

# 3. Modelo actual

`MovimientoCaja` contiene:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| EmpresaId | Empresa propietaria |
| CajaId | Caja afectada |
| Tipo | Origen/tipo financiero |
| Direccion | Ingreso o Egreso |
| Importe | Monto |
| Fecha | Fecha y hora |
| UsuarioId | Usuario responsable |
| MedioPagoId | Medio de pago opcional |
| TurnoCajaId | Turno opcional |
| CategoriaGastoId | Categoría de gasto opcional |
| Concepto | Concepto opcional |
| Observaciones | Observaciones opcionales |
| MovimientoOrigenId | Movimiento revertido, cuando corresponde |
| CobroVentaId | Origen CobroVenta opcional |
| PagoProveedorId | Origen PagoProveedor opcional |
| ReintegroVentaId | Origen ReintegroVenta opcional |
| ReintegroProveedorId | Origen ReintegroProveedor opcional |
| TransferenciaCajaId | Origen TransferenciaCaja opcional |

No posee actualmente:

- SucursalId.
- SaldoAnterior.
- SaldoNuevo.
- CompraId directo.
- VentaId directo.
- Estado de Soft Delete.

---

# 4. Dirección

Todo movimiento posee una dirección financiera:

```text
Ingreso
Egreso
```

La dirección determina cómo impacta el movimiento en el saldo.

Conceptualmente:

```text
Ingreso -> +Importe
Egreso  -> -Importe
```

---

# 5. Tipos de movimiento actuales

`TipoMovimientoCaja` contiene actualmente 20 valores:

```text
CobroVenta
PagoProveedor
ReintegroVenta
ReintegroProveedor
IngresoManual
EgresoManual
TransferenciaEntrada
TransferenciaSalida
AjusteSobranteCaja
AjusteFaltanteCaja
ReversionCobroVenta
ReversionPagoProveedor
ReversionReintegroVenta
ReversionReintegroProveedor
ReversionIngresoManual
ReversionEgresoManual
ReversionAjusteSobranteCaja
ReversionAjusteFaltanteCaja
ReversionTransferenciaEntrada
ReversionTransferenciaSalida
```

Esta enumeración es la referencia actual para clasificar el origen financiero del movimiento.

---

# 6. Acceso

`MovimientoCajaController` posee actualmente:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede consultar movimientos de múltiples Empresas.

Puede utilizar el filtro por Empresa.

Actualmente no puede registrar directamente IngresoManual o EgresoManual desde esos endpoints: el flujo exige operar dentro de una Empresa específica y rechaza a SuperAdmin.

## AdminEmpresa

Puede consultar y registrar movimientos manuales únicamente dentro de su Empresa.

## Cajero

Actualmente no posee acceso mediante este controller porque el rol no está incluido en la autorización vigente.

---

# 7. Consulta e historial

El listado permite filtrar actualmente por:

- Caja.
- Medio de pago.
- Categoría de gasto.
- TurnoCaja.
- Usuario.
- Tipo.
- Dirección.
- Fecha desde.
- Fecha hasta.
- Empresa para SuperAdmin.

Los movimientos se ordenan por fecha descendente.

La pantalla utiliza paginación de:

```text
20 movimientos por página
```

---

# 8. Totales del período

El listado calcula:

```text
TotalIngresos
TotalEgresos
NetoPeriodo
```

Para estos totales se consideran movimientos vigentes y se excluyen conceptualmente los movimientos que fueron revertidos junto con las propias reversiones correspondientes.

Esto evita que una operación anulada continúe afectando los totales operativos del período.

---

# 9. Importe

El importe posee validación:

```text
Importe >= 0.01
```

No se utilizan importes negativos.

La dirección del movimiento determina si el importe suma o resta financieramente.

---

# 10. Fecha y usuario

Los movimientos generados por los flujos del sistema registran la fecha y el usuario desde servidor.

Conceptualmente:

```text
Fecha = DateTime.Now
UsuarioId = usuario autenticado
```

No debe confiarse en valores editables enviados por el navegador para estos datos de auditoría.

---

# 11. Seguridad multiempresa

Todo MovimientoCaja posee:

```text
EmpresaId
```

Los usuarios `AdminEmpresa` sólo pueden consultar y operar sobre movimientos de su propia Empresa.

Las Cajas, MediosPago, CategoriasGasto y Turnos utilizados deben pertenecer al mismo contexto empresarial según las reglas del flujo.

Los IDs enviados por el navegador deben validarse nuevamente en servidor.

---

# 12. Ingreso manual

El IngresoManual está implementado.

El usuario selecciona:

- Caja.
- Medio de pago.
- Importe.
- Concepto.
- Observaciones opcionales.

El sistema genera:

```text
Tipo = IngresoManual
Direccion = Ingreso
```

El Concepto se normaliza mediante `Trim()`.

Las Observaciones vacías se almacenan como `null` y, cuando existen, también se normalizan.

---

# 13. Egreso manual

El EgresoManual está implementado.

Además de los datos financieros generales, requiere una Categoría de Gasto válida.

El sistema genera:

```text
Tipo = EgresoManual
Direccion = Egreso
```

Antes de registrar el egreso se calcula el saldo disponible mediante `CajaSaldoService`.

Si:

```text
Importe > SaldoDisponible
```

el movimiento se rechaza.

Por lo tanto, el flujo manual actual no permite retirar más fondos que los disponibles según las reglas del servicio de saldo.

---

# 14. Caja válida

Para movimientos manuales, la Caja debe:

- Existir.
- Estar activa.
- Pertenecer a la Empresa del usuario.

No se utiliza una Caja de otra Empresa aunque se manipule el identificador enviado desde el formulario.

---

# 15. Medio de pago

En movimientos manuales, el MedioPago debe:

- Existir.
- Estar activo.
- Pertenecer a la misma Empresa.
- Estar asociado a la Caja mediante `CajaMedioPago`.

Por lo tanto, no cualquier MedioPago de la Empresa puede utilizarse arbitrariamente en cualquier Caja.

---

# 16. Categoría de gasto

El EgresoManual utiliza `CategoriaGastoId`.

La Categoría debe:

- Existir.
- Estar activa.
- Pertenecer a la Empresa del usuario.

Los ingresos manuales no utilizan CategoriaGasto.

---

# 17. Relación con TurnoCaja

`TurnoCajaId` es opcional porque no todas las Cajas requieren turnos.

Si:

```text
Caja.PermiteTurnos == true
```

el usuario debe poseer un turno propio abierto en esa Caja para registrar el movimiento manual.

La validación busca un TurnoCaja con:

```text
CajaId == caja.Id
UsuarioAperturaId == usuario.Id
Estado == Abierto
```

Si no existe, el movimiento manual se rechaza.

Para Cajas que no permiten turnos, el movimiento puede registrarse sin `TurnoCajaId`.

---

# 18. Cobros de Venta

Los cobros se integran financieramente mediante:

```text
CobroVentaId
TipoMovimientoCaja.CobroVenta
```

El movimiento representa el ingreso efectivo a la Caja correspondiente al cobro, no simplemente la existencia de una Venta.

Esta separación permite que una Venta pueda poseer distintos cobros y medios de pago según las reglas comerciales actuales.

---

# 19. Pagos a Proveedor

Los pagos se integran mediante:

```text
PagoProveedorId
TipoMovimientoCaja.PagoProveedor
```

El movimiento representa la salida financiera asociada al pago.

Por lo tanto, `MovimientoCaja` no necesita un `CompraId` directo para representar el egreso financiero de una Compra.

La relación comercial se obtiene a través de PagoProveedor.

---

# 20. Reintegros

El modelo contempla:

```text
ReintegroVentaId
ReintegroProveedorId
```

Y los tipos:

```text
ReintegroVenta
ReintegroProveedor
```

Esto permite diferenciar correctamente devoluciones financieras según el origen comercial.

También existen tipos específicos para sus reversiones.

---

# 21. Transferencias entre Cajas

Las transferencias están implementadas mediante:

```text
TransferenciaCajaId
TransferenciaEntrada
TransferenciaSalida
```

Una transferencia genera la trazabilidad financiera correspondiente en Caja origen y Caja destino.

También existen tipos:

```text
ReversionTransferenciaEntrada
ReversionTransferenciaSalida
```

para conservar el historial cuando la operación debe revertirse.

---

# 22. Diferencias de Caja

El sistema contempla movimientos:

```text
AjusteSobranteCaja
AjusteFaltanteCaja
```

Estos tipos permiten registrar financieramente diferencias detectadas en la operatoria de cierre/arqueo.

También existen:

```text
ReversionAjusteSobranteCaja
ReversionAjusteFaltanteCaja
```

para mantener una corrección trazable sin eliminar el movimiento original.

---

# 23. Reversiones

`MovimientoCaja` utiliza:

```text
MovimientoOrigenId
```

para relacionar una reversión con el movimiento original.

Además posee la navegación:

```text
MovimientoOrigen
Reversiones
```

El patrón actual evita editar o borrar el movimiento financiero histórico.

En su lugar se registra un movimiento inverso que referencia al original.

---

# 24. Reversión de movimientos manuales

La interfaz actual permite revertir movimientos manuales vigentes de tipo:

```text
IngresoManual
EgresoManual
```

siempre que no sean ya una reversión y no hayan sido revertidos previamente.

La corrección queda registrada mediante los tipos:

```text
ReversionIngresoManual
ReversionEgresoManual
```

El movimiento original permanece almacenado.

---

# 25. Inmutabilidad financiera

No existen acciones administrativas normales para modificar un MovimientoCaja confirmado.

No se utiliza eliminación física ni Soft Delete para corregir el historial.

La estrategia es:

```text
Movimiento original
+
Movimiento de reversión
```

Esto conserva la trazabilidad de la operación original y de su corrección.

---

# 26. Saldo

`MovimientoCaja` no almacena:

```text
SaldoAnterior
SaldoNuevo
```

como afirmaba la documentación anterior.

El saldo se obtiene de los movimientos financieros.

En términos generales:

```text
Saldo = SUM(Ingresos) - SUM(Egresos)
```

Los flujos que requieren disponibilidad operativa pueden aplicar reglas adicionales mediante `CajaSaldoService`, especialmente cuando existen turnos y fondo fijo.

---

# 27. Apertura y cierre

No existe un movimiento genérico obligatorio `AperturaCaja` o `CierreCaja` dentro del enum actual.

La apertura y cierre operativo se administra mediante `TurnoCaja`.

El fondo fijo, efectivo esperado, contado, diferencia e importe rendido pertenecen al turno.

Por lo tanto, no debe asumirse que abrir o cerrar un Turno genera necesariamente un tipo `Apertura de caja` o `Cierre de caja` en `TipoMovimientoCaja`.

---

# 28. Sucursales

Actualmente `MovimientoCaja` no posee:

```text
SucursalId
```

La seguridad y propiedad actual se resuelve mediante `EmpresaId` y `CajaId`.

La futura implementación de Sucursales requerirá definir cómo se relacionan Cajas y movimientos con cada ubicación.

---

# 29. Reglas de negocio

1. Todo MovimientoCaja pertenece a una Empresa y una Caja.
2. Todo movimiento posee Tipo y Dirección.
3. El Importe debe ser mayor a cero.
4. Los importes se almacenan positivos; la Dirección determina el signo financiero.
5. Fecha y Usuario se determinan desde el flujo de servidor.
6. El saldo de Caja no se almacena en MovimientoCaja como SaldoAnterior/SaldoNuevo.
7. Los movimientos forman la base para calcular el saldo.
8. AdminEmpresa sólo opera dentro de su Empresa.
9. SuperAdmin puede consultar múltiples Empresas.
10. SuperAdmin no registra actualmente movimientos manuales desde los endpoints IngresoManual/EgresoManual.
11. Cajero no está autorizado actualmente por MovimientoCajaController.
12. Una Caja manual debe estar activa y pertenecer a la Empresa.
13. El MedioPago debe estar activo, pertenecer a la Empresa y estar asociado a la Caja.
14. EgresoManual requiere CategoriaGasto válida.
15. Una Caja con turnos requiere turno propio abierto para el movimiento manual.
16. EgresoManual no puede superar el saldo disponible calculado.
17. Los movimientos comerciales conservan referencias a sus entidades financieras de origen.
18. Las transferencias generan movimientos específicos.
19. Las diferencias de Caja poseen tipos específicos.
20. Las correcciones se realizan mediante reversiones.
21. Un movimiento revertido no se elimina.
22. Una reversión referencia al movimiento original mediante MovimientoOrigenId.
23. Actualmente no existe SucursalId.
24. MovimientoCaja no utiliza Soft Delete para corregir operaciones.

---

# 30. Casos de error relevantes

- Usuario no autenticado.
- Usuario sin rol autorizado.
- Intento de movimiento manual por SuperAdmin.
- Caja inexistente.
- Caja inactiva.
- Caja de otra Empresa.
- MedioPago inválido.
- MedioPago no asociado a la Caja.
- Categoría de gasto inválida.
- Importe inválido.
- Egreso superior al saldo disponible.
- Falta de turno propio abierto en una Caja que requiere turnos.
- Intento de revertir un movimiento no permitido.
- Intento de revertir una reversión.
- Intento de revertir nuevamente un movimiento ya revertido.

---

# 31. Integraciones actuales

MovimientoCaja se integra con:

- Empresa.
- Caja.
- Usuario.
- MedioPago.
- TurnoCaja.
- CategoriaGasto.
- CobroVenta.
- PagoProveedor.
- ReintegroVenta.
- ReintegroProveedor.
- TransferenciaCaja.
- CajaSaldoService.

Actualmente no se integra directamente con Sucursal.

---

# 32. Capacidades futuras

Quedan para evolución posterior, entre otras:

- Conciliación bancaria formal.
- Conciliación automática con proveedores externos.
- Integración directa con Mercado Pago u otras billeteras.
- Exportación contable avanzada.
- Reglas de aprobación para determinados egresos.
- Permisos granulares por empleado.
- Alertas financieras configurables.
- Reportes históricos avanzados de diferencias.

---

# 33. Estado actual

✅ Ingresos y egresos implementados.

✅ Movimientos manuales implementados.

✅ Cobros de Venta integrados.

✅ Pagos a Proveedor integrados.

✅ Reintegros integrados.

✅ Transferencias entre Cajas integradas.

✅ Categorías de gasto integradas.

✅ Turnos de Caja integrados.

✅ Ajustes por sobrante/faltante contemplados.

✅ Reversiones financieras implementadas en el modelo y flujos correspondientes.

✅ Seguridad multiempresa implementada.

✅ Filtros y paginación implementados.

🚧 Conciliación bancaria, integraciones externas y automatización financiera quedan para evolución futura.