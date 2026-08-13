# Módulo Permiso

---

# 1. Objetivo

El módulo Permiso permite definir las acciones específicas que pueden realizar los usuarios dentro de Veltika.

Cada permiso representa una funcionalidad individual del sistema y puede ser asignado a uno o varios roles, proporcionando un control preciso sobre el acceso a cada módulo.

Este módulo constituye la base del sistema de autorización de Veltika.

---

# 2. Alcance

El módulo permite administrar los permisos disponibles en el sistema y establecer su relación con los distintos roles.

Cada rol podrá poseer múltiples permisos, determinando las operaciones habilitadas para los usuarios que lo integran.

---

# 3. Actores

- Super Administrador

---

# 4. Permisos

## Super Administrador

✅ Crear permisos

✅ Editar permisos

✅ Activar permisos

✅ Desactivar permisos

✅ Consultar permisos

✅ Asignar permisos a roles

## Administrador de Empresa

✅ Consultar permisos

❌ Crear permisos

❌ Editar permisos

❌ Eliminar permisos

❌ Modificar permisos del sistema

---

# 5. Funcionalidades

Actualmente

- Registrar permiso
- Editar permiso
- Activar permiso
- Desactivar permiso
- Consultar permisos
- Asociar permisos a roles

Versiones futuras

- Agrupación por módulos
- Permisos personalizados
- Herencia de permisos
- Permisos temporales
- Exportación de configuraciones
- Importación de configuraciones
- Auditoría de modificaciones

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| Nombre | Nombre del permiso |
| Descripcion | Descripción del permiso |
| Modulo | Módulo al que pertenece |
| Estado | Activo o Inactivo |

Campos futuros

- Codigo
- Categoria
- EsPermisoSistema
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El nombre no puede repetirse.
- El módulo es obligatorio.
- El estado inicial será Activo.
- Solo el Super Administrador podrá administrar permisos.

---

# 8. Reglas de negocio

- Un permiso puede pertenecer a múltiples roles.
- Un rol puede contener múltiples permisos.
- Los permisos definen las acciones permitidas dentro del sistema.
- Un permiso desactivado dejará de otorgar acceso automáticamente.
- Los permisos del sistema no podrán eliminarse físicamente.
- La eliminación física de permisos no estará permitida.

---

# 9. Casos de uso

## Crear permiso

El Super Administrador registra un nuevo permiso.

Resultado esperado:

- Permiso creado correctamente.
- Disponible para asignarse a uno o varios roles.

---

## Editar permiso

Permite modificar el nombre o descripción del permiso.

---

## Desactivar permiso

El permiso deja de estar disponible para futuras asignaciones.

---

## Consultar permisos

Permite visualizar todos los permisos registrados en el sistema.

---

## Asignar permiso a un rol

Permite asociar uno o varios permisos a un rol determinado.

Resultado esperado:

- El rol obtiene acceso a las funcionalidades correspondientes.

---

# 10. Casos de error

- Nombre vacío.
- Permiso duplicado.
- Módulo inexistente.
- Usuario sin permisos.
- Intento de modificar un permiso del sistema.
- Intento de eliminar un permiso utilizado por un rol.

---

# 11. Flujo funcional

1. El Super Administrador ingresa al módulo Permisos.
2. Selecciona "Nuevo Permiso".
3. Completa la información.
4. El sistema valida los datos.
5. Se registra el permiso.
6. El permiso queda disponible para ser asignado a los distintos roles.

---

# 12. Integraciones

Este módulo se relaciona con:

- Roles
- Usuarios
- Empresa
- Auditoría
- Reportes

---

# 13. Mejoras futuras

- Permisos dinámicos.
- Permisos por sucursal.
- Permisos temporales.
- Auditoría completa de modificaciones.
- Plantillas de permisos.
- Agrupación por funcionalidades.
- API de autorización.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación
- Consulta
- Asignación a roles

Versión 2.0

- Agrupación por módulos
- Permisos personalizados
- Auditoría
- Exportación

Versión 3.0

- Permisos por sucursal
- Permisos temporales
- Configuración avanzada
- API de autorización