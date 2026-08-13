# Módulo Categoría

---

# 1. Objetivo

El módulo Categoría permite organizar los productos de una empresa mediante grupos o clasificaciones.

Su finalidad es facilitar la administración del catálogo de productos, mejorar la búsqueda de información y generar reportes más precisos.

Cada categoría pertenece exclusivamente a una empresa.

---

# 2. Alcance

El módulo permite crear, modificar, activar, desactivar y consultar categorías utilizadas para clasificar los productos del sistema.

Las categorías funcionan como un nivel organizativo y no contienen stock ni información comercial.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa

---

# 4. Permisos

## Super Administrador

✅ Visualizar categorías de cualquier empresa

✅ Crear categorías

✅ Editar categorías

✅ Activar categorías

✅ Desactivar categorías

## Administrador de Empresa

✅ Crear categorías

✅ Editar categorías

✅ Activar categorías

✅ Desactivar categorías

✅ Consultar categorías

❌ Acceder a categorías de otras empresas

---

# 5. Funcionalidades

Actualmente

- Registrar categoría
- Editar categoría
- Activar categoría
- Desactivar categoría
- Consultar categorías
- Buscar categorías

Versiones futuras

- Categorías jerárquicas
- Subcategorías
- Orden personalizado
- Color identificador
- Ícono de categoría
- Categorías favoritas
- Importación masiva
- Exportación

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| Nombre | Nombre de la categoría |
| Estado | Activa o inactiva |

Campos futuros

- Descripción
- Color
- Icono
- Orden
- CategoriaPadreId
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El nombre no puede superar la longitud máxima definida.
- No puede existir otra categoría con el mismo nombre dentro de la misma empresa.
- El estado inicial será Activo.
- La empresa debe existir.

---

# 8. Reglas de negocio

- Cada categoría pertenece a una única empresa.
- Una empresa puede tener múltiples categorías.
- Una categoría puede estar asociada a múltiples productos.
- No se pueden visualizar categorías pertenecientes a otra empresa.
- Una categoría desactivada no podrá asignarse a nuevos productos.
- La desactivación no elimina la información relacionada.

---

# 9. Casos de uso

## Crear categoría

El usuario registra una nueva categoría para organizar sus productos.

Resultado esperado:

- Categoría creada correctamente.
- Disponible para asignar a productos.

---

## Editar categoría

Permite modificar el nombre u otros datos de la categoría.

---

## Desactivar categoría

La categoría deja de estar disponible para nuevas asignaciones.

Los productos existentes mantienen la referencia.

---

## Consultar categorías

Permite visualizar todas las categorías registradas por la empresa.

---

# 10. Casos de error

- Nombre vacío.
- Categoría duplicada.
- Empresa inexistente.
- Usuario sin permisos.
- Intento de acceder a categorías de otra empresa.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Categorías.
2. Selecciona "Nueva Categoría".
3. Completa el nombre.
4. El sistema valida la información.
5. Se registra la categoría.
6. La categoría queda disponible para asociar productos.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Productos
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Subcategorías ilimitadas.
- Árbol de categorías.
- Colores e íconos personalizados.
- Reordenamiento mediante drag & drop.
- Estadísticas de uso.
- Categorías inteligentes.
- Importación desde Excel.
- Exportación.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación
- Consulta

Versión 2.0

- Subcategorías
- Colores
- Iconos
- Orden personalizado

Versión 3.0

- Categorías inteligentes
- Estadísticas
- Importación y exportación