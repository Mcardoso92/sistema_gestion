# Módulo Caja

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Caja permite administrar las cuentas o destinos financieros operativos de cada Empresa dentro de Veltika.

Una Caja puede representar actualmente:

- Efectivo.
- Banco.
- Billetera virtual.
- Otro destino financiero.

Las Cajas se relacionan con medios de pago, movimientos financieros, transferencias y, cuando corresponde, turnos operativos de efectivo.

---

# 2. Concepto actual

En la implementación vigente, `Caja` no representa una apertura/cierre de jornada.

La entidad `Caja` representa el recurso financiero permanente.

La apertura y cierre operativo se modela mediante:

```text
TurnoCaja
```

Por lo tanto:

```text
Caja      -> recurso financiero
TurnoCaja -> período operativo de apertura/cierre
```

Esta separación es fundamental para comprender el diseño actual.

---

# 3. Modelo Caja

La entidad contiene actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| Nombre | Nombre de la Caja |
| Tipo | Tipo financiero |
| PermiteTurnos | Indica si utiliza TurnoCaja |
| FondoFijo | Fondo fijo configurado |
| Estado | Activa/Inactiva |
| FechaAlta | Fecha de creación |
| EmpresaId | Empresa propietaria |

Relaciones actuales:

- Empresa.
- CajaMediosPago.
- TurnosCaja.
- MovimientosCaja.
- TransferenciasOrigen.
- TransferenciasDestino.

No posee actualmente:

- SucursalId.
- UsuarioId responsable permanente.
- FechaApertura.
- FechaCierre.
- SaldoInicial.
- SaldoActual persistido.

---

# 4. Tipos de Caja

`TipoCaja` contiene actualmente:

```text
Efectivo = 1
Banco = 2
BilleteraVirtual = 3
Otro = 4
```

El tipo permite distinguir el comportamiento financiero esperado de la Caja.

---

# 5. Acceso y permisos

`CajaController` posee actualmente:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede administrar Cajas de múltiples Empresas y utilizar el filtro de Empresa.

## AdminEmpresa

Puede administrar únicamente las Cajas de su propia Empresa.

Actualmente `Cajero` no posee acceso directo al CRUD administrativo de Caja.

La operación cotidiana de apertura/cierre corresponde al módulo `TurnoCaja`, con sus propias reglas de autorización.

---

# 6. Listado

El listado permite actualmente:

- Buscar por Nombre.
- Filtrar por Estado.
- Filtrar por Tipo.
- Filtrar por Empresa para SuperAdmin.
- Visualizar saldo actual calculado.
- Visualizar FondoFijo.
- Visualizar si permite turnos.

Por defecto se muestran Cajas activas.

La paginación utiliza:

```text
20 registros por página
```

---

# 7. Saldo actual

`Caja` no almacena un campo persistido `SaldoActual`.

El saldo se calcula a partir de `MovimientoCaja`:

```text
Ingresos - Egresos
```

Conceptualmente:

```text
SaldoActual = SUM(Ingresos) - SUM(Egresos)
```

Por lo tanto, los movimientos financieros constituyen la base para justificar el saldo mostrado.

---

# 8. Creación

Para crear una Caja se define:

- Nombre.
- Empresa.
- Tipo.
- Si permite turnos.
- Fondo fijo.
- Medios de pago asociados.

Al crearse:

```text
Estado = true
FechaAlta = DateTime.Now
```

La creación de Caja y sus relaciones con medios de pago se realiza dentro de una transacción.

---

# 9. Nombre

El Nombre es obligatorio y admite hasta 100 caracteres.

Antes de persistirse se normaliza mediante `Trim()`.

No puede existir otra Caja activa con el mismo Nombre dentro de la misma Empresa.

La unicidad se aplica al contexto de Empresa y a Cajas activas.

---

# 10. Empresa

Toda Caja pertenece a una Empresa mediante:

```text
EmpresaId
```

La Empresa debe existir y estar activa.

Para `AdminEmpresa`, el EmpresaId se determina desde el usuario autenticado y no se confía en un valor enviado por el navegador.

`SuperAdmin` puede seleccionar la Empresa en los flujos administrativos correspondientes.

---

# 11. Fondo fijo

`Caja` posee:

```text
FondoFijo
```

con validación:

```text
FondoFijo >= 0
```

El fondo fijo se utiliza especialmente en la operatoria de TurnoCaja.

Cuando se abre un turno, el modelo `TurnoCaja` conserva el valor aplicado en:

```text
FondoFijoAplicado
```

Esto permite mantener el valor histórico utilizado durante ese turno aunque posteriormente cambie la configuración de la Caja.

---

# 12. Turnos

Los turnos ya están implementados.

Una Caja puede indicar:

```text
PermiteTurnos = true/false
```

Actualmente sólo una Caja de tipo:

```text
Efectivo
```

puede permitir turnos.

Intentar configurar turnos en una Caja Banco, BilleteraVirtual u Otro es inválido.

---

# 13. TurnoCaja

`TurnoCaja` registra actualmente:

- EmpresaId.
- CajaId.
- UsuarioAperturaId.
- FechaApertura.
- Estado.
- FechaCierre.
- UsuarioCierreId.
- CierreForzado.
- MotivoCierreForzado.
- FondoFijoAplicado.
- EfectivoEsperado.
- EfectivoContado.
- Diferencia.
- ImporteRendido.

Por lo tanto, apertura, cierre, arqueo y diferencia pertenecen conceptualmente a `TurnoCaja`, no a la entidad `Caja`.

---

# 14. Arqueo y diferencia

El arqueo ya está implementado mediante los campos de TurnoCaja:

```text
EfectivoEsperado
EfectivoContado
Diferencia
ImporteRendido
```

Por lo tanto, no debe documentarse como una funcionalidad futura genérica.

La diferencia permite conservar el resultado entre el efectivo esperado por el sistema y el efectivo contado al cierre.

---

# 15. Medios de pago

Una Caja puede asociarse a uno o más MediosPago mediante:

```text
CajaMedioPago
```

Los medios seleccionados deben:

- Existir.
- Estar activos.
- Pertenecer a la misma Empresa.
- Ser compatibles con el Tipo de Caja.

La compatibilidad se valida mediante:

```text
CompatibilidadFinanciera.EsCompatible(...)
```

Los IDs enviados desde el formulario son validados en servidor.

---

# 16. Estado y baja lógica

Caja posee:

```text
Estado
```

El flujo administrativo utiliza activación/inactivación en lugar de eliminar físicamente el historial financiero.

Una Caja inactiva permanece registrada y conserva sus relaciones históricas.

La reactivación debe respetar nuevamente las reglas de negocio y consistencia aplicables.

---

# 17. Edición

La edición permite administrar la configuración de la Caja sin alterar su historial financiero previo.

Entre los datos configurables se encuentran:

- Nombre.
- Tipo.
- PermiteTurnos.
- FondoFijo.
- Estado.
- Medios de pago asociados.

La edición debe mantener la seguridad multiempresa y la compatibilidad financiera de los medios seleccionados.

---

# 18. Movimientos financieros

Los ingresos y egresos no se almacenan como campos acumulados dentro de Caja.

Se registran mediante:

```text
MovimientoCaja
```

Cada movimiento referencia la Caja correspondiente.

El saldo actual se obtiene a partir de esos movimientos.

Las reglas detalladas se documentan en `18-MovimientoCaja.md`.

---

# 19. Transferencias

Caja ya posee integración con transferencias financieras mediante:

```text
TransferenciasOrigen
TransferenciasDestino
```

Una transferencia permite mover fondos entre Cajas según las reglas del módulo `TransferenciaCaja`.

Por lo tanto, la transferencia entre destinos financieros no debe considerarse una capacidad inexistente del modelo actual.

---

# 20. Seguridad multiempresa

Las consultas administrativas se filtran por:

```text
Caja.EmpresaId
```

Para usuarios que no son SuperAdmin:

```text
Caja.EmpresaId == usuario.EmpresaId
```

Además:

- Los medios de pago deben pertenecer a la misma Empresa.
- Las Empresas seleccionadas deben existir y estar activas.
- No debe confiarse en IDs enviados por el cliente para autorizar acceso entre tenants.

---

# 21. Sucursales

Actualmente Caja no posee:

```text
SucursalId
```

No existe una relación productiva Caja → Sucursal.

El alcance actual es por Empresa.

Cuando se implemente multi-sucursal deberá definirse expresamente si una Caja pertenece a una Sucursal, si puede ser compartida o si determinadas Cajas financieras operan a nivel Empresa.

No debe asumirse ese diseño antes de implementar el módulo Sucursal.

---

# 22. Reglas de negocio

1. Caja representa un recurso financiero permanente, no una jornada de apertura/cierre.
2. Toda Caja pertenece a una Empresa.
3. Actualmente no pertenece a una Sucursal.
4. El Nombre es obligatorio y admite hasta 100 caracteres.
5. No puede existir otra Caja activa con el mismo Nombre dentro de la Empresa.
6. Los tipos actuales son Efectivo, Banco, BilleteraVirtual y Otro.
7. FondoFijo no puede ser negativo.
8. Sólo las Cajas Efectivo pueden permitir Turnos.
9. La apertura/cierre operativo se administra mediante TurnoCaja.
10. El arqueo y la diferencia están implementados en TurnoCaja.
11. Caja no almacena SaldoActual persistido.
12. El saldo se calcula mediante MovimientoCaja.
13. Los medios de pago asociados deben pertenecer a la misma Empresa.
14. Los medios de pago deben ser compatibles con el TipoCaja.
15. Caja utiliza Estado para activación/inactivación.
16. El historial financiero no debe eliminarse al inactivar una Caja.
17. SuperAdmin puede administrar múltiples Empresas.
18. AdminEmpresa sólo administra Cajas de su Empresa.
19. Cajero no posee actualmente acceso al CRUD administrativo de Caja.
20. Las transferencias entre Cajas ya forman parte del modelo actual.

---

# 23. Casos de error relevantes

- Usuario no autenticado.
- Usuario sin rol autorizado.
- Empresa inexistente o inactiva.
- Intento de acceso a Caja de otra Empresa.
- Nombre duplicado dentro de la Empresa.
- TipoCaja inválido.
- Fondo fijo negativo.
- Intentar habilitar turnos en una Caja que no sea Efectivo.
- Medio de pago inexistente.
- Medio de pago inactivo.
- Medio de pago de otra Empresa.
- Medio de pago incompatible con el TipoCaja.

---

# 24. Integraciones actuales

Caja se integra actualmente con:

- Empresa.
- MedioPago.
- CajaMedioPago.
- TurnoCaja.
- MovimientoCaja.
- TransferenciaCaja.
- Cobros de Venta.
- Pagos a Proveedor.
- Reintegros y otros movimientos financieros según su flujo.

Actualmente no se integra con Sucursal.

---

# 25. Capacidades no implementadas o pendientes de evolución

Entre las capacidades que todavía requieren evolución se encuentran:

- Sucursales.
- Caja por Sucursal.
- Conciliación bancaria formal.
- Conciliación automática con proveedores de pago.
- Integraciones directas con terminales físicas POS.
- Integración financiera automática con Mercado Pago.
- Políticas más granulares por empleado.
- Indicadores históricos avanzados de diferencias de Caja.

---

# 26. Estado actual

✅ CRUD administrativo de Cajas implementado.

✅ Tipos Efectivo/Banco/BilleteraVirtual/Otro implementados.

✅ Fondo fijo implementado.

✅ Asociación Caja ↔ MedioPago implementada.

✅ Compatibilidad financiera implementada.

✅ Turnos de Caja implementados.

✅ Apertura y cierre mediante TurnoCaja implementados.

✅ Arqueo, efectivo esperado/contado y diferencia implementados.

✅ Movimientos de Caja implementados.

✅ Saldo calculado desde movimientos implementado.

✅ Transferencias entre Cajas implementadas.

✅ Seguridad multiempresa implementada.

🚧 Sucursales, conciliación bancaria e integraciones financieras externas quedan para evolución futura.