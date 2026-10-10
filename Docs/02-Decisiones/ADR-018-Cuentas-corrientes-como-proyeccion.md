# ADR-018: Cuentas corrientes como proyección de operaciones existentes

## Estado

Aceptada.

## Contexto

Veltika ya registra las operaciones que explican las deudas comerciales:
`Venta`, `CobroVenta`, `ReintegroVenta`, `Compra`, `PagoProveedor`,
`DevolucionCompra` y `ReintegroProveedor`. También centraliza los cálculos por
operación mediante `VentaSaldoService` y `CompraSaldoService`.

Persistir además un saldo mutable o crear movimientos propios de cuenta
corriente produciría una segunda fuente de verdad que podría desincronizarse de
las operaciones comerciales, sus anulaciones y sus movimientos de Caja.

## Decisión

La primera implementación de cuentas corrientes será una proyección de lectura
sobre las operaciones existentes. No se crearán tablas `CuentaCorriente`,
`SaldoCliente`, `SaldoProveedor` ni movimientos contables paralelos.

La cuenta corriente explica una deuda comercial. `MovimientoCaja` continúa
siendo la fuente de los movimientos financieros reales y no se utilizará para
recalcular la deuda cuando la operación comercial asociada ya contiene toda la
información necesaria.

## Cuenta corriente de clientes

- Sólo participan ventas con un `ClienteId` identificado.
- Consumidor Final queda excluido porque no representa una identidad sobre la
  cual mantener deuda.
- El saldo por venta se obtiene con `VentaSaldoService.ObtenerSaldoPendiente`.
- El saldo total del cliente es la suma de los saldos pendientes de sus ventas
  activas.
- Una venta activa incrementa la deuda y un cobro activo la reduce.
- Los cobros anulados no producen efecto.
- Una venta anulada no produce efecto, aunque puede mostrarse con ese estado
  para conservar el contexto histórico.
- `ReintegroVenta` puede mostrarse como operación informativa, pero no modifica
  el saldo pendiente: el modelo actual lo trata como devolución de dinero ya
  cobrado y no como un nuevo importe adeudado por el cliente.

Esta definición conserva exactamente la regla vigente de Venta y evita que el
estado de cuenta contradiga los importes mostrados en sus listados y detalles.

## Cuenta corriente de proveedores

El proveedor puede presentar dos posiciones diferentes y deben mostrarse por
separado:

- **Saldo a pagar:** suma de `CompraSaldoService.ObtenerSaldoPendiente` para
  compras activas.
- **Saldo a recuperar:** suma de
  `CompraSaldoService.ObtenerPendienteRecuperar` para compras activas.

Una compra activa incrementa la obligación. Una devolución activa reduce el
total neto de la compra. Un pago activo reduce el saldo a pagar y puede generar
un importe a recuperar cuando, después de una devolución, lo pagado supera al
total neto. Un reintegro activo del proveedor reduce ese importe a recuperar.

Compras, pagos, devoluciones y reintegros anulados no producen efecto en los
saldos vigentes, aunque pueden mostrarse identificados como anulados.

No se reemplazarán ambos importes por un único saldo absoluto: hacerlo ocultaría
si Veltika debe pagar al proveedor o si el proveedor debe reintegrar dinero.

## Estado de cuenta inicial

Los futuros estados de cuenta se construirán con ViewModels de lectura y un
servicio de consulta reutilizable. Cada movimiento proyectado podrá exponer:

- fecha;
- tipo de operación;
- referencia e identificador original;
- importe;
- efecto sobre el saldo;
- saldo resultante;
- estado vigente;
- usuario cuando corresponda;
- ruta hacia el detalle original.

Las filas anuladas podrán permanecer visibles con efecto cero. El saldo
resultante será una reconstrucción según el estado vigente, no una contabilidad
histórica «a fecha», porque algunos modelos actuales no conservan una operación
de reversión comercial independiente ni una fecha de anulación homogénea.

La vista inicial tendrá filtro por estado, orden cronológico estable y
paginación. Los filtros por fecha podrán incorporarse cuando el uso real los
justifique, definiendo también cómo presentar el saldo anterior al período.
Las consultas deben proyectar únicamente los campos requeridos y filtrar por
empresa antes de resolver cualquier identificador recibido.

## Seguridad multiempresa

- `AdminEmpresa` sólo consulta clientes, proveedores y operaciones de su
  `EmpresaId`.
- `SuperAdmin` mantiene la selección explícita de empresa utilizada en los
  módulos actuales.
- No se confía en `ClienteId`, `ProveedorId` ni referencias recibidas desde el
  navegador sin validar su pertenencia.
- Los enlaces a operaciones originales conservan las validaciones de acceso de
  sus controllers.

## Rendimiento

La primera estrategia será optimizar consultas de lectura, agregaciones,
proyecciones e índices antes de persistir información derivada. Sólo una
necesidad de rendimiento medida justificaría materializar saldos, acompañada de
una estrategia transaccional de sincronización y reconstrucción.

## Consecuencias

La arquitectura mantiene una única fuente de verdad y permite implementar por
separado las cuentas de clientes y proveedores sin alterar Caja, Venta, Compra,
stock ni los flujos de anulación actuales. La contrapartida es que el estado de
cuenta inicial describe la situación vigente y no pretende ser un libro mayor
contable ni reconstruir saldos históricos para cualquier fecha pasada.

No se requiere migración de base de datos para esta decisión.
