# Módulo ProductoImportacion

Última actualización: 01/09/2026

---

# 1. Objetivo

ProductoImportacion permite realizar el alta masiva inicial de Productos mediante una plantilla Excel `.xlsx`.

El flujo está diseñado en dos etapas:

```text
Archivo Excel
    -> Analizar
    -> Vista previa y validaciones
    -> Confirmar
    -> Alta atómica de Productos y Stock inicial
```

No modifica Productos existentes.

---

# 2. Arquitectura actual

El módulo utiliza:

- `ProductoImportacionController`.
- `IProductoImportacionService`.
- `ProductoImportacionService`.
- ViewModels específicos de importación.
- ClosedXML para leer y generar archivos Excel.
- `IMemoryCache` para mantener temporalmente la vista previa.

La lógica principal de importación se encuentra en el Service y no directamente en el Controller.

---

# 3. Autorización

`ProductoImportacionController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Además se valida la Empresa permitida para el Usuario en backend.

---

# 4. Multiempresa

Para AdminEmpresa:

```text
EmpresaId = usuario.EmpresaId
```

El EmpresaId enviado por el formulario no se considera fuente confiable.

Para SuperAdmin se permite seleccionar una Empresa activa.

---

# 5. Empresa válida

La Empresa elegida debe:

```text
existir
Estado == true
```

Si no es válida, no se analiza ni importa el archivo.

---

# 6. Plantilla oficial

El sistema genera una plantilla denominada:

```text
PlantillaProductosVeltika.xlsx
```

Contiene una hoja principal:

```text
Productos
```

y una hoja:

```text
Instrucciones
```

---

# 7. Columnas de la plantilla

Las columnas esperadas son exactamente, en este orden:

```text
Nombre
CodigoBarra
Categoria
PrecioCosto
PrecioVenta
StockInicial
PuntoReposicion
Descripcion
```

No debe modificarse el nombre ni el orden de las columnas.

---

# 8. Formato de CodigoBarra

La columna `CodigoBarra` se genera con formato de texto en Excel.

Esto evita que Excel altere códigos con:

- ceros iniciales.
- números largos.
- formatos científicos.

---

# 9. Archivo admitido

Actualmente sólo se admite:

```text
.xlsx
```

No se aceptan otros formatos como:

```text
.xls
.csv
```

---

# 10. Tamaño máximo

El archivo no puede superar:

```text
5 MB
```

Tampoco puede estar vacío.

---

# 11. Selección de hoja

El Service intenta utilizar primero una hoja llamada:

```text
Productos
```

sin distinguir mayúsculas/minúsculas.

Si no existe, utiliza la primera hoja del libro.

En cualquier caso, los encabezados deben ser válidos.

---

# 12. Validación de encabezados

Los ocho encabezados se validan uno por uno.

Si una columna no coincide con el nombre esperado, el análisis se rechaza indicando qué columna es incorrecta.

---

# 13. Filas vacías

Una fila donde las ocho columnas estén vacías se ignora.

No genera Producto ni error.

---

# 14. Nombre

`Nombre` es obligatorio.

Reglas actuales:

```text
máximo 100 caracteres
Trim
único dentro del archivo
único dentro de la Empresa
```

La comparación de duplicados no distingue mayúsculas/minúsculas.

---

# 15. Productos existentes

La importación es exclusivamente de alta.

Si ya existe un Producto con el mismo Nombre dentro de la Empresa, la fila queda con error.

No se actualiza ni sobrescribe el Producto existente.

---

# 16. CodigoBarra

`CodigoBarra` es opcional.

Si se informa:

```text
máximo 100 caracteres
único dentro del archivo
único dentro de la Empresa
```

La comparación no distingue mayúsculas/minúsculas.

---

# 17. Categoria

La Categoria indicada debe:

```text
existir
pertenecer a la Empresa
Estado == true
```

La búsqueda por Nombre no distingue mayúsculas/minúsculas.

---

# 18. Categoria vacía

Si la columna Categoria está vacía, el sistema intenta utilizar:

```text
Sin categoría
```

La Empresa debe poseer una Categoria activa con ese Nombre.

Si no existe, la fila queda con error.

---

# 19. PrecioCosto

`PrecioCosto` es opcional.

Si está vacío:

```text
PrecioCosto = 0
```

Si posee valor, debe ser un decimal entre:

```text
0
999999999,99
```

---

# 20. PrecioVenta

`PrecioVenta` es obligatorio.

Debe ser un decimal entre:

```text
0
999999999,99
```

Actualmente el Service permite valor 0; la obligatoriedad significa que la celda debe contener un valor numérico válido.

---

# 21. StockInicial

`StockInicial` es opcional.

Si está vacío:

```text
StockInicial = 0
```

Si posee valor debe ser:

```text
entero >= 0
```

No admite cantidades negativas ni decimales.

---

# 22. PuntoReposicion

`PuntoReposicion` es opcional.

Si está vacío:

```text
PuntoReposicion = 0
```

Si posee valor debe ser:

```text
entero >= 0
```

---

# 23. Descripcion

`Descripcion` es opcional.

Se normaliza mediante Trim.

Si queda vacía se interpreta como null.

Longitud máxima en importación:

```text
500 caracteres
```

---

# 24. Vista previa

Analizar el archivo no persiste Productos todavía.

Genera una `ProductoImportacionVistaPreviaVM` con:

- Token.
- EmpresaId.
- NombreArchivo.
- Filas analizadas.
- errores por fila.

Esto permite revisar el resultado antes de confirmar.

---

# 25. Token de vista previa

Cada análisis genera un Token aleatorio:

```text
Guid.NewGuid().ToString("N")
```

El Token identifica temporalmente esa importación.

---

# 26. Seguridad de la vista previa

La vista previa queda ligada a:

```text
Token
EmpresaId
UsuarioId
```

No alcanza con conocer el Token para confirmar una importación.

También deben coincidir Empresa y Usuario.

---

# 27. Duración de la vista previa

La vista previa permanece en memoria durante:

```text
30 minutos
```

Luego vence.

Si venció, el Usuario debe analizar nuevamente el archivo.

---

# 28. Almacenamiento temporal

Actualmente la vista previa se almacena mediante:

```text
IMemoryCache
```

No se persiste en SQL Server.

Por lo tanto es información temporal del proceso de importación.

---

# 29. Importación con errores

Si la vista previa contiene errores:

```text
PuedeImportar == false
```

La confirmación se rechaza.

No se importan parcialmente las filas válidas.

---

# 30. Política todo o nada

La importación actual sigue una política:

```text
Todas las filas válidas
    -> se puede confirmar

Una o más filas con error
    -> no se confirma ninguna
```

Esto evita terminar con altas parciales inesperadas.

---

# 31. Revalidación al confirmar

Entre Analizar y Confirmar pueden cambiar datos de la Empresa.

Por eso al confirmar se vuelven a validar condiciones críticas.

---

# 32. Revalidación de duplicados

Antes de importar se vuelve a consultar la base para verificar que no hayan aparecido Productos con:

```text
mismo Nombre
mismo CodigoBarra informado
```

desde que se generó la vista previa.

Si existe un duplicado nuevo, la importación se cancela y debe analizarse nuevamente.

---

# 33. Revalidación de Categorias

Antes de importar también se verifica que todas las Categorias utilizadas:

```text
sigan existiendo
pertenezcan a la Empresa
continúen activas
```

Si alguna dejó de estar disponible, la importación se cancela.

---

# 34. Alta de Producto

Por cada fila válida se crea un nuevo `Producto` con:

```text
Nombre
CodigoBarra
CategoriaId
PrecioCosto
PrecioVenta
Stock = StockInicial
PuntoReposicion
Descripcion
Estado = true
FechaAlta = fecha común de importación
EmpresaId
```

---

# 35. Productos activos

Todos los Productos importados comienzan con:

```text
Estado = true
```

---

# 36. Fecha de alta

La importación toma una única:

```text
DateTime.Now
```

antes de crear los Productos.

Las filas de una misma confirmación comparten esa fecha de operación.

---

# 37. Stock inicial

El valor de `StockInicial` se asigna directamente al campo:

```text
Producto.Stock
```

Pero además se genera trazabilidad mediante MovimientoStock cuando el valor es mayor a cero.

---

# 38. MovimientoStock inicial

Si:

```text
StockInicial > 0
```

se registra:

```text
Tipo = StockInicial
Cantidad = StockInicial
StockAnterior = 0
StockPosterior = StockInicial
Motivo = "Stock inicial por importación"
Fecha = fecha de importación
UsuarioId = usuario actual
EmpresaId = Empresa importada
```

---

# 39. StockInicial igual a cero

Si:

```text
StockInicial == 0
```

el Producto se crea con Stock 0 pero no se genera MovimientoStock inicial.

---

# 40. Relación Producto-MovimientoStock durante el alta

El MovimientoStock se relaciona directamente con la instancia de `Producto` antes de que ésta tenga su Id definitivo.

Entity Framework resuelve la relación al guardar.

Esto permite persistir Producto y trazabilidad dentro de la misma unidad de trabajo.

---

# 41. Transacción de importación

La creación masiva utiliza una transacción de base de datos.

Dentro de ella se crean:

```text
Productos
+
MovimientosStock iniciales correspondientes
```

---

# 42. Atomicidad

Si ocurre una excepción durante el guardado:

```text
Rollback
```

El mensaje funcional del Controller indica:

```text
Ocurrió un error y no se importó ningún producto.
```

La intención del flujo es evitar importaciones parciales.

---

# 43. Finalización correcta

Después de una importación exitosa:

- se hace Commit.
- se elimina la vista previa del cache.
- se informa la cantidad de Productos importados.
- se redirige al Index de Producto.

---

# 44. Reutilización del Token

Una vez completada exitosamente la importación, el Token se elimina del cache.

No debe utilizarse para importar nuevamente las mismas filas.

---

# 45. Imágenes

La versión actual no importa imágenes de Producto.

La plantilla lo informa explícitamente.

La carga de imágenes continúa siendo un proceso separado.

---

# 46. Qué no hace actualmente

ProductoImportacion no implementa:

- actualización masiva de Productos existentes.
- actualización masiva de precios.
- actualización masiva de Stock existente.
- importación de imágenes.
- creación automática de Categorias indicadas en Excel.
- importación CSV.
- importación parcial de sólo las filas correctas.
- persistencia histórica de lotes de importación.

---

# 47. Seguridad

Las reglas críticas se validan en backend:

- Usuario autenticado.
- Rol autorizado.
- Empresa activa.
- alcance multiempresa.
- formato `.xlsx`.
- tamaño máximo.
- encabezados esperados.
- validaciones por fila.
- unicidad de Nombre.
- unicidad de CodigoBarra informado.
- Categoria activa y perteneciente a Empresa.
- Token ligado a Empresa y Usuario.
- revalidación de duplicados al confirmar.
- revalidación de Categorias al confirmar.
- transacción para el guardado masivo.

---

# 48. Reglas de negocio actuales

1. La importación crea Productos nuevos; no actualiza existentes.
2. Sólo se admite Excel `.xlsx`.
3. El archivo no puede superar 5 MB.
4. La estructura de ocho columnas es fija.
5. Nombre es obligatorio y máximo 100 caracteres.
6. Nombre debe ser único dentro del archivo y de la Empresa.
7. CodigoBarra es opcional y máximo 100 caracteres.
8. Si se informa CodigoBarra, debe ser único dentro del archivo y de la Empresa.
9. Categoria debe existir, pertenecer a Empresa y estar activa.
10. Categoria vacía utiliza `Sin categoría`.
11. Si no existe `Sin categoría`, la fila es inválida.
12. PrecioCosto vacío se interpreta como 0.
13. PrecioVenta es obligatorio.
14. Precios deben estar entre 0 y 999999999,99.
15. StockInicial vacío se interpreta como 0.
16. StockInicial debe ser entero >= 0.
17. PuntoReposicion vacío se interpreta como 0.
18. PuntoReposicion debe ser entero >= 0.
19. Descripcion es opcional y máximo 500 caracteres en el flujo de importación.
20. Filas completamente vacías se ignoran.
21. Analizar no persiste Productos.
22. La vista previa dura 30 minutos.
23. La vista previa pertenece a una combinación Token + Empresa + Usuario.
24. Una vista previa con errores no puede confirmarse.
25. No existe importación parcial de filas válidas.
26. Duplicados se revalidan al confirmar.
27. Categorias se revalidan al confirmar.
28. Productos importados se crean activos.
29. StockInicial se asigna al Producto.
30. Si StockInicial > 0 se genera MovimientoStock Tipo StockInicial.
31. Producto y MovimientoStock se guardan dentro de la misma transacción.
32. Ante error se realiza Rollback.
33. Tras éxito se elimina el Token del cache.
34. AdminEmpresa sólo importa en su Empresa.
35. SuperAdmin puede seleccionar una Empresa activa.

---

# 49. Evolución futura

Posibles mejoras futuras:

- actualización masiva controlada de Productos existentes.
- importación específica de listas de precios.
- importación de ajustes de Stock.
- soporte CSV.
- historial persistido de lotes de importación.
- descarga de reporte de errores.
- procesamiento de archivos mayores mediante estrategia escalable.
- importación de variantes/unidades cuando existan esos módulos.
- integración con catálogo maestro por CodigoBarra cuando se implemente y valide esa funcionalidad.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 50. Estado actual

✅ Plantilla Excel oficial implementada.

✅ Descarga de plantilla implementada.

✅ Validación `.xlsx` y tamaño máximo implementada.

✅ Validación estricta de columnas implementada.

✅ Vista previa implementada.

✅ Errores por fila implementados.

✅ Detección de duplicados dentro del archivo implementada.

✅ Detección de duplicados contra base implementada.

✅ Validación de Categorias implementada.

✅ Categoria base `Sin categoría` soportada.

✅ Vista previa ligada a Usuario y Empresa implementada.

✅ Expiración a 30 minutos implementada.

✅ Revalidación antes de Confirmar implementada.

✅ Importación transaccional todo-o-nada implementada.

✅ Creación de MovimientoStock para Stock inicial implementada.

✅ Multiempresa implementado.

🚧 Actualización masiva de Productos existentes pendiente.

🚧 Importación de imágenes pendiente.

🚧 Historial persistido de lotes pendiente.