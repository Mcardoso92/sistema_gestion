# Módulo Rol

---

# 1. Objetivo

El módulo Rol permite definir los distintos perfiles de acceso que pueden asignarse a los usuarios dentro de Veltika.

Cada rol agrupa un conjunto de permisos que determinan las funcionalidades disponibles para cada usuario, facilitando la administración de la seguridad del sistema.

---

# 2. Alcance

El módulo permite crear, modificar, activar, desactivar y consultar los distintos roles utilizados dentro de una empresa.

Cada usuario deberá tener asignado un único rol.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa

---

# 4. Permisos

## Super Administrador

✅ Crear roles

✅ Editar roles

✅ Activar roles

✅ Desactivar roles

✅ Consultar todos los roles

## Administrador de Empresa

✅ Crear roles personalizados

✅ Editar roles

✅ Activar roles

✅ Desactivar roles

✅ Consultar roles

❌ Modificar roles del sistema (futuro)

---

# 5. Funcionalidades

Actualmente

- Registrar rol
- Editar rol
- Activar rol
- Desactivar rol
- Consultar roles
- Asignar rol a usuarios

Versiones futuras

- Roles predefinidos
- Copiar rol
- Duplicar permisos
- Exportar configuración
- Importar configuración
- Roles temporales
- Roles por sucursal
- Roles compartidos entre empresas

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| Nombre | Nombre del rol |
| Descripcion | Descripción del rol |
| Estado | Activo o Inactivo |

Campos futuros

- EsRolSistema
- Prioridad
- Color
- Icono
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El nombre no puede repetirse dentro de la misma empresa.
- La descripción es opcional.
- La empresa debe existir.
- El estado inicial será Activo.

---

# 8. Reglas de negocio

- Cada rol pertenece a una única empresa.
- Un rol puede estar asignado a múltiples usuarios.
- Todo usuario debe tener un rol asignado.
- Un rol desactivado no podrá asignarse a nuevos usuarios.
- No podrán eliminarse roles que tengan usuarios asociados.
- La eliminación física no estará permitida.

---

# 9. Casos de uso

## Crear rol

El administrador registra un nuevo rol para la empresa.

Resultado esperado:

- Rol creado correctamente.
- Disponible para asignar usuarios.

---

## Editar rol

Permite modificar el nombre o descripción del rol.

---

## Desactivar rol

El rol deja de estar disponible para nuevas asignaciones.

Los usuarios existentes mantienen su rol hasta ser modificado.

---

## Consultar roles

Permite visualizar todos los roles registrados.

---

# 10. Casos de error

- Nombre vacío.
- Rol duplicado.
- Empresa inexistente.
- Usuario sin permisos.
- Intento de desactivar un rol crítico del sistema (futuro).
- Intento de eliminar un rol con usuarios asociados.

---

# 11. Flujo funcional

1. El administrador ingresa al módulo Roles.
2. Selecciona "Nuevo Rol".
3. Completa la información.
4. El sistema valida los datos.
5. Se registra el rol.
6. El rol queda disponible para asignar a usuarios.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Usuarios
- Permisos
- Auditoría
- Reportes

---

# 13. Mejoras futuras

- Roles del sistema.
- Roles personalizados.
- Copia de roles.
- Roles por sucursal.
- Roles temporales.
- Importación y exportación.
- Plantillas de roles.
- Biblioteca de permisos.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación
- Consulta
- Asignación a usuarios

Versión 2.0

- Copia de roles
- Roles predefinidos
- Plantillas
- Exportación

Versión 3.0

- Roles por sucursal
- Roles temporales
- Biblioteca de permisos
- Configuración avanzada