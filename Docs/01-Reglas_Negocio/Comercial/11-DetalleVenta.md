# Módulo Detalle de Venta

---

# 1. Objetivo

El módulo Detalle de Venta almacena cada uno de los productos vendidos dentro de una venta.

Mientras que el módulo Venta representa la operación comercial completa, el Detalle de Venta registra línea por línea los productos involucrados, las cantidades, los precios utilizados al momento de la venta y los subtotales correspondientes.

Este módulo garantiza la integridad histórica de las ventas y permite reconstruir exactamente qué se vendió en cualquier momento.

---

# 2. Alcance

El módulo administra los productos pertenecientes a una venta.

Cada venta deberá contener uno o más detalles.

Cada detalle representa un único producto.

El detalle se genera automáticamente durante el proceso de venta y no puede administrarse manualmente desde un CRUD independiente.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Cajero
- Vendedor

---

# 4. Permisos

## Super Administrador

✅ Consultar detalles de cualquier venta.

## Administrador de Empresa

✅ Consultar detalles.

## Cajero

✅ Consultar detalles de sus ventas.

## Vendedor

✅ Consultar detalles de sus ventas.

❌ Ningún usuario puede crear, editar o eliminar detalles manualmente.

---

# 5. Funcionalidades

Actualmente

- Generación automática del detalle.
- Consulta del detalle.
- Visualización de productos vendidos.

Versiones futuras

- Descuentos por línea.
- Promociones.
- Combos.
- Kits de productos.
- Bonificaciones.
- Observaciones por producto.
- Devoluciones parciales.
- Número de serie por producto.
- Lotes.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| VentaId | Venta a la que pertenece |
| ProductoId | Producto vendido |
| Cantidad | Cantidad vendida |
| PrecioUnitario | Precio al momento de la venta |
| Subtotal | Cantidad × Precio |

Campos futuros

- Descuento
- Recargo
- Impuesto
- Observaciones
- NumeroSerie
- Lote
- FechaVencimiento
- Estado

---

# 7. Validaciones

- Debe existir una venta válida.
- Debe existir un producto.
- La cantidad debe ser mayor a cero.
- El precio unitario debe ser mayor a cero.
- El subtotal será calculado automáticamente.
- No pueden agregarse productos duplicados dentro de la misma venta; si el usuario selecciona nuevamente un producto, el sistema incrementará la cantidad existente.

---

# 8. Reglas de negocio

- Un detalle pertenece únicamente a una venta.
- Una venta debe tener al menos un detalle.
- Cada detalle corresponde a un único producto.
- El precio unitario se copia desde el producto al momento de confirmar la venta.
- Si el precio del producto cambia posteriormente, las ventas históricas no se modificarán.
- El subtotal se calcula automáticamente.
- El total de la venta será la suma de todos sus detalles.
- Los detalles no podrán editarse una vez confirmada la venta.
- Los detalles nunca podrán eliminarse físicamente.

---

# 9. Casos de uso

## Registrar detalle

El sistema genera automáticamente cada línea de la venta.

Resultado esperado:

- Producto asociado correctamente.
- Precio histórico almacenado.
- Subtotal calculado automáticamente.

---

## Consultar detalle

Permite visualizar todos los productos pertenecientes a una venta.

---

# 10. Casos de error

- Producto inexistente.
- Cantidad inválida.
- Precio inválido.
- Venta inexistente.
- Usuario sin permisos.

---

# 11. Flujo funcional

1. El usuario agrega un producto al carrito.
2. El sistema obtiene el precio actual del producto.
3. El usuario indica la cantidad.
4. El sistema calcula el subtotal.
5. El usuario continúa agregando productos.
6. Al confirmar la venta se generan automáticamente todos los detalles.
7. Los detalles quedan almacenados de forma permanente.

---

# 12. Integraciones

Este módulo se relaciona con:

- Venta
- Producto
- Stock
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Descuentos por producto.
- Combos.
- Promociones.
- Lotes.
- Series.
- Productos compuestos.
- Trazabilidad.
- Garantías.

---

# 14. Roadmap

Versión 1.0

- Generación automática.
- Consulta.
- Precio histórico.
- Subtotal automático.

Versión 2.0

- Descuentos.
- Promociones.
- Combos.
- Bonificaciones.

Versión 3.0

- Lotes.
- Series.
- Garantías.
- Devoluciones parciales.

---

# 15. Decisiones de Arquitectura

## Precio histórico

El detalle almacenará el precio utilizado al momento de la venta.

Esto garantiza que una modificación posterior del precio del producto no altere las ventas históricas.

---

## Producto eliminado

Los productos nunca serán eliminados físicamente.

Únicamente podrán desactivarse.

De esta forma todas las ventas históricas conservarán su integridad.

---

## Modificación de ventas

Una venta confirmada será inmutable.

No podrá modificarse ningún detalle.

Si existió un error, deberá realizarse una anulación o devolución según corresponda.

---

## Productos duplicados

Si durante una venta el usuario selecciona dos veces el mismo producto, el sistema no generará dos líneas.

Incrementará automáticamente la cantidad del detalle existente.

Ejemplo

Coca Cola 500ml

Cantidad: 1

El usuario vuelve a seleccionarla.

Resultado:

Coca Cola 500ml

Cantidad: 2

---

## Cálculo del subtotal

El subtotal nunca será ingresado manualmente.

Siempre será calculado automáticamente por el sistema.

Subtotal = Cantidad × Precio Unitario

---

## Integridad histórica

Toda la información almacenada en el detalle representa exactamente el estado de la venta en el momento en que fue realizada.

Los cambios posteriores en productos, categorías o precios no modificarán los datos históricos.