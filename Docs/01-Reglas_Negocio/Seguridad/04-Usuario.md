# Módulo Usuario

---

# 1. Objetivo

El módulo Usuario permite administrar las personas que acceden al sistema Veltika.

Cada usuario pertenece a una empresa y posee un rol que determina las acciones que puede realizar dentro de la aplicación.

Este módulo garantiza la seguridad, el control de acceso y la trazabilidad de todas las operaciones realizadas en el sistema.

---

# 2. Alcance

El módulo permite registrar, modificar, activar, desactivar y consultar usuarios pertenecientes a una empresa.

Además, administra la información personal, las credenciales de acceso y la relación con los roles y permisos definidos en el sistema.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa

---

# 4. Permisos

## Super Administrador

✅ Visualizar usuarios de cualquier empresa

✅ Crear usuarios

✅ Editar usuarios

✅ Activar usuarios

✅ Desactivar usuarios

✅ Restablecer contraseñas

## Administrador de Empresa

✅ Crear usuarios de su empresa

✅ Editar usuarios de su empresa

✅ Activar usuarios

✅ Desactivar usuarios

✅ Consultar usuarios

✅ Restablecer contraseñas

❌ Acceder a usuarios de otras empresas

---

# 5. Funcionalidades

Actualmente

- Registrar usuario
- Editar usuario
- Activar usuario
- Desactivar usuario
- Consultar usuarios
- Buscar usuarios
- Asignar rol
- Cambiar contraseña

Versiones futuras

- Foto de perfil
- Firma digital
- Doble factor de autenticación (2FA)
- Inicio de sesión con Google
- Inicio de sesión con Microsoft
- Historial de accesos
- Bloqueo automático por intentos fallidos
- Gestión de sesiones activas
- Notificaciones de inicio de sesión
- Recuperación de contraseña por correo

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa a la que pertenece |
| Nombre | Nombre del usuario |
| Apellido | Apellido del usuario |
| Email | Correo electrónico |
| Password | Contraseña encriptada |
| ImagenPerfil | Imagen del usuario |
| RolId | Rol asignado |
| Estado | Activo o Inactivo |

Campos futuros

- Teléfono
- Documento
- FechaNacimiento
- ÚltimoAcceso
- IntentosFallidos
- TokenRecuperación
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El apellido es obligatorio.
- El correo electrónico es obligatorio.
- El correo electrónico debe tener un formato válido.
- No puede existir otro usuario con el mismo correo dentro de la misma empresa.
- La contraseña debe cumplir con la política de seguridad definida.
- El rol debe existir.
- La empresa debe existir.

---

# 8. Reglas de negocio

- Cada usuario pertenece a una única empresa.
- Cada usuario posee un único rol.
- Un rol puede estar asignado a múltiples usuarios.
- Un usuario desactivado no podrá iniciar sesión.
- El Super Administrador podrá administrar todas las empresas.
- Los administradores únicamente podrán administrar usuarios de su propia empresa.
- Todas las acciones realizadas por un usuario quedarán registradas en auditoría.

---

# 9. Casos de uso

## Crear usuario

El administrador registra un nuevo usuario dentro de la empresa.

Resultado esperado:

- Usuario creado correctamente.
- El usuario queda habilitado para acceder al sistema.

---

## Editar usuario

Permite modificar la información personal o el rol del usuario.

---

## Desactivar usuario

El usuario deja de poder acceder al sistema.

La información histórica permanece registrada.

---

## Restablecer contraseña

Permite generar una nueva contraseña para un usuario.

---

## Consultar usuarios

Permite visualizar todos los usuarios registrados de la empresa.

---

# 10. Casos de error

- Nombre vacío.
- Apellido vacío.
- Correo electrónico inválido.
- Usuario duplicado.
- Rol inexistente.
- Empresa inexistente.
- Usuario sin permisos.
- Intento de modificar usuarios de otra empresa.

---

# 11. Flujo funcional

1. El administrador ingresa al módulo Usuarios.
2. Selecciona "Nuevo Usuario".
3. Completa la información requerida.
4. Asigna un rol.
5. El sistema valida los datos.
6. Se registra el usuario.
7. El usuario queda habilitado para iniciar sesión.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Roles
- Permisos
- Sucursales
- Ventas
- Compras
- Caja
- Auditoría
- Reportes

---

# 13. Mejoras futuras

- Doble autenticación (2FA).
- Inicio de sesión con Google.
- Inicio de sesión con Microsoft.
- Gestión de dispositivos autorizados.
- Control de sesiones activas.
- Historial de accesos.
- Firma digital.
- Notificaciones de seguridad.
- Gestión avanzada de contraseñas.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación
- Consulta
- Cambio de contraseña

Versión 2.0

- Foto de perfil
- Recuperación de contraseña
- Historial de accesos
- Bloqueo automático

Versión 3.0

- Doble autenticación
- Inicio de sesión con Google
- Inicio de sesión con Microsoft
- Gestión de sesiones
- Seguridad avanzadaç