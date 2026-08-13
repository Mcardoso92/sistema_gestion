# Módulo Detalle de Compra

---

# 1. Objetivo

El módulo Detalle de Compra almacena cada uno de los productos adquiridos dentro de una compra.

Mientras que el módulo Compra representa la operación comercial completa realizada con un proveedor, el Detalle de Compra registra línea por línea los productos comprados, las cantidades, los costos utilizados al momento de la compra y los subtotales correspondientes.

Este módulo garantiza la integridad histórica de las compras y permite reconstruir exactamente qué productos ingresaron al inventario en cualquier momento.

---

# 2. Alcance

El módulo administra los productos pertenecientes a una compra.

Cada compra deberá contener uno o más detalles.

Cada detalle representa un único producto.

El detalle se genera automáticamente durante el proceso de compra y no puede administrarse manualmente desde un CRUD independiente.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Responsable de Compras

---

# 4. Permisos

## Super Administrador

✅ Consultar detalles de cualquier compra.

## Administrador de Empresa

✅ Consultar detalles.

## Responsable de Compras

✅ Consultar detalles de las compras registradas.

❌ Ningún usuario puede crear, editar o eliminar detalles manualmente.

---

# 5. Funcionalidades

Actualmente

- Generación automática del detalle.
- Consulta del detalle.
- Visualización de productos comprados.

Versiones futuras

- Costos adicionales por línea.
- Descuentos por producto.
- Bonificaciones.
- Lotes.
- Número de serie.
- Fecha de vencimiento.
- Recepciones parciales.
- Productos con trazabilidad.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| CompraId | Compra a la que pertenece |
| ProductoId | Producto adquirido |
| Cantidad | Cantidad comprada |
| CostoUnitario | Costo al momento de la compra |
| Subtotal | Cantidad × Costo Unitario |

Campos futuros

- Descuento
- Impuesto
- Recargo
- Lote
- NumeroSerie
- FechaVencimiento
- Observaciones
- Estado

---

# 7. Validaciones

- Debe existir una compra válida.
- Debe existir un producto.
- La cantidad debe ser mayor a cero.
- El costo unitario debe ser mayor a cero.
- El subtotal será calculado automáticamente.
- No pueden agregarse productos duplicados dentro de la misma compra; si el usuario selecciona nuevamente un producto, el sistema incrementará la cantidad existente.

---

# 8. Reglas de negocio

- Un detalle pertenece únicamente a una compra.
- Una compra debe contener al menos un detalle.
- Cada detalle corresponde a un único producto.
- El costo unitario se almacena como costo histórico.
- El subtotal se calcula automáticamente.
- El total de la compra será la suma de todos sus detalles.
- Los detalles no podrán editarse una vez confirmada la compra.
- Los detalles nunca podrán eliminarse físicamente.

---

# 9. Casos de uso

## Registrar detalle

El sistema genera automáticamente cada línea de la compra.

Resultado esperado:

- Producto asociado correctamente.
- Costo histórico almacenado.
- Subtotal calculado automáticamente.

---

## Consultar detalle

Permite visualizar todos los productos pertenecientes a una compra.

---

# 10. Casos de error

- Producto inexistente.
- Cantidad inválida.
- Costo inválido.
- Compra inexistente.
- Usuario sin permisos.

---

# 11. Flujo funcional

1. El usuario agrega un producto a la compra.
2. El sistema obtiene el costo actual del producto.
3. El usuario puede modificar el costo según la factura del proveedor.
4. El usuario indica la cantidad.
5. El sistema calcula el subtotal.
6. El usuario continúa agregando productos.
7. Al confirmar la compra se generan automáticamente todos los detalles.
8. Se incrementa el stock.
9. El costo actual del producto se actualiza automáticamente.
10. Los detalles quedan almacenados de forma permanente.

---

# 12. Integraciones

Este módulo se relaciona con:

- Compra
- Producto
- Stock
- Proveedor
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Productos con lote.
- Productos con serie.
- Productos con vencimiento.
- Costos logísticos.
- Costos de importación.
- Trazabilidad completa.
- Recepciones parciales.
- Integración con órdenes de compra.

---

# 14. Roadmap

Versión 1.0

- Generación automática.
- Consulta.
- Costo histórico.
- Subtotal automático.

Versión 2.0

- Lotes.
- Series.
- Vencimientos.
- Costos adicionales.

Versión 3.0

- Recepciones parciales.
- Importaciones.
- Trazabilidad completa.
- Automatización de compras.

---

# 15. Decisiones de Arquitectura

## Costo histórico

Cada detalle almacenará el costo utilizado al momento de registrar la compra.

Si el costo del producto cambia posteriormente, las compras históricas permanecerán sin modificaciones.

---

## Actualización del costo del producto

Al confirmar la compra, el sistema actualizará automáticamente el costo vigente del producto.

Este costo será utilizado como referencia para futuras compras y para el cálculo de márgenes de ganancia.

---

## Productos duplicados

Si durante una compra el usuario selecciona dos veces el mismo producto, el sistema no generará dos líneas.

Incrementará automáticamente la cantidad del detalle existente.

Ejemplo

Yerba 1 Kg

Cantidad: 5

El usuario vuelve a agregar Yerba 1 Kg.

Resultado:

Yerba 1 Kg

Cantidad: 8

---

## Cálculo del subtotal

El subtotal nunca será ingresado manualmente.

Siempre será calculado automáticamente por el sistema.

Subtotal = Cantidad × Costo Unitario

---

## Compra inmutable

Una compra confirmada será inmutable.

No podrá modificarse ningún detalle.

Si existió un error, deberá anularse y registrarse nuevamente.

---

## Integridad histórica

Toda la información almacenada en el detalle representa exactamente el estado de la compra en el momento en que fue realizada.

Los cambios posteriores en proveedores, productos o costos no modificarán los datos históricos.

---

## Impacto en el stock

Cada detalle incrementará el stock del producto según la cantidad comprada.

El sistema registrará el movimiento correspondiente para mantener la trazabilidad completa del inventario.

---

## Base para el cálculo de costos

En futuras versiones, el Detalle de Compra será la fuente principal para calcular:

- Costo promedio.
- Último costo.
- Margen de ganancia.
- Rentabilidad por producto.
- Historial de variaciones de costos.