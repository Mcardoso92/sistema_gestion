# Módulo Stock

---

# 1. Objetivo

El módulo Stock permite consultar el inventario actual de los productos pertenecientes a una empresa dentro de Veltika.

Su función principal es brindar una visión precisa y en tiempo real de la disponibilidad de cada producto, sin permitir modificaciones directas sobre las cantidades.

El stock representa el resultado acumulado de todos los movimientos registrados en el sistema.

---

# 2. Alcance

El módulo permite consultar el estado actual del inventario.

No permite crear, modificar ni eliminar registros de stock de forma manual.

Las cantidades disponibles serán actualizadas automáticamente por los distintos procesos del sistema.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Responsable de Depósito
- Cajero
- Vendedor

---

# 4. Permisos

## Super Administrador

✅ Consultar stock de cualquier empresa.

## Administrador de Empresa

✅ Consultar stock.

## Responsable de Depósito

✅ Consultar stock.

✅ Consultar historial de movimientos.

## Cajero

✅ Consultar disponibilidad de productos.

## Vendedor

✅ Consultar disponibilidad de productos.

❌ Ningún usuario puede modificar manualmente el stock.

---

# 5. Funcionalidades

Actualmente

- Consultar stock actual.
- Buscar productos.
- Filtrar productos.
- Visualizar disponibilidad.

Versiones futuras

- Stock mínimo.
- Stock máximo.
- Stock comprometido.
- Stock reservado.
- Stock disponible.
- Inventario valorizado.
- Alertas automáticas.
- Dashboard de inventario.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal |
| ProductoId | Producto |
| CantidadActual | Stock disponible |

Campos futuros

- StockMinimo
- StockMaximo
- StockReservado
- StockComprometido
- StockDisponible
- UltimaCompra
- UltimaVenta
- FechaUltimoMovimiento
- CostoPromedio
- ValorInventario

---

# 7. Validaciones

- Debe existir el producto.
- Debe existir la empresa.
- Debe existir la sucursal.
- El stock nunca podrá modificarse manualmente.
- Toda modificación deberá provenir de un Movimiento de Stock.

---

# 8. Reglas de negocio

- Cada producto posee un stock por sucursal.
- El stock se actualiza automáticamente.
- El stock nunca será ingresado manualmente.
- Toda modificación deberá quedar registrada mediante un Movimiento de Stock.
- El stock podrá consultarse en cualquier momento.
- Si el producto está desactivado continuará conservando su stock histórico.

---

# 9. Casos de uso

## Consultar stock

Permite visualizar la cantidad disponible de cada producto.

Resultado esperado:

- Inventario actualizado.

---

## Buscar producto

Permite localizar rápidamente un producto para conocer su disponibilidad.

---

## Consultar historial

Permite acceder al historial completo de movimientos del producto.

---

# 10. Casos de error

- Producto inexistente.
- Usuario sin permisos.
- Empresa inexistente.
- Sucursal inexistente.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Stock.
2. Selecciona un producto.
3. El sistema consulta el inventario actualizado.
4. Se muestra la cantidad disponible.
5. El usuario puede acceder al historial de movimientos.

---

# 12. Integraciones

Este módulo se relaciona con:

- Producto
- MovimientoStock
- Compra
- Venta
- AjusteStock
- Sucursal
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Stock mínimo.
- Stock máximo.
- Inventarios físicos.
- Alertas automáticas.
- ABC de productos.
- Productos sin movimiento.
- Rotación de inventario.
- Valor económico del inventario.

---

# 14. Roadmap

Versión 1.0

- Consulta de stock.
- Consulta por sucursal.
- Búsquedas.

Versión 2.0

- Stock mínimo.
- Alertas.
- Inventarios.

Versión 3.0

- Predicción de demanda.
- IA.
- Dashboard ejecutivo.

---

# 15. Decisiones de Arquitectura

## El stock no posee CRUD

El módulo Stock no permitirá crear, editar ni eliminar registros.

Su única función será consultar el estado actual del inventario.

---

## El stock es un resultado

La cantidad disponible será el resultado de todos los movimientos registrados.

Ejemplo:

Compra +50

Venta -12

Venta -5

Ajuste -1

Stock actual = 32

---

## Origen único

Todo cambio de stock deberá provenir de alguno de los siguientes procesos:

- Compra
- Venta
- Ajuste de Stock
- Devolución de Cliente
- Devolución a Proveedor
- Transferencia entre sucursales (futuro)

No existirán otros mecanismos para modificar el inventario.

---

## Consulta en tiempo real

El stock mostrado siempre corresponderá al estado más reciente del inventario.

No existirán sincronizaciones manuales.

---

## Productos sin movimiento

Cuando un producto sea creado, su stock inicial será cero.

El primer movimiento que modificará su inventario será una compra o un ajuste inicial de stock.

---

## Stock por sucursal

Cada sucursal administrará su propio inventario.

Un mismo producto podrá tener distintas cantidades según la sucursal donde se consulte.

---

## Auditoría

Toda modificación del inventario podrá reconstruirse consultando el historial de movimientos del producto.

El módulo Stock nunca almacenará información que no pueda justificarse mediante los movimientos registrados.