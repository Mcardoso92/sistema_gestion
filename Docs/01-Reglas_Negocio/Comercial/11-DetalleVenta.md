# Módulo Detalle de Venta

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Detalle de Venta almacena cada línea de producto confirmada dentro de una venta.

Mientras `Venta` representa la operación comercial completa, `DetalleVenta` conserva:

- Producto vendido.
- Cantidad.
- Precio unitario aplicado al momento de la operación.
- Subtotal histórico.

Su función principal es preservar la integridad histórica de la venta y permitir reconstruir exactamente qué se vendió y a qué precio.

---

# 2. Alcance actual

Actualmente:

- Los detalles se generan automáticamente durante el alta de una venta.
- No existe un CRUD independiente de `DetalleVenta`.
- Los detalles se consultan dentro del detalle de Venta.
- Cada venta debe contener al menos un detalle válido.
- El precio unitario se obtiene desde el producto en servidor al momento de confirmar.
- El subtotal se calcula en servidor.
- Las líneas repetidas del mismo producto se consolidan antes de persistir.

---

# 3. Acceso y permisos

`DetalleVenta` no posee un controller administrativo independiente.

El acceso se realiza a través del módulo Venta, cuyo controller está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto, actualmente no existen permisos independientes de DetalleVenta para roles `Cajero` o `Vendedor`.

La consulta de detalles hereda las reglas de seguridad y aislamiento multiempresa de Venta.

---

# 4. Modelo actual

La entidad `DetalleVenta` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| VentaId | int | Venta propietaria |
| ProductoId | int | Producto vendido |
| Cantidad | int | Mayor o igual a 1 |
| PrecioUnitario | decimal | Mayor o igual a 0,01 |
| Subtotal | decimal | Mayor o igual a 0,01 |

Relaciones:

- Venta.
- Producto.

---

# 5. Generación del detalle

Los detalles no se crean directamente desde datos económicos enviados por la interfaz.

El POST de Venta recibe principalmente:

- `ProductoId`.
- `Cantidad`.

Luego el servidor consulta nuevamente los productos válidos de la empresa y utiliza el precio vigente almacenado:

```text
PrecioUnitario = producto.PrecioVenta
```

El subtotal se calcula como:

```text
Subtotal = PrecioUnitario × Cantidad
```

Finalmente el detalle se agrega a la venta dentro de la misma transacción de creación.

---

# 6. Validaciones

## Venta

El detalle debe pertenecer a una venta válida.

La venta completa debe contener al menos una línea de producto.

## Producto

Durante el proceso de Venta el producto debe:

- Existir.
- Estar activo.
- Pertenecer a la empresa de la venta.
- Poseer stock suficiente para la cantidad solicitada.

## Cantidad

```text
Cantidad >= 1
```

El modelo valida que sea mayor a cero.

## Precio unitario

```text
PrecioUnitario >= 0.01
```

El precio utilizado no se toma del navegador, sino del producto recuperado desde base de datos.

## Subtotal

```text
Subtotal >= 0.01
```

Se calcula en servidor y no debe ingresarse manualmente.

---

# 7. Productos duplicados

La interfaz del POS intenta evitar líneas duplicadas, pero la regla también se aplica en servidor.

Antes de procesar la venta:

```text
Detalles
    .GroupBy(d => d.ProductoId)
    .Select(... Cantidad = suma ...)
```

Por lo tanto, si un POST manipulado incluye varias líneas del mismo producto, el servidor las consolida en una única línea lógica sumando cantidades.

Ejemplo:

```text
Producto 10 - Cantidad 1
Producto 10 - Cantidad 2
```

se procesa como:

```text
Producto 10 - Cantidad 3
```

Esta regla evita depender exclusivamente de JavaScript para mantener consistencia.

---

# 8. Precio histórico

`PrecioUnitario` representa el precio aplicado al momento de confirmar la venta.

Ejemplo:

```text
Precio actual del producto: $1.000
Cantidad: 2
PrecioUnitario guardado: $1.000
Subtotal guardado: $2.000
```

Si posteriormente el producto cambia a:

```text
PrecioVenta = $1.300
```

la venta anterior continúa mostrando `$1.000` por unidad.

Esta es una regla crítica de integridad histórica.

---

# 9. Subtotal histórico

El subtotal se guarda junto al detalle:

```text
Subtotal = PrecioUnitario × Cantidad
```

No se recalcula utilizando el precio actual del producto al consultar una venta histórica.

El total de la Venta se determina al confirmar la operación mediante la suma económica de sus líneas.

---

# 10. Inmutabilidad

No existe actualmente una acción de edición de `DetalleVenta` una vez confirmada la venta.

Una venta histórica no debe modificarse directamente para corregir cantidades o precios.

Cuando una operación necesita revertirse o corregirse deben utilizarse los flujos comerciales correspondientes, como:

- Anulación de Venta.
- Reintegro/devolución cuando corresponda.

Esto mantiene trazabilidad y evita alterar registros históricos silenciosamente.

---

# 11. Relación con stock

`DetalleVenta` describe qué se vendió, pero la trazabilidad de inventario se registra mediante `MovimientoStock`.

Al confirmar una venta, por cada detalle se realiza:

```text
StockPosterior = StockAnterior - Cantidad
```

y se genera un movimiento de tipo:

```text
Venta
```

Al anular una venta, las cantidades de los detalles se utilizan para restaurar stock y generar movimientos de tipo:

```text
AnulacionVenta
```

El detalle no debe utilizarse como sustituto del historial de movimientos de stock.

---

# 12. Relación con producto

El detalle mantiene una relación con `ProductoId` para identificar el artículo vendido.

Los productos se administran mediante baja lógica, por lo que un producto desactivado puede seguir apareciendo correctamente en ventas históricas.

La desactivación posterior del producto no modifica:

- Cantidad vendida.
- Precio unitario histórico.
- Subtotal.

---

# 13. Seguridad multiempresa

`DetalleVenta` no posee actualmente un `EmpresaId` propio.

La pertenencia a empresa se determina a través de:

```text
DetalleVenta -> Venta -> EmpresaId
```

Durante la creación, además, el Producto se valida explícitamente contra la empresa de la Venta.

Las consultas de detalles deben realizarse siempre dentro de una Venta previamente autorizada para el tenant correspondiente.

---

# 14. Reglas de negocio

1. Cada detalle pertenece a una única venta.
2. Cada detalle corresponde a un único producto.
3. Una venta debe contener al menos un detalle.
4. La cantidad debe ser mayor a cero.
5. El producto debe ser válido para la empresa de la venta.
6. El precio unitario se obtiene desde base de datos al confirmar.
7. No se confía en un precio enviado por el cliente.
8. El subtotal se calcula en servidor.
9. Las líneas repetidas del mismo producto se consolidan en servidor.
10. El precio unitario queda preservado históricamente.
11. Cambiar `Producto.PrecioVenta` no altera ventas anteriores.
12. Los detalles confirmados no poseen edición administrativa independiente.
13. Los detalles históricos no se eliminan físicamente de forma aislada.
14. La corrección de una operación debe realizarse mediante anulación o reintegro según corresponda.
15. La trazabilidad de stock corresponde a `MovimientoStock`, no al detalle por sí solo.

---

# 15. Casos de error relevantes

- Venta sin detalles.
- Producto inexistente.
- Producto inactivo.
- Producto de otra empresa.
- Cantidad menor o igual a cero.
- Stock insuficiente.
- Precio inválido en el producto.
- Error de persistencia durante la creación de la venta.

Ante errores durante el alta, la transacción de Venta debe impedir que queden detalles persistidos parcialmente.

---

# 16. Integraciones actuales

DetalleVenta se integra con:

- Venta.
- Producto.
- MovimientoStock de manera indirecta durante el proceso de venta.
- Reportes de ventas.
- Dashboard.
- Reintegros/devoluciones que utilicen las líneas históricas de venta.

---

# 17. Capacidades no implementadas

Actualmente `DetalleVenta` no posee campos propios para:

- Descuento por línea.
- Recargo por línea.
- Impuestos desglosados por línea.
- Observaciones.
- Promoción aplicada.
- Lote.
- Número de serie.
- Fecha de vencimiento.
- Garantía.
- Variante del producto como entidad separada.

Estas capacidades requieren reglas adicionales antes de incorporarse.

---

# 18. Evolución futura

La evolución se administra mediante Roadmap y GitHub Issues.

Entre las mejoras posibles se encuentran:

- Descuentos por producto.
- Promociones.
- Combos y kits.
- Variantes.
- Lotes.
- Series.
- Garantías.
- Mayor trazabilidad para devoluciones parciales.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 19. Estado

✅ Generación automática implementada.

✅ Precio unitario histórico implementado.

✅ Subtotal histórico implementado.

✅ Consolidación server-side de productos duplicados implementada.

✅ Integración con Venta y stock implementada.

✅ Inmutabilidad operativa de detalles confirmados implementada mediante ausencia de CRUD de edición.

🚧 Descuentos, promociones, lotes, series y otras capacidades avanzadas reservadas para evolución futura.