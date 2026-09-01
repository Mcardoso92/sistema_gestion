# Módulo Categoría

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Categoría permite organizar los productos de una empresa mediante grupos o clasificaciones.

Cada categoría pertenece a una única empresa y forma parte del catálogo comercial de esa empresa.

El módulo debe preservar en todo momento el aislamiento multiempresa y evitar que un usuario pueda consultar o modificar categorías pertenecientes a otra organización.

---

# 2. Alcance actual

El módulo permite:

- Listar categorías.
- Buscar categorías por nombre.
- Filtrar categorías por estado.
- Filtrar por empresa cuando opera un `SuperAdmin`.
- Consultar el detalle de una categoría.
- Crear categorías.
- Editar categorías.
- Desactivar categorías mediante baja lógica.
- Reactivar categorías desde la edición.

Las categorías no almacenan stock ni precios. Su función es clasificar productos.

---

# 3. Actores y permisos

El controller está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede:

- Visualizar categorías de todas las empresas.
- Filtrar el listado por empresa.
- Crear categorías para una empresa activa.
- Editar categorías de cualquier empresa.
- Cambiar la empresa asociada a una categoría durante la edición, siempre que la empresa destino sea válida y activa.
- Desactivar categorías.
- Reactivar categorías.

## AdminEmpresa

Puede:

- Visualizar únicamente categorías de su propia empresa.
- Crear categorías para su propia empresa.
- Editar categorías de su propia empresa.
- Desactivar categorías de su propia empresa.
- Reactivar categorías de su propia empresa.

No puede seleccionar ni modificar manualmente `EmpresaId`.

El servidor fuerza siempre:

```text
categoria.EmpresaId = usuarioLogueado.EmpresaId
```

para usuarios que no son `SuperAdmin`.

---

# 4. Modelo actual

La entidad `Categoria` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| Nombre | string | Obligatorio, máximo 50 caracteres |
| Estado | bool | Indica si la categoría está activa |
| EmpresaId | int | Empresa propietaria |

Relaciones:

- Una categoría pertenece a una empresa.
- Una categoría puede estar asociada a múltiples productos.

---

# 5. Listado y consulta

El listado muestra categorías activas por defecto.

Permite filtrar por:

- Activas.
- Inactivas.
- Todas.
- Texto contenido en el nombre.

Para `SuperAdmin` también permite filtrar por empresa.

Para `AdminEmpresa`, el filtro por empresa no es configurable: la consulta se restringe automáticamente a la empresa del usuario autenticado.

Las categorías se ordenan alfabéticamente por nombre.

La paginación actual utiliza 20 registros por página.

El listado utiliza `AsNoTracking()` al tratarse de una consulta de lectura.

---

# 6. Creación

## Datos ingresados

Se recibe:

- Nombre.
- EmpresaId únicamente cuando corresponde al flujo de `SuperAdmin`.

Para `AdminEmpresa`, cualquier `EmpresaId` recibido desde el cliente es reemplazado por el `EmpresaId` del usuario autenticado.

## Estado inicial

Toda categoría nueva se crea con:

```text
Estado = true
```

## Validación de empresa

La empresa asignada debe:

- Existir.
- Estar activa.

Si la empresa no cumple estas condiciones, la categoría no puede crearse.

## Duplicados

No puede existir otra categoría con el mismo nombre dentro de la misma empresa, ignorando diferencias entre mayúsculas y minúsculas.

El mismo nombre sí puede existir en empresas distintas.

---

# 7. Validaciones

- El nombre es obligatorio.
- El nombre admite como máximo 50 caracteres.
- La empresa asignada debe existir.
- La empresa asignada debe encontrarse activa.
- No puede existir otra categoría con el mismo nombre dentro de la misma empresa.
- Un `AdminEmpresa` no puede asignar la categoría a otra empresa.
- El `Id` recibido al editar debe coincidir con el `Id` de la ruta.

---

# 8. Edición y reactivación

Durante la edición pueden modificarse:

- Nombre.
- Estado.

Para `SuperAdmin` también puede modificarse:

- EmpresaId.

Para `AdminEmpresa`, el `EmpresaId` permanece forzado a su propia empresa.

Antes de guardar se vuelve a validar:

- Empresa activa y válida.
- Nombre no duplicado dentro de la empresa.
- Acceso del usuario al registro.

Una categoría inactiva puede reactivarse estableciendo nuevamente:

```text
Estado = true
```

---

# 9. Desactivación

La desactivación es lógica:

```text
Estado = false
```

La categoría no se elimina físicamente.

## Restricción por productos activos

Una categoría no puede desactivarse si posee productos activos asociados.

La validación actual verifica:

```text
Producto.CategoriaId == categoria.Id
&& Producto.Estado == true
```

Si existen productos activos asociados, la operación se rechaza y la categoría permanece activa.

Los productos inactivos no impiden actualmente la desactivación.

---

# 10. Seguridad multiempresa

La seguridad se aplica en servidor.

Para usuarios que no son `SuperAdmin`, las consultas de detalle, edición y desactivación se restringen mediante:

```text
Categoria.EmpresaId == usuario.EmpresaId
```

Si el registro solicitado no pertenece a la empresa del usuario, no se devuelve la categoría.

La aplicación no debe confiar en `EmpresaId` enviado desde formularios, query strings o JavaScript para usuarios de empresa.

---

# 11. Reglas de negocio

1. Cada categoría pertenece a una única empresa.
2. Una empresa puede tener múltiples categorías.
3. Una categoría puede clasificar múltiples productos.
4. El nombre debe ser único dentro de cada empresa.
5. Categorías de empresas distintas pueden compartir nombre.
6. Una categoría nueva comienza activa.
7. Una categoría sólo puede asociarse a una empresa activa.
8. Un `AdminEmpresa` sólo puede operar categorías de su empresa.
9. Un `AdminEmpresa` no puede cambiar la empresa propietaria de una categoría.
10. Una categoría no puede desactivarse mientras tenga productos activos asociados.
11. La desactivación es lógica y no elimina relaciones históricas.
12. Una categoría desactivada puede reactivarse mediante edición.

---

# 12. Categoría predeterminada

Al inicializar una empresa, Veltika crea automáticamente la categoría:

```text
Sin categoría
```

si todavía no existe.

Esta categoría funciona como categoría base de la empresa y forma parte de la inicialización automática realizada por `EmpresaInicializacionService`.

Cualquier cambio futuro sobre el tratamiento especial de esta categoría deberá documentarse explícitamente.

---

# 13. Casos de uso

## Crear categoría como AdminEmpresa

1. El usuario autenticado ingresa al módulo.
2. Selecciona crear categoría.
3. Ingresa el nombre.
4. El servidor obtiene la empresa desde el usuario autenticado.
5. Valida empresa y duplicados.
6. Crea la categoría activa.

## Crear categoría como SuperAdmin

1. El usuario selecciona una empresa activa.
2. Ingresa el nombre.
3. El sistema valida empresa y duplicados.
4. Crea la categoría activa para la empresa seleccionada.

## Editar categoría

El usuario modifica los datos permitidos según su rol.

## Reactivar categoría

Una categoría inactiva puede volver a estado activo desde edición.

## Desactivar categoría

El sistema verifica primero que no existan productos activos asociados.

Si existen, la operación se bloquea.

---

# 14. Casos de error relevantes

- Nombre vacío.
- Nombre con más de 50 caracteres.
- Nombre duplicado dentro de la empresa.
- Empresa inexistente.
- Empresa inactiva.
- Categoría inexistente.
- Intento de operar una categoría de otra empresa.
- ID inconsistente durante edición.
- Intento de desactivar una categoría con productos activos.
- Error de persistencia durante alta, edición o baja lógica.

---

# 15. Integraciones

El módulo se relaciona actualmente con:

- Empresa.
- Producto.
- Inicialización automática de empresa.
- Seguridad mediante Identity y roles.
- Filtros y consultas administrativas.

La relación con Producto es especialmente importante porque determina la posibilidad de desactivar una categoría.

---

# 16. Capacidades no implementadas

Los siguientes conceptos no forman parte del modelo actual de Categoría:

- Descripción.
- Color.
- Ícono.
- Orden personalizado.
- Categoría padre.
- Subcategorías.
- Jerarquía de categorías.
- Fecha de alta propia.
- Fecha de modificación.
- Usuario de alta o modificación.
- Categorías favoritas.
- Estadísticas específicas de uso.

La importación masiva actual de productos no implica que exista una funcionalidad independiente de importación masiva de categorías.

---

# 17. Evolución futura

Las mejoras futuras deberán gestionarse mediante Roadmap y GitHub Issues.

Posibles evoluciones:

- Jerarquías o subcategorías si existe necesidad real.
- Mejoras de organización visual.
- Estadísticas por categoría.
- Herramientas de mantenimiento masivo.

No se mantiene un roadmap de versiones independiente dentro de este documento.

---

# 18. Estado

✅ CRUD administrativo implementado.

✅ Seguridad por empresa implementada.

✅ Roles `SuperAdmin` y `AdminEmpresa` implementados.

✅ Baja lógica y reactivación implementadas.

✅ Validación de duplicados por empresa implementada.

✅ Restricción de desactivación con productos activos implementada.

✅ Búsqueda, filtros y paginación implementados.

✅ Categoría predeterminada incluida en inicialización de empresa.