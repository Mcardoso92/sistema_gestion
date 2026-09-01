# Módulo Venta

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Venta registra las operaciones comerciales de salida realizadas por cada empresa dentro de Veltika.

Una venta puede incluir uno o varios productos, un cliente opcional, uno o varios cobros y los movimientos de stock y caja asociados.

El proceso debe preservar consistencia transaccional, trazabilidad de inventario, trazabilidad financiera y aislamiento multiempresa.

---

# 2. Alcance actual

Actualmente permite:

- Listar ventas.
- Buscar ventas.
- Filtrar por fecha, usuario, estado, estado de cobro y empresa para `SuperAdmin`.
- Registrar ventas desde el Punto de Venta.
- Vender a cliente registrado o a cliente ocasional.
- Registrar uno o múltiples medios de pago.
- Registrar ventas totalmente cobradas.
- Registrar ventas con pago parcial.
- Registrar ventas completamente a cuenta.
- Descontar stock automáticamente.
- Generar movimientos de stock.
- Generar cobros de venta.
- Generar movimientos de caja.
- Consultar detalle completo de venta.
- Consultar cobros y reintegros asociados.
- Anular ventas bajo determinadas condiciones.
- Restaurar stock cuando una venta se anula.
- Buscar productos por nombre o código de barras desde el POS.
- Buscar clientes desde el POS.

---

# 3. Actores y permisos

El controller está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto, el flujo actual de Venta no está habilitado mediante roles separados de `Cajero` o `Vendedor`.

## SuperAdmin

Puede:

- Consultar ventas de todas las empresas.
- Filtrar por empresa.
- Ingresar al POS seleccionando una empresa activa.
- Registrar ventas para la empresa seleccionada.
- Consultar detalle.
- Anular ventas si cumplen las reglas vigentes.

## AdminEmpresa

Puede:

- Consultar ventas de su empresa.
- Registrar ventas para su empresa.
- Consultar detalle.
- Anular ventas si cumplen las reglas vigentes.

Para `AdminEmpresa`, la empresa de la venta se determina siempre desde el usuario autenticado.

---

# 4. Modelo actual

La entidad `Venta` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| Fecha | DateTime | Fecha y hora de la venta |
| Total | decimal | Total calculado en servidor |
| Estado | bool | Activa o anulada |
| EmpresaId | int | Empresa propietaria |
| UsuarioId | string | Usuario que registró la venta |
| ClienteId | int? | Cliente opcional |

Relaciones:

- Empresa.
- Usuario.
- Cliente opcional.
- Detalles de venta.
- Movimientos de stock.
- Cobros de venta.
- Reintegros de venta.

Actualmente `Venta` no posee `SucursalId`, `CajaId` ni `TurnoCajaId` directos.

Caja y turno se registran en los cobros y movimientos financieros asociados.

---

# 5. Listado y filtros

El listado utiliza `VentaIndexVM` y permite filtrar por:

- Empresa para `SuperAdmin`.
- Texto de búsqueda.
- Fecha desde.
- Fecha hasta.
- Usuario.
- Estado de venta.
- Estado de cobro.

La búsqueda admite:

- ID de venta.
- Nombre del cliente.
- Apellido del cliente.
- Documento del cliente.

Las ventas se ordenan por fecha descendente y luego por ID descendente.

La paginación actual utiliza 20 registros por página.

El listado calcula además:

- Cantidad de ventas activas con saldo pendiente.
- Total pendiente de cobro.
- Total cobrado por venta.
- Total de unidades por venta.

---

# 6. Estados de cobro

El estado de cobro no se guarda como un campo fijo de Venta.

Se deriva del total de cobros activos asociados.

## Cobrada

```text
TotalCobrado >= TotalVenta
```

## Pago parcial

```text
TotalCobrado > 0
&& TotalCobrado < TotalVenta
```

## Pendiente

```text
TotalCobrado <= 0
```

Esto permite que el estado financiero se determine a partir de los cobros vigentes y no mediante un valor duplicado en Venta.

---

# 7. Inicio del Punto de Venta

Para `AdminEmpresa`, la empresa se obtiene desde el usuario autenticado.

Para `SuperAdmin`, debe seleccionarse explícitamente una empresa antes de ingresar al POS.

La empresa debe existir y encontrarse activa.

El POS inicia por defecto con:

```text
ClienteId = null
ClienteNombre = "Cliente ocasional"
```

---

# 8. Cliente opcional

Una venta puede realizarse sin cliente registrado.

En ese caso se considera una venta a:

```text
Cliente ocasional
```

Si se selecciona cliente, el servidor valida que:

- Exista.
- Se encuentre activo.
- Pertenezca a la misma empresa de la venta.

## Saldo pendiente

Una venta sin cliente registrado debe quedar totalmente cobrada.

Si:

```text
TotalPagado < TotalVenta
```

debe existir un `ClienteId` válido.

Por lo tanto, sólo un cliente identificado puede dejar saldo pendiente.

---

# 9. Detalles de venta

La venta debe contener al menos un producto.

Cada línea debe tener:

- ProductoId válido.
- Cantidad mayor a 0.

Aunque la interfaz evita normalmente agregar el mismo producto más de una vez, el servidor no confía en JavaScript.

Antes de procesar la venta agrupa líneas duplicadas por `ProductoId` y suma sus cantidades.

Esto evita manipulación del POST y simplifica la validación de stock.

---

# 10. Validación de productos

Todos los productos deben:

- Existir.
- Estar activos.
- Pertenecer a la empresa de la venta.

El precio utilizado se obtiene siempre desde el producto almacenado en servidor:

```text
producto.PrecioVenta
```

No se confía en precios enviados desde la interfaz.

El total se recalcula completamente en servidor.

---

# 11. Control de stock

Antes de confirmar se valida por cada producto:

```text
producto.Stock >= CantidadSolicitada
```

Si algún producto no posee stock suficiente, la venta completa se rechaza.

Al confirmar:

```text
StockPosterior = StockAnterior - CantidadVendida
```

y se genera un `MovimientoStock` con tipo:

```text
Venta
```

registrando:

- Producto.
- Empresa.
- Cantidad.
- Stock anterior.
- Stock posterior.
- Fecha.
- Usuario.

---

# 12. Total de venta

El total se calcula exclusivamente en servidor:

```text
TotalVenta = Σ (Producto.PrecioVenta × Cantidad)
```

Cada `DetalleVenta` conserva:

- PrecioUnitario utilizado en el momento de la venta.
- Cantidad.
- Subtotal.

De esta forma una modificación futura del precio del producto no altera ventas históricas.

---

# 13. Pagos múltiples

Una venta puede tener cero, uno o múltiples pagos.

Cada pago especifica:

- Caja.
- Medio de pago.
- Importe.

Los pagos vacíos se descartan antes de procesar.

Cada importe informado debe ser mayor a 0.

El total pagado no puede superar el total de la venta.

Actualmente no se registra sobrepago para calcular vuelto como parte del modelo persistido del cobro.

---

# 14. Validación de caja y medio de pago

Cada pago debe utilizar:

- Una caja activa.
- De la misma empresa.
- Un medio de pago activo.
- Habilitado específicamente para esa caja mediante `CajaMediosPago`.

El servidor valida la relación Caja-MedioPago y no confía únicamente en las opciones mostradas por la vista.

---

# 15. Turnos de caja

Si una caja posee:

```text
PermiteTurnos = true
```

el usuario debe tener un turno propio abierto en esa misma caja para poder registrar el cobro.

Si la caja no requiere turnos, el cobro puede existir con:

```text
TurnoCajaId = null
```

El turno queda asociado al cobro y al movimiento de caja, no directamente a la Venta.

---

# 16. Cobros de venta

Por cada pago válido se crea un `CobroVenta` con:

- Venta.
- Empresa.
- Caja.
- Medio de pago.
- Turno de caja cuando corresponde.
- Usuario.
- Fecha.
- Importe.
- Estado activo.

Los cobros representan la situación financiera real de la venta.

Una venta puede existir:

- Sin cobros.
- Con cobro parcial.
- Con cobro total.
- Con múltiples cobros.

---

# 17. Movimientos de caja

Por cada `CobroVenta` se genera un `MovimientoCaja` con:

```text
Tipo = CobroVenta
Direccion = Ingreso
```

El movimiento conserva:

- Empresa.
- Caja.
- Medio de pago.
- Turno cuando corresponde.
- Importe.
- Usuario.
- Fecha.
- Referencia al cobro.

El concepto generado identifica la venta:

```text
Cobro de venta #<VentaId>
```

---

# 18. Consistencia transaccional

La creación utiliza una transacción de base de datos con aislamiento:

```text
Serializable
```

Dentro de la misma operación se coordinan:

1. Validaciones definitivas.
2. Venta.
3. Detalles.
4. Actualización de stock.
5. Movimientos de stock.
6. Cobros.
7. Movimientos de caja.

Si ocurre un error antes de confirmar la transacción, la venta completa se revierte.

Esto evita ventas registradas sin stock actualizado o cobros sin su venta correspondiente.

---

# 19. Resultado según cobro

Al finalizar correctamente, el sistema distingue:

## Totalmente cobrada

```text
TotalPagado == TotalVenta
```

## A cuenta

```text
TotalPagado == 0
```

Requiere cliente identificado.

## Pago parcial

```text
0 < TotalPagado < TotalVenta
```

También requiere cliente identificado.

---

# 20. Detalle de venta

La vista de detalle incluye actualmente:

- Fecha.
- Total.
- Estado.
- Empresa.
- Usuario.
- Cliente o cliente ocasional.
- Documento y email del cliente cuando existen.
- Productos.
- Código de barras.
- Cantidad.
- Precio unitario histórico.
- Subtotal.
- Total cobrado.
- Cobros asociados.
- Total reintegrado.
- Importe pendiente de reintegrar.
- Reintegros asociados.

---

# 21. Anulación de venta

Una venta no se elimina físicamente.

La anulación establece:

```text
Estado = false
```

pero únicamente puede realizarse si se cumplen todas las condiciones.

## Restricciones

No puede anularse una venta si:

- Ya está anulada.
- Posee cobros activos.
- Posee reintegros activos.

Los cobros activos deben anularse antes de anular la venta.

Los reintegros activos también deben resolverse previamente.

---

# 22. Restauración de stock al anular

Cuando la venta puede anularse, se restaura el stock de cada producto:

```text
StockPosterior = StockAnterior + CantidadVendida
```

Además se genera un `MovimientoStock` con tipo:

```text
AnulacionVenta
```

registrando la trazabilidad completa.

La anulación también utiliza una transacción `Serializable`.

---

# 23. Búsqueda de productos en POS

El endpoint de búsqueda devuelve únicamente productos:

- Activos.
- De la empresa de la venta.

Permite buscar por:

- Nombre.
- Código de barras.

La consulta devuelve como máximo 10 resultados por búsqueda e incluye:

- Id.
- Nombre.
- Código de barras.
- Precio de venta.
- Stock disponible.

---

# 24. Búsqueda de clientes en POS

La búsqueda devuelve únicamente clientes:

- Activos.
- De la empresa de la venta.

Permite buscar por:

- Nombre.
- Apellido.
- Documento.
- Email.

Devuelve como máximo 10 resultados por búsqueda.

---

# 25. Seguridad multiempresa

Toda operación debe respetar `EmpresaId`.

Para `AdminEmpresa`, la empresa se deriva exclusivamente del usuario autenticado.

Productos, clientes, cajas y medios de pago son validados en servidor contra la empresa de la venta.

Las consultas de listado, detalle y anulación también se restringen por empresa.

Un ID válido de otra empresa nunca debe ser suficiente para acceder o asociar información cross-tenant.

---

# 26. Reglas de negocio

1. Cada venta pertenece a una única empresa.
2. Actualmente no existe relación con Sucursal.
3. Cada venta pertenece al usuario que la registró.
4. El cliente es opcional cuando la venta queda totalmente cobrada.
5. Para dejar saldo pendiente debe existir un cliente activo de la misma empresa.
6. La venta debe contener al menos un producto.
7. Las cantidades deben ser mayores a 0.
8. Los productos repetidos se consolidan en servidor.
9. Todos los productos deben estar activos y pertenecer a la empresa.
10. Debe existir stock suficiente antes de confirmar.
11. Los precios y el total se recalculan en servidor.
12. El precio unitario histórico se conserva en `DetalleVenta`.
13. El total pagado no puede superar el total de la venta.
14. Cada pago debe usar una caja y medio de pago válidos de la empresa.
15. Si la caja requiere turnos, el usuario debe tener un turno propio abierto en esa caja.
16. Cada cobro genera un movimiento de caja de ingreso.
17. Cada producto vendido genera movimiento de stock.
18. La creación es transaccional.
19. La venta no se elimina físicamente.
20. Una venta con cobros activos no puede anularse.
21. Una venta con reintegros activos no puede anularse.
22. La anulación restaura stock y genera movimientos de anulación.

---

# 27. Casos de error relevantes

- Venta sin productos.
- Producto o cantidad inválida.
- Producto inexistente.
- Producto inactivo.
- Producto perteneciente a otra empresa.
- Stock insuficiente.
- Cliente inexistente.
- Cliente inactivo.
- Cliente de otra empresa.
- Saldo pendiente sin cliente identificado.
- Pago con importe menor o igual a 0.
- Total pagado superior al total de venta.
- Caja inexistente, inactiva o de otra empresa.
- Medio de pago inválido para la caja.
- Falta de turno abierto cuando la caja lo requiere.
- Error de base de datos durante creación.
- Venta inexistente.
- Venta ya anulada.
- Venta con cobros activos.
- Venta con reintegros activos.

---

# 28. Integraciones actuales

Venta se integra con:

- Empresa.
- Usuario.
- Cliente.
- Producto.
- DetalleVenta.
- MovimientoStock.
- Caja.
- MedioPago.
- TurnoCaja.
- CobroVenta.
- MovimientoCaja.
- ReintegroVenta.
- Dashboard.
- Reportes.

Actualmente no existe integración con una entidad Sucursal.

---

# 29. Capacidades no implementadas

Actualmente no forman parte del módulo:

- Facturación electrónica ARCA.
- Número fiscal de comprobante.
- Tipos fiscales de comprobante.
- Notas de crédito fiscales.
- Notas de débito.
- Presupuestos convertibles a venta.
- Pedidos de clientes.
- Ventas suspendidas.
- Promociones automáticas.
- Motor de descuentos.
- Listas de precios.
- Cuotas como estructura financiera propia.
- Integración directa con Mercado Pago.
- Sucursal.
- Moneda y cotización por venta.
- API pública de ventas.

El POS ya existe; no debe considerarse una funcionalidad futura.

Los múltiples medios de pago también están implementados.

---

# 30. Evolución futura

La evolución se administra mediante Roadmap y GitHub Issues.

Entre las mejoras posibles se encuentran:

- Favoritos en POS.
- Ventas suspendidas.
- Descuentos.
- Promociones.
- Ticket digital.
- Presupuestos convertibles.
- Pedidos de clientes.
- Facturación electrónica.
- Mercado Pago.
- Menor cantidad de clics y mejoras de UX.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 31. Estado

✅ Punto de Venta implementado.

✅ Cliente opcional implementado.

✅ Venta a cuenta y pago parcial implementados.

✅ Múltiples medios de pago implementados.

✅ Integración con cajas y turnos implementada.

✅ Control y trazabilidad de stock implementados.

✅ Cobros y movimientos de caja implementados.

✅ Anulación con restauración de stock implementada.

✅ Reintegros integrados al detalle de venta.

✅ Seguridad multiempresa implementada.

✅ Búsqueda, filtros y paginación implementados.

🚧 Facturación electrónica y capacidades comerciales avanzadas reservadas para evolución post-MVP.