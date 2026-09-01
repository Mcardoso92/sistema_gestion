# Módulo Producto

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Producto administra el catálogo de artículos comercializados por cada empresa dentro de Veltika.

Cada producto pertenece a una única empresa y se integra con categorías, ventas, compras, inventario, reportes y procesos de importación.

El módulo debe preservar el aislamiento multiempresa y la trazabilidad de stock.

---

# 2. Alcance actual

Actualmente permite:

- Listar productos.
- Buscar por nombre o código de barras.
- Filtrar por estado.
- Filtrar por categoría.
- Filtrar por empresa para `SuperAdmin`.
- Consultar detalle.
- Crear productos.
- Editar información comercial.
- Desactivar productos mediante baja lógica.
- Reactivar productos desde edición.
- Asociar una categoría activa de la misma empresa.
- Gestionar una imagen por producto.
- Registrar código de barras.
- Definir punto de reposición.
- Registrar stock inicial al crear el producto.
- Generar trazabilidad del stock inicial mediante `MovimientoStock`.
- Importar productos y stock mediante archivo Excel con vista previa y confirmación.

---

# 3. Actores y permisos

El controller principal está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

El flujo de importación posee la misma restricción.

## SuperAdmin

Puede:

- Visualizar productos de todas las empresas.
- Filtrar por empresa.
- Crear productos para una empresa activa.
- Editar productos de cualquier empresa.
- Cambiar la empresa de un producto durante edición, sujeto a validaciones.
- Desactivar y reactivar productos.
- Importar productos para una empresa seleccionada.

## AdminEmpresa

Puede:

- Visualizar únicamente productos de su empresa.
- Crear productos para su empresa.
- Editar productos de su empresa.
- Desactivar y reactivar productos de su empresa.
- Importar productos para su empresa.

Para usuarios que no son `SuperAdmin`, el servidor fuerza `EmpresaId` al valor del usuario autenticado.

---

# 4. Modelo actual

La entidad `Producto` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| CodigoBarra | string? | Opcional, máximo 100 caracteres |
| Nombre | string | Obligatorio, máximo 100 caracteres |
| Descripcion | string? | Opcional, máximo 500 caracteres |
| CategoriaId | int | Categoría propietaria dentro de la empresa |
| PrecioCosto | decimal | Mayor o igual a 0 |
| PrecioVenta | decimal | Mayor o igual a 0 |
| Stock | int | Mayor o igual a 0 |
| PuntoReposicion | int | Mayor o igual a 0 |
| Estado | bool | Activo o inactivo |
| UrlImagen | string? | Ruta/URL, máximo 500 caracteres |
| FechaAlta | DateTime | Asignada automáticamente al crear |
| EmpresaId | int | Empresa propietaria |

Relaciones principales:

- Empresa.
- Categoría.
- Detalles de venta.
- Detalles de compra.
- Movimientos de stock.

---

# 5. Listado y búsqueda

El listado muestra productos activos por defecto.

Permite filtrar por:

- Activos.
- Inactivos.
- Todos.
- Categoría.
- Empresa para `SuperAdmin`.

La búsqueda acepta coincidencias en:

- Nombre.
- Código de barras.

Los productos se ordenan alfabéticamente por nombre.

La paginación actual utiliza 20 registros por página.

El listado utiliza `AsNoTracking()` para consultas de lectura.

---

# 6. Creación

Al crear un producto pueden informarse:

- Código de barras.
- Nombre.
- Descripción.
- Categoría.
- Precio de costo.
- Precio de venta.
- Stock inicial.
- Punto de reposición.
- Empresa cuando opera un `SuperAdmin`.
- Imagen opcional.

El servidor asigna automáticamente:

```text
FechaAlta = DateTime.Now
Estado = true
```

Para `AdminEmpresa`, cualquier `EmpresaId` recibido desde el cliente es reemplazado por el de su usuario autenticado.

---

# 7. Validaciones de creación y edición

## Empresa

La empresa debe:

- Existir.
- Estar activa.

## Categoría

La categoría debe:

- Existir.
- Estar activa.
- Pertenecer a la misma empresa del producto.

## Nombre

El nombre:

- Es obligatorio.
- Admite como máximo 100 caracteres.
- Debe ser único dentro de la misma empresa ignorando diferencias de mayúsculas y minúsculas.

El mismo nombre puede existir en empresas distintas.

## Valores numéricos

- `PrecioCosto >= 0`.
- `PrecioVenta >= 0`.
- `Stock >= 0`.
- `PuntoReposicion >= 0`.

## Otros límites

- Código de barras: máximo 100 caracteres.
- Descripción: máximo 500 caracteres.
- URL/ruta de imagen: máximo 500 caracteres.

---

# 8. Stock inicial

El stock informado durante la creación se considera stock inicial.

Si:

```text
Stock > 0
```

el sistema crea un `MovimientoStock` con:

- Tipo `StockInicial`.
- Cantidad igual al stock cargado.
- StockAnterior = 0.
- StockPosterior = stock inicial.
- Motivo `Stock inicial`.
- Fecha de creación.
- Usuario responsable.
- Producto y empresa correspondientes.

Esto garantiza que el stock inicial tenga trazabilidad.

Si el producto se crea con stock 0, no se genera movimiento de stock inicial.

---

# 9. Edición y regla de stock

Durante la edición normal pueden modificarse actualmente:

- Código de barras.
- Nombre.
- Descripción.
- Categoría.
- Precio de costo.
- Precio de venta.
- Punto de reposición.
- Estado.
- Empresa para `SuperAdmin`.
- Imagen.

El campo `Stock` no forma parte del bind de edición del `ProductoController`.

Por lo tanto, el stock existente no debe modificarse arbitrariamente desde la edición general del producto.

Los cambios posteriores de stock deben realizarse mediante procesos que generen la trazabilidad correspondiente, como:

- Compras.
- Ventas.
- Ajustes.
- Devoluciones/reintegros.
- Importaciones cuando corresponda.

---

# 10. Imágenes

El producto puede poseer una imagen asociada mediante `UrlImagen`.

En creación:

- La imagen es opcional.
- Se procesa mediante `IImagenService`.
- Si falla el guardado de la imagen, la creación del producto se revierte dentro de la transacción.

En edición:

- Puede cargarse una nueva imagen.
- Puede eliminarse la imagen existente.
- Cuando la nueva imagen se guarda correctamente, se elimina la anterior.
- Si ocurre un error, se intenta preservar la imagen anterior y eliminar cualquier archivo nuevo incompleto.

Actualmente el modelo contempla una única imagen principal por producto.

---

# 11. Transacción de creación

La creación del producto utiliza una transacción para coordinar:

1. Alta del producto.
2. Guardado de imagen, si existe.
3. Registro de movimiento de stock inicial, si corresponde.

La operación se confirma únicamente cuando todos los pasos requeridos finalizan correctamente.

Esto evita dejar productos parcialmente creados o stock inicial sin trazabilidad.

---

# 12. Desactivación y reactivación

La baja es lógica:

```text
Estado = false
```

No se elimina físicamente el producto.

Esto preserva:

- Ventas históricas.
- Compras históricas.
- Movimientos de stock.
- Relaciones existentes.

Una vez desactivado, el producto puede reactivarse desde edición estableciendo nuevamente `Estado = true`.

---

# 13. Seguridad multiempresa

Para usuarios que no son `SuperAdmin`, las consultas de detalle, edición y desactivación filtran por:

```text
Producto.EmpresaId == usuario.EmpresaId
```

Además:

- La empresa enviada por el cliente no es confiable para `AdminEmpresa`.
- La categoría seleccionada se valida contra la empresa del producto.
- No debe ser posible utilizar una categoría de otra empresa.

La seguridad multiempresa debe mantenerse también en búsquedas, importaciones y futuras integraciones de catálogo.

---

# 14. Importación masiva desde Excel

Veltika posee actualmente un flujo específico de importación de productos.

El módulo permite:

- Descargar una plantilla oficial `PlantillaProductosVeltika.xlsx`.
- Seleccionar archivo Excel.
- Analizar el archivo antes de importar.
- Generar una vista previa.
- Confirmar la importación posteriormente.

La importación está protegida para roles:

```text
SuperAdmin,AdminEmpresa
```

## Empresa destino

Para `SuperAdmin` se utiliza la empresa seleccionada.

Para `AdminEmpresa` se ignora cualquier empresa externa y se utiliza su propia empresa.

La empresa debe existir y estar activa.

## Vista previa

El análisis genera una vista previa identificada mediante un token asociado a:

- Empresa.
- Usuario.

Si la vista previa vence, debe analizarse nuevamente el archivo.

## Confirmación

La importación sólo se ejecuta después de la confirmación.

Ante un error global de importación, el flujo informa que no se importó ningún producto.

Las reglas específicas de formato, filas y validaciones internas corresponden a `IProductoImportacionService` y deberán mantenerse sincronizadas con este documento si evolucionan.

---

# 15. Punto de reposición

`PuntoReposicion` forma parte actualmente del modelo y ya no es una funcionalidad futura.

Permite establecer un umbral de referencia para identificar productos que requieren reposición.

Debe ser mayor o igual a 0.

Este valor puede utilizarse en reportes, dashboard y futuras herramientas de sugerencia de compra.

---

# 16. Código de barras

`CodigoBarra` forma parte actualmente del modelo.

Es opcional y admite hasta 100 caracteres.

La búsqueda administrativa permite localizar productos por coincidencias en este campo.

También es utilizado por los flujos orientados a venta/POS para facilitar la identificación rápida del producto.

Actualmente la existencia de un código de barras en el producto no implica la existencia de un catálogo maestro global de códigos de barras de Veltika.

Ese concepto corresponde a una posible evolución futura independiente.

---

# 17. Reglas de negocio

1. Cada producto pertenece a una única empresa.
2. Cada producto pertenece a una categoría de esa misma empresa.
3. La categoría debe encontrarse activa al crear o editar el producto.
4. La empresa debe encontrarse activa.
5. El nombre debe ser único dentro de la empresa.
6. Un producto nuevo comienza activo.
7. El stock no puede ser negativo.
8. El precio de costo no puede ser negativo.
9. El precio de venta no puede ser negativo.
10. El punto de reposición no puede ser negativo.
11. El stock inicial positivo debe generar un movimiento de tipo `StockInicial`.
12. El stock posterior al alta no se modifica desde la edición general del producto.
13. La baja es lógica.
14. La información histórica de ventas, compras y stock debe preservarse.
15. Un `AdminEmpresa` sólo puede operar productos de su empresa.
16. Las categorías nunca pueden cruzarse entre empresas.
17. Las imágenes son opcionales y deben gestionarse mediante el servicio de imágenes.

---

# 18. Casos de error relevantes

- Nombre vacío.
- Nombre con más de 100 caracteres.
- Producto duplicado dentro de la empresa.
- Empresa inexistente o inactiva.
- Categoría inexistente.
- Categoría inactiva.
- Categoría perteneciente a otra empresa.
- Precio de costo negativo.
- Precio de venta negativo.
- Stock negativo.
- Punto de reposición negativo.
- Imagen inválida o error durante su almacenamiento.
- Producto inexistente.
- Intento de acceso a un producto de otra empresa.
- ID inconsistente durante edición.
- Archivo Excel inválido.
- Vista previa de importación vencida.
- Error durante la importación.

---

# 19. Integraciones actuales

El módulo se integra con:

- Empresa.
- Categoría.
- Ventas.
- DetalleVenta.
- Compras.
- DetalleCompra.
- MovimientoStock.
- Ajustes de stock.
- Reintegros/devoluciones.
- Dashboard.
- Reportes de stock.
- Valorización de inventario.
- POS.
- Servicio de imágenes.
- Importación Excel.

---

# 20. Capacidades no implementadas

Los siguientes conceptos continúan siendo futuros y no deben confundirse con el estado actual:

- SKU específico separado del código de barras.
- Variantes.
- Productos compuestos.
- Packs o kits con estructura propia.
- Series.
- Lotes.
- Vencimientos.
- Múltiples imágenes por producto.
- Historial explícito de cambios de precio.
- Listas de precios.
- Stock máximo.
- Unidad de medida configurable.
- Marca y modelo estructurados.
- Datos tributarios por producto.
- Ubicación física en depósito.
- Catálogo maestro global de códigos de barras.
- Integración directa con eCommerce.
- API pública de productos.

---

# 21. Evolución futura

La evolución del módulo se gestiona mediante el Roadmap general y GitHub Issues.

Entre las mejoras posibles se encuentran:

- Historial de costos y precios.
- Listas de precios.
- Cambios masivos de precios.
- Margen estimado.
- Reposición sugerida.
- Inventario físico.
- Variantes.
- Unidades de medida.
- Kits y combos.
- Lotes y vencimientos.
- Series.
- Catálogo maestro de códigos de barras.

No se mantiene un roadmap de versiones independiente dentro de este documento.

---

# 22. Estado

✅ CRUD administrativo implementado.

✅ Seguridad multiempresa implementada.

✅ Código de barras implementado.

✅ Punto de reposición implementado.

✅ Imagen de producto implementada.

✅ Stock inicial con trazabilidad implementado.

✅ Baja lógica y reactivación implementadas.

✅ Búsqueda, filtros y paginación implementados.

✅ Importación masiva mediante Excel implementada.

✅ Integración con ventas, compras e inventario implementada.

🚧 Capacidades avanzadas de catálogo e inventario reservadas para evolución post-MVP.