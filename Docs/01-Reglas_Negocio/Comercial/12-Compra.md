# Módulo Compra

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Compra registra adquisiciones realizadas a proveedores dentro de Veltika.

Una compra representa el ingreso comercial de mercadería y conserva los productos adquiridos, cantidades, costos históricos, comprobante, proveedor y usuario responsable.

Al confirmarse, incrementa el stock, genera trazabilidad de inventario y actualiza el costo actual de los productos involucrados.

El registro de la Compra y el pago al proveedor son operaciones separadas.

---

# 2. Alcance actual

Actualmente permite:

- Listar compras.
- Buscar compras.
- Filtrar por estado.
- Filtrar por proveedor.
- Filtrar por rango de fechas.
- Filtrar por empresa para `SuperAdmin`.
- Registrar compras.
- Registrar tipo y número de comprobante opcionales.
- Registrar observaciones.
- Incorporar múltiples productos.
- Incrementar stock automáticamente.
- Generar movimientos de stock.
- Actualizar el precio de costo actual del producto.
- Actualizar opcionalmente el precio de venta desde la compra.
- Consultar el detalle histórico.
- Consultar pagos al proveedor.
- Consultar saldo pendiente.
- Consultar reintegros del proveedor.
- Consultar devoluciones de compra.
- Anular compras bajo determinadas condiciones.
- Revertir stock al anular.
- Restaurar costos/precios anteriores cuando corresponde y no existen cambios posteriores.

---

# 3. Actores y permisos

El controller está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Actualmente no existe un rol separado de `Responsable de Compras` autorizado directamente sobre este controller.

## SuperAdmin

Puede:

- Consultar compras de todas las empresas.
- Filtrar por empresa.
- Registrar compras para una empresa activa seleccionada.
- Consultar detalle.
- Anular compras cuando cumplen las reglas vigentes.

## AdminEmpresa

Puede:

- Consultar compras de su empresa.
- Registrar compras para su empresa.
- Consultar detalle.
- Anular compras de su empresa cuando cumplen las reglas vigentes.

Para `AdminEmpresa`, `EmpresaId` se obtiene del usuario autenticado.

---

# 4. Modelo actual

La entidad `Compra` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| Fecha | DateTime | Fecha y hora de la compra |
| TipoComprobante | string? | Opcional, máximo 30 caracteres |
| NumeroComprobante | string? | Opcional, máximo 50 caracteres |
| Total | decimal | Total calculado de la compra |
| Estado | bool | Activa o anulada |
| Observaciones | string? | Opcional, máximo 500 caracteres |
| FechaAnulacion | DateTime? | Fecha de anulación cuando corresponde |
| EmpresaId | int | Empresa propietaria |
| ProveedorId | int | Proveedor asociado |
| UsuarioId | string | Usuario que registró la compra |
| UsuarioAnulacionId | string? | Usuario que anuló la compra |

Relaciones:

- Empresa.
- Proveedor.
- Usuario de creación.
- Usuario de anulación.
- Detalles de compra.
- Movimientos de stock.
- Pagos al proveedor.
- Reintegros del proveedor.
- Devoluciones de compra.

Actualmente `Compra` no posee `SucursalId`.

---

# 5. Listado y filtros

El listado utiliza `CompraIndexVM`.

Por defecto muestra compras activas.

Permite filtrar por:

- Activas.
- Anuladas.
- Todas.
- Proveedor.
- Fecha desde.
- Fecha hasta.
- Empresa para `SuperAdmin`.

La búsqueda permite coincidencias por:

- ID de compra cuando el texto es numérico.
- Número de comprobante.
- Tipo de comprobante.

Las compras se ordenan por fecha descendente y luego por ID descendente.

La paginación actual utiliza 20 registros por página.

---

# 6. Inicio de creación

Para `AdminEmpresa`, la empresa se obtiene desde el usuario autenticado.

Para `SuperAdmin`, debe seleccionarse una empresa activa.

La empresa validada será utilizada como límite para:

- Proveedor.
- Productos.
- Compra.
- Movimientos de stock.

---

# 7. Proveedor obligatorio

Toda compra requiere proveedor.

El proveedor seleccionado debe:

- Existir.
- Estar activo.
- Pertenecer a la misma empresa de la compra.

El servidor vuelve a validar estas condiciones dentro de la transacción.

---

# 8. Comprobante

Los campos son opcionales:

- `TipoComprobante`.
- `NumeroComprobante`.

Ambos se normalizan eliminando espacios externos y convirtiendo valores vacíos a `null`.

Si se informa `NumeroComprobante`, el sistema controla que no exista otra **compra activa** con la misma combinación de:

```text
EmpresaId
ProveedorId
TipoComprobante
NumeroComprobante
```

Una compra anulada no participa actualmente de esa validación de duplicidad.

---

# 9. Detalles de compra

Toda compra debe incluir al menos un producto.

Cada línea contiene como mínimo:

- ProductoId.
- Cantidad.
- PrecioUnitario de costo.
- NuevoPrecioVenta opcional.

Las cantidades deben ser mayores a cero.

Actualmente `PrecioUnitario` y `NuevoPrecioVenta`, cuando se informa, no pueden ser negativos.

---

# 10. Productos duplicados

A diferencia del flujo de Venta, Compra no consolida automáticamente productos repetidos.

El servidor rechaza la operación si el mismo `ProductoId` aparece más de una vez.

Por lo tanto:

```text
Producto 10 - Cantidad 1
Producto 10 - Cantidad 2
```

es inválido como POST de Compra.

La interfaz debe representar cada producto una única vez.

---

# 11. Validación de productos

Todos los productos incluidos deben:

- Existir.
- Encontrarse activos.
- Pertenecer a la empresa de la compra.

La validación se realiza nuevamente en servidor antes de modificar stock o costos.

---

# 12. Costo histórico

Por cada línea se guarda el costo ingresado en:

```text
DetalleCompra.PrecioUnitario
```

El subtotal se calcula como:

```text
Subtotal = Cantidad × PrecioUnitario
```

El detalle también conserva:

```text
PrecioCostoAnterior
```

Esto permite conocer el costo vigente antes de registrar la compra y posibilita determinadas restauraciones durante una anulación.

---

# 13. Actualización del costo del producto

Al confirmar cada línea:

```text
producto.PrecioCosto = detalleVM.PrecioUnitario
```

Por lo tanto, el costo actual del producto pasa a ser el costo unitario informado en la compra más reciente que lo modifica.

Actualmente no se utiliza costo promedio ponderado ni FIFO para actualizar `Producto.PrecioCosto`.

El historial permanece disponible a través de los detalles de compra.

---

# 14. Actualización opcional del precio de venta

Durante una compra puede informarse un nuevo precio de venta.

Si el nuevo valor es diferente al precio actual, el detalle conserva:

- `PrecioVentaAnterior`.
- `PrecioVentaNuevo`.

Y el producto se actualiza a:

```text
producto.PrecioVenta = PrecioVentaNuevo
```

Si no se informa un nuevo precio o coincide con el existente, no se registra un cambio histórico de precio de venta en ese detalle.

---

# 15. Incremento de stock

Por cada producto:

```text
StockPosterior = StockAnterior + CantidadComprada
```

El producto se actualiza con ese stock y se genera un `MovimientoStock` de tipo:

```text
Compra
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

# 16. Total de la compra

El total se calcula en servidor:

```text
TotalCompra = Σ (Cantidad × PrecioUnitario)
```

No debe confiarse en un total enviado desde la interfaz.

---

# 17. Registro transaccional

La creación utiliza una transacción con aislamiento:

```text
Serializable
```

Dentro de la misma operación se coordinan:

1. Validación del proveedor.
2. Validación del comprobante.
3. Validación de productos.
4. Creación de Compra.
5. Creación de DetalleCompra.
6. Incremento de stock.
7. Actualización del costo del producto.
8. Cambio opcional del precio de venta.
9. Creación de movimientos de stock.

Si ocurre un error, la operación se revierte.

---

# 18. Compra y pago al proveedor

Registrar una Compra **no genera automáticamente un PagoProveedor**.

Son procesos distintos.

Esto permite que una compra quede:

- Sin pagar.
- Parcialmente pagada.
- Totalmente pagada.

El estado financiero se obtiene de los pagos vigentes asociados y del `CompraSaldoService`.

La vista de detalle calcula actualmente:

- Total pagado.
- Saldo pendiente.
- Total reintegrado.
- Importe pendiente de recuperar.

---

# 19. Detalle de compra

La consulta de detalle incluye actualmente:

- Fecha.
- Proveedor.
- Tipo de comprobante.
- Número de comprobante.
- Total.
- Estado.
- Observaciones.
- Usuario de creación.
- Fecha y usuario de anulación cuando existen.
- Empresa.
- Productos.
- Cantidades.
- Costos históricos.
- Subtotales.
- Cambios de precio de venta registrados en los detalles.
- Pagos al proveedor.
- Saldo pendiente.
- Reintegros del proveedor.
- Devoluciones de compra.

---

# 20. Anulación de compra

La compra no se elimina físicamente.

Al anular se establece:

```text
Estado = false
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
```

La anulación se realiza dentro de una transacción `Serializable`.

---

# 21. Restricciones para anular

No puede anularse una compra si:

- Ya se encuentra anulada.
- Tiene pagos al proveedor activos.
- Tiene devoluciones de compra activas.
- No existe stock suficiente para retirar las unidades que originalmente ingresaron mediante la compra.

Los pagos activos deben anularse previamente.

Las devoluciones activas también deben anularse previamente.

Actualmente la acción `Compra.Anular` no utiliza la existencia de reintegros de proveedor activos como bloqueo explícito independiente.

---

# 22. Reversión de stock

Antes de anular se valida para cada línea:

```text
Producto.Stock >= CantidadComprada
```

Esto es necesario porque las unidades compradas pueden haber sido vendidas o retiradas posteriormente.

Si falta stock, la compra completa no puede anularse.

Cuando la anulación procede:

```text
StockPosterior = StockAnterior - CantidadComprada
```

Además se genera un `MovimientoStock` de tipo:

```text
AnulacionCompra
```

---

# 23. Restauración del costo al anular

La anulación no restaura el costo anterior de forma ciega.

Antes verifica si existe otra compra activa posterior del mismo producto.

Sólo restaura:

```text
producto.PrecioCosto = detalle.PrecioCostoAnterior
```

cuando:

- No existe una compra activa posterior que haya definido un costo más reciente.
- El costo actual del producto sigue coincidiendo con el costo de la compra que se está anulando.

Esto evita pisar información más nueva.

---

# 24. Restauración del precio de venta al anular

Si la compra había modificado el precio de venta, se conservan:

- PrecioVentaAnterior.
- PrecioVentaNuevo.

Al anular, el sistema sólo restaura el precio anterior cuando:

- No existe un cambio de precio de venta posterior proveniente de otra compra activa.
- El precio de venta actual sigue siendo el valor nuevo aplicado por la compra anulada.

De esa forma una anulación antigua no sobrescribe cambios posteriores legítimos.

---

# 25. Seguridad multiempresa

Para `AdminEmpresa`, las consultas y operaciones se restringen mediante:

```text
Compra.EmpresaId == usuario.EmpresaId
```

Además se validan explícitamente contra la empresa:

- Proveedor.
- Productos.

Para `SuperAdmin`, la empresa debe seleccionarse explícitamente al crear cuando no viene determinada previamente.

Un ID perteneciente a otra empresa nunca debe permitir acceso o asociación cross-tenant.

---

# 26. Reglas de negocio

1. Cada compra pertenece a una única empresa.
2. Actualmente no existe relación con Sucursal.
3. Toda compra requiere un proveedor activo de la misma empresa.
4. Toda compra requiere al menos un producto.
5. Un producto no puede aparecer repetido en la misma compra.
6. Todos los productos deben estar activos y pertenecer a la empresa.
7. La cantidad debe ser mayor a cero.
8. El costo unitario no puede ser negativo.
9. El subtotal se calcula en servidor.
10. El total se calcula en servidor.
11. Al confirmar una compra se incrementa stock.
12. Cada incremento genera un MovimientoStock de tipo Compra.
13. El precio de costo actual del producto se actualiza al costo informado en la compra.
14. Puede modificarse opcionalmente el precio de venta desde la misma operación.
15. Los costos anteriores y cambios de precio de venta se conservan en DetalleCompra.
16. El comprobante es opcional.
17. Cuando se informa número de comprobante, no puede duplicarse entre compras activas para la misma empresa, proveedor y tipo de comprobante.
18. Registrar una compra no implica registrar su pago.
19. La compra no se elimina físicamente.
20. La anulación revierte stock sólo si existe cantidad suficiente.
21. Una compra con pagos activos no puede anularse.
22. Una compra con devoluciones activas no puede anularse.
23. La restauración de costos y precios al anular respeta cambios posteriores.
24. Creación y anulación son operaciones transaccionales.

---

# 27. Casos de error relevantes

- Empresa inexistente o inactiva.
- Proveedor inexistente.
- Proveedor inactivo.
- Proveedor de otra empresa.
- Compra sin productos.
- Producto repetido.
- Producto inexistente.
- Producto inactivo.
- Producto de otra empresa.
- Cantidad menor o igual a cero.
- Precio de costo negativo.
- Nuevo precio de venta negativo.
- Comprobante activo duplicado.
- Error de base de datos durante creación.
- Compra inexistente.
- Compra ya anulada.
- Compra con pagos activos.
- Compra con devoluciones activas.
- Stock insuficiente para revertir una compra.

---

# 28. Integraciones actuales

Compra se integra con:

- Empresa.
- Usuario.
- Proveedor.
- Producto.
- DetalleCompra.
- MovimientoStock.
- PagoProveedor.
- ReintegroProveedor.
- DevolucionCompra.
- Caja y medios de pago a través del circuito de pagos/reintegros.
- Reportes.

Actualmente no existe integración con una entidad Sucursal.

---

# 29. Capacidades no implementadas

Actualmente no forman parte del módulo Compra:

- Sucursal.
- Órdenes de compra.
- Recepciones parciales.
- Estados pedido/recibido/pendiente.
- Costos adicionales distribuidos entre productos.
- Gastos logísticos estructurados.
- Gastos de importación estructurados.
- Compras internacionales con moneda/cotización.
- Facturación electrónica de proveedores.
- Importación masiva específica de compras.
- Comparación automática entre proveedores durante el alta.
- Sugerencias automáticas de compra.

---

# 30. Evolución futura

La evolución se administra mediante Roadmap y GitHub Issues.

Entre las mejoras previstas o posibles se encuentran:

- Historial y análisis de costos.
- Comparación de proveedores.
- Órdenes de compra.
- Recepciones parciales.
- Cantidades pedidas, recibidas y pendientes.
- Sugerencias de reposición.
- Costos adicionales.
- Recomendación de precio de venta frente a cambios de costo.
- Resumen financiero de proveedor.
- Creación de productos dentro del flujo de compra.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 31. Estado

✅ Registro de compras implementado.

✅ Comprobantes y observaciones implementados.

✅ Seguridad multiempresa implementada.

✅ Incremento y trazabilidad de stock implementados.

✅ Actualización de costo del producto implementada.

✅ Cambio opcional de precio de venta implementado.

✅ Pagos y saldo de proveedor integrados al detalle.

✅ Devoluciones y reintegros integrados al detalle.

✅ Anulación con reversión de stock implementada.

✅ Restauración protegida de costos/precios implementada.

✅ Filtros, búsqueda y paginación implementados.

🚧 Órdenes de compra, recepciones parciales y abastecimiento avanzado reservados para evolución post-MVP.