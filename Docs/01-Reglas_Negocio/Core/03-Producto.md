# Módulo Producto

---

# 1. Objetivo

El módulo Producto permite administrar el catálogo de artículos comercializados por cada empresa dentro de Veltika.

Cada producto contiene toda la información necesaria para ser vendido, comprado, controlado en stock y utilizado por los distintos módulos del sistema.

Este módulo constituye uno de los pilares principales del sistema de gestión.

---

# 2. Alcance

El módulo permite registrar, modificar, consultar, activar y desactivar productos pertenecientes a una empresa.

Además, administra la información comercial, tributaria y de inventario de cada producto, integrándose con ventas, compras, stock, reportes y futuras funcionalidades.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Empleado de Ventas
- Responsable de Stock

---

# 4. Permisos

## Super Administrador

✅ Visualizar productos de cualquier empresa

✅ Crear productos

✅ Editar productos

✅ Activar productos

✅ Desactivar productos

## Administrador de Empresa

✅ Crear productos

✅ Editar productos

✅ Activar productos

✅ Desactivar productos

✅ Consultar productos

## Empleado

✅ Consultar productos

❌ Crear productos

❌ Modificar productos

❌ Eliminar productos

---

# 5. Funcionalidades

Actualmente

- Registrar producto
- Editar producto
- Activar producto
- Desactivar producto
- Consultar productos
- Buscar productos
- Filtrar productos
- Asociar categoría
- Cargar imagen

Versiones futuras

- Código de barras
- SKU automático
- Productos con variantes
- Productos compuestos
- Packs
- Kits
- Productos con series
- Productos con lote
- Productos con vencimiento
- Múltiples imágenes
- Historial de precios
- Historial de stock
- Importación masiva
- Exportación
- Etiquetas
- Generación de códigos QR
- Integración con eCommerce

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| CategoriaId | Categoría del producto |
| Nombre | Nombre del producto |
| Descripcion | Descripción |
| PrecioCompra | Precio de compra |
| PrecioVenta | Precio de venta |
| Stock | Cantidad disponible |
| UrlImagen | Imagen del producto |
| Estado | Activo o Inactivo |

Campos futuros

- SKU
- Código de barras
- Marca
- Modelo
- Unidad de medida
- IVA
- Impuesto interno
- Peso
- Alto
- Ancho
- Largo
- Stock mínimo
- Stock máximo
- Ubicación en depósito
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El nombre no puede superar la longitud máxima permitida.
- El precio de compra no puede ser negativo.
- El precio de venta no puede ser negativo.
- El stock no puede ser negativo.
- La categoría debe existir.
- La empresa debe existir.
- La imagen debe cumplir con los formatos permitidos.
- No puede existir otro producto con el mismo nombre dentro de la misma empresa (si así se define).

---

# 8. Reglas de negocio

- Cada producto pertenece exclusivamente a una empresa.
- Cada producto pertenece a una única categoría.
- Una categoría puede contener múltiples productos.
- Un producto puede participar en múltiples ventas.
- Un producto puede participar en múltiples compras.
- Un producto desactivado no podrá venderse.
- El stock se actualizará automáticamente mediante ventas y compras.
- La eliminación física del producto no está permitida; únicamente podrá desactivarse.

---

# 9. Casos de uso

## Crear producto

El usuario registra un nuevo producto dentro de la empresa.

Resultado esperado:

- Producto creado correctamente.
- Disponible para ventas y control de stock.

---

## Editar producto

Permite modificar la información comercial del producto.

---

## Desactivar producto

El producto deja de estar disponible para futuras ventas.

Las operaciones históricas permanecen intactas.

---

## Consultar productos

Permite visualizar el listado completo de productos registrados.

---

## Buscar producto

Permite localizar productos mediante distintos criterios de búsqueda.

---

# 10. Casos de error

- Nombre vacío.
- Precio inválido.
- Stock inválido.
- Categoría inexistente.
- Empresa inexistente.
- Imagen inválida.
- Usuario sin permisos.
- Producto duplicado.
- Intento de modificar un producto inexistente.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Productos.
2. Selecciona "Nuevo Producto".
3. Completa la información requerida.
4. Selecciona una categoría.
5. Carga una imagen (opcional).
6. El sistema valida la información.
7. Se registra el producto.
8. El producto queda disponible para futuras operaciones.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Categorías
- Ventas
- DetalleVenta
- Compras
- DetalleCompra
- Stock
- Caja
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Variantes de productos.
- Productos compuestos.
- Kits.
- Productos con número de serie.
- Control por lote.
- Control de vencimientos.
- Múltiples listas de precios.
- Promociones.
- Descuentos automáticos.
- Inteligencia de reposición.
- Integración con balanzas.
- Integración con lectores de código de barras.
- Sincronización con tiendas online.
- API de productos.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Consulta
- Activación
- Desactivación
- Imagen
- Categorías

Versión 2.0

- Código de barras
- SKU
- Stock mínimo
- Historial de precios
- Importación y exportación

Versión 3.0

- Variantes
- Productos compuestos
- Lotes
- Series
- Vencimientos
- Integración eCommerce
- API pública