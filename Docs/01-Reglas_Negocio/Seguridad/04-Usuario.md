# Módulo Usuario

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Usuario administra las identidades que acceden a Veltika, sus credenciales, Empresa, Estado, rol e imagen de perfil.

La autenticación y gestión de credenciales se implementan sobre ASP.NET Core Identity mediante `Usuario : IdentityUser`.

---

# 2. Modelo Usuario

`Usuario` extiende `IdentityUser` y agrega actualmente:

| Campo | Regla |
|---|---|
| Nombre | Obligatorio, máximo 50 caracteres |
| Apellido | Obligatorio, máximo 50 caracteres |
| ImagenPerfil | Opcional, máximo 500 caracteres |
| EmpresaId | Empresa asociada |
| Estado | Activo/Inactivo |
| FechaAlta | Fecha de alta |

Email, UserName, PasswordHash, lockout y demás datos de autenticación pertenecen a ASP.NET Core Identity.

No existe un campo `Password` persistido en texto plano.

---

# 3. Relaciones operativas

Usuario mantiene relaciones de trazabilidad con operaciones como:

- Ventas.
- MovimientosStock.
- Compras y Compras anuladas.
- Aperturas y cierres de TurnoCaja.
- CobrosVenta y anulaciones.
- PagosProveedor y anulaciones.
- ReintegrosVenta y anulaciones.
- ReintegrosProveedor y anulaciones.
- TransferenciasCaja y anulaciones.
- MovimientosCaja.

Estas relaciones permiten identificar usuarios responsables en distintos flujos, pero no equivalen a una auditoría administrativa genérica de todas las acciones del sistema.

---

# 4. Acceso administrativo

`UsuarioController` utiliza a nivel de clase:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Las acciones públicas de Registro, Login y recuperación de contraseña utilizan `AllowAnonymous`.

---

# 5. SuperAdmin

SuperAdmin puede:

- Consultar usuarios de distintas Empresas.
- Filtrar por Empresa.
- Crear usuarios para Empresas activas.
- Asignar roles existentes, incluido SuperAdmin.
- Editar usuarios.
- Cambiar la Empresa de un usuario mediante Edit.
- Activar/desactivar usuarios respetando las protecciones del sistema.

---

# 6. AdminEmpresa

AdminEmpresa administra únicamente usuarios de su propia Empresa.

El sistema fuerza el contexto empresarial desde:

```text
usuarioLogueado.EmpresaId
```

AdminEmpresa:

- No puede consultar usuarios de otras Empresas.
- No puede editar usuarios de otras Empresas.
- No puede asignar el rol SuperAdmin.
- No puede administrar un usuario SuperAdmin.

Estas restricciones se validan en servidor y no dependen únicamente de la interfaz.

---

# 7. Listado de usuarios

El listado permite actualmente filtrar por:

- Estado: activos, inactivos o todos.
- Rol.
- Empresa para SuperAdmin.
- Búsqueda por Nombre, Apellido o Email.

El valor por defecto de Estado es:

```text
activos
```

La paginación actual es de:

```text
20 usuarios por página
```

---

# 8. Visibilidad de SuperAdmin

Cuando el usuario autenticado no es SuperAdmin, el listado excluye explícitamente usuarios que poseen el rol SuperAdmin.

Además, Details/Edit/Delete impiden que un AdminEmpresa administre un usuario SuperAdmin.

---

# 9. Creación de Usuario

La creación administrativa utiliza `UsuarioCreateVM`.

Reglas principales:

1. La Empresa debe existir y estar activa.
2. AdminEmpresa queda forzado a su propia Empresa.
3. El Email no puede existir previamente en Identity.
4. El rol debe existir.
5. AdminEmpresa no puede asignar SuperAdmin.
6. La contraseña debe superar las validaciones configuradas en ASP.NET Core Identity.
7. Se establece `UserName = Email`.
8. Se establece `FechaAlta = DateTime.Now`.
9. El rol se asigna mediante `UserManager.AddToRoleAsync`.

---

# 10. Unicidad del Email

La documentación anterior indicaba unicidad sólo dentro de la misma Empresa.

La implementación actual utiliza:

```text
UserManager.FindByEmailAsync(email)
```

y rechaza el Email si ya pertenece a cualquier usuario existente.

Por lo tanto, el Email es efectivamente único a nivel de la identidad global de Veltika, no sólo dentro de una Empresa.

---

# 11. Roles

Los roles se administran mediante ASP.NET Core Identity (`IdentityRole`).

En los flujos actuales de Usuario se trabaja conceptualmente con un único rol operativo principal por usuario: al editar se obtiene el primer rol actual y, si cambia, se remueve y reemplaza por el nuevo.

No debe confundirse esta implementación con un sistema de permisos granulares por acción, que todavía es evolución futura.

---

# 12. Imagen de perfil

La imagen de perfil ya está implementada y no debe figurar como funcionalidad futura.

Se procesa mediante:

```text
IImagenService
```

Para usuarios se utiliza el contexto lógico:

```text
usuarios
```

junto con EmpresaId y el Id del Usuario.

La ruta resultante se almacena en:

```text
Usuario.ImagenPerfil
```

---

# 13. Fallos durante creación con imagen

Si la creación del Usuario, asignación de rol o persistencia de imagen falla, el flujo intenta evitar dejar información parcial.

Entre otras protecciones:

- Si falla la asignación de rol, se elimina el Usuario recién creado.
- Si la imagen es inválida, se elimina el Usuario creado.
- Si falla la actualización posterior de imagen, se elimina la imagen nueva y el Usuario creado.

---

# 14. Edición de Usuario

La edición permite actualizar actualmente:

- Nombre.
- Apellido.
- Email/UserName.
- Estado.
- Rol.
- Empresa para SuperAdmin.
- Imagen de perfil.

La Empresa debe continuar existiendo y estar activa.

El Email continúa validándose como único globalmente, excluyendo al propio Usuario editado.

---

# 15. Protecciones al editar el propio Usuario

Cuando un usuario administra su propio registro:

- No puede cambiar su propio rol desde este flujo.
- No puede cambiar su Empresa mediante este flujo.
- No puede desactivar su propio Usuario.

Estas reglas reducen el riesgo de que un administrador se quite accidentalmente acceso administrativo o quede fuera de su contexto empresarial.

---

# 16. Cambio de rol

Si el rol cambia durante Edit:

1. Se obtiene el rol actual.
2. Se elimina el rol anterior.
3. Se agrega el nuevo rol.
4. Si la asignación nueva falla, se intenta restaurar el rol anterior.

La operación de edición utiliza una transacción de base de datos alrededor del flujo principal.

---

# 17. Desactivación

`Delete` no elimina físicamente al Usuario.

La confirmación realiza:

```text
usuarioDb.Estado = false
```

Por lo tanto, el comportamiento actual es una desactivación lógica.

No se elimina la identidad ni su información histórica.

---

# 18. Protecciones de desactivación

No se permite:

- Que AdminEmpresa desactive usuarios de otra Empresa.
- Que AdminEmpresa desactive un SuperAdmin.
- Que un usuario se desactive a sí mismo.

---

# 19. Reactivación

La reactivación se realiza mediante Edit estableciendo nuevamente:

```text
Estado = true
```

No existe un flujo independiente obligatorio de Reactivar.

---

# 20. Login

Login es público mediante `AllowAnonymous` y posee rate limiting de autenticación.

El flujo actual:

1. Busca el Usuario por Email.
2. Si no existe, devuelve un mensaje genérico.
3. Si `Estado == false`, rechaza el acceso con el mismo mensaje genérico.
4. Para usuarios no SuperAdmin valida que la Empresa esté activa.
5. Ejecuta `PasswordSignInAsync`.
6. Utiliza `lockoutOnFailure: true`.
7. Si autentica correctamente redirige al Dashboard.

---

# 21. Mensajes de autenticación

El Login utiliza el mensaje genérico:

```text
Usuario o contraseña incorrectos.
```

para distintos casos de rechazo.

Esto evita revelar innecesariamente si un Email existe, si el Usuario está inactivo o si la Empresa está inactiva.

---

# 22. Lockout

El inicio de sesión utiliza:

```text
lockoutOnFailure: true
```

Por lo tanto ASP.NET Core Identity puede aplicar la política de bloqueo configurada ante intentos fallidos.

La política concreta de duración/cantidad de intentos depende de la configuración global de Identity y no debe inventarse en este documento sin revisar dicha configuración.

---

# 23. Registro público de Empresa

El alta self-service ya está implementada mediante:

```text
Registro
```

con `AllowAnonymous` y rate limiting.

Permite crear conjuntamente:

- Una Empresa.
- Su configuración/datos iniciales mediante `EmpresaInicializacionService`.
- El primer Usuario administrador.

---

# 24. Reglas del Registro público

El flujo normaliza:

- Nombre.
- Apellido.
- Email.
- Nombre de Empresa.

Antes de crear valida que no exista:

- El Email.
- Otra Empresa con el mismo Nombre según la comparación actual.

Para evitar enumeración innecesaria, ante cualquiera de esos conflictos se devuelve un mensaje genérico de imposibilidad de completar el registro.

---

# 25. Rol del primer Usuario

El Usuario creado mediante Registro público recibe automáticamente:

```text
AdminEmpresa
```

No puede elegir un rol desde el formulario público.

Después de completar correctamente la transacción, el sistema inicia sesión automáticamente y redirige al Dashboard.

---

# 26. Inicialización de Empresa

Durante Registro se ejecuta:

```text
EmpresaInicializacionService.InicializarAsync
```

para preparar los datos base requeridos por una Empresa nueva.

El alta de Empresa, inicialización y Usuario se ejecuta dentro de una transacción para reducir estados parciales.

---

# 27. Recuperación de contraseña

La recuperación de contraseña por correo ya está implementada y no debe figurar como funcionalidad futura.

El flujo utiliza:

```text
RecuperarPassword
```

con `AllowAnonymous` y rate limiting.

---

# 28. Seguridad de RecuperarPassword

El sistema busca el Usuario por Email.

Sólo si existe y está activo genera un token mediante:

```text
GeneratePasswordResetTokenAsync
```

y envía el enlace mediante `IEmailService`.

Sin embargo, la pantalla devuelve igualmente el estado de solicitud enviada aunque el Email no corresponda a un Usuario activo.

Esto evita revelar fácilmente qué Emails están registrados.

---

# 29. Email de recuperación

El enlace se genera hacia:

```text
RestablecerPassword
```

incluyendo:

- Email.
- Token de recuperación.

El contenido HTML codifica el enlace antes de incorporarlo al mensaje y el envío se realiza mediante `IEmailService`.

---

# 30. Restablecimiento de contraseña

`RestablecerPassword` es público.

Requiere Email y Token.

En POST:

1. Busca el Usuario por Email.
2. Exige que exista y esté activo.
3. Ejecuta `UserManager.ResetPasswordAsync`.
4. Si el token es inválido/expiró, muestra un mensaje genérico.
5. Si se restablece correctamente, redirige al Login.

---

# 31. Cambio de contraseña autenticado

Existe además:

```text
CambiarPassword
```

para un Usuario autenticado.

Utiliza:

```text
UserManager.ChangePasswordAsync
```

requiriendo la contraseña actual y la nueva contraseña.

Si la contraseña actual no coincide, se informa específicamente en el formulario correspondiente.

---

# 32. Estado del Usuario y autenticación

Un Usuario con:

```text
Estado == false
```

no puede iniciar sesión mediante el Login actual.

Asimismo, un Usuario no SuperAdmin cuya Empresa esté inactiva tampoco puede autenticarse.

Esto complementa la desactivación lógica sin eliminar información histórica.

---

# 33. Empresa y Usuario

Cada Usuario posee actualmente un `EmpresaId` obligatorio.

La arquitectura utiliza ese valor para delimitar el tenant de usuarios empresariales.

SuperAdmin sigue teniendo un EmpresaId a nivel de modelo porque el campo no es nullable, aunque su comportamiento funcional puede operar globalmente en distintos módulos.

---

# 34. Auditoría

No existe en UsuarioController una regla general que cree automáticamente un registro de Auditoría por cada alta, edición, desactivación o Login.

Sí existe trazabilidad mediante UsuarioId en múltiples entidades operativas.

Por lo tanto no debe documentarse como implementada una auditoría administrativa completa de todas las acciones de Usuario.

---

# 35. Sucursales

Actualmente Usuario no posee `SucursalId` ni asignación productiva por Sucursal.

El dominio Sucursal todavía no está implementado productivamente.

Una futura asignación de empleados por Sucursal deberá diseñarse junto con multi-sucursal y permisos.

---

# 36. Permisos granulares

Actualmente el acceso se basa principalmente en roles de ASP.NET Identity y atributos `Authorize`.

No existe todavía un sistema productivo de permisos granulares por Usuario/acción como:

```text
PuedeAnularVenta
PuedeCerrarCaja
PuedeVerCostos
PuedeEditarPrecios
```

Esta evolución se encuentra separada del modelo Usuario actual.

---

# 37. Funcionalidades no implementadas actualmente

No deben considerarse disponibles todavía:

- 2FA propio/configurado como feature de Veltika.
- Login con Google.
- Login con Microsoft.
- Gestión de dispositivos autorizados.
- Panel de sesiones activas.
- Historial administrativo de accesos.
- Notificaciones de inicio de sesión.
- Firma digital.
- Permisos granulares por empleado.

---

# 38. Reglas de negocio

1. Usuario extiende ASP.NET Core Identity.
2. Nombre y Apellido son obligatorios y tienen máximo 50 caracteres.
3. Email/UserName identifican el acceso.
4. El Email es único globalmente en la implementación actual.
5. Password no se almacena en texto plano en Usuario.
6. Cada Usuario posee EmpresaId.
7. AdminEmpresa sólo administra su Empresa.
8. AdminEmpresa no puede asignar ni administrar SuperAdmin.
9. La Empresa seleccionada debe existir y estar activa.
10. Los roles deben existir en Identity.
11. El flujo actual trata un rol principal por Usuario.
12. ImagenPerfil está implementada.
13. Delete desactiva lógicamente al Usuario.
14. Un Usuario no puede desactivarse a sí mismo.
15. Un Usuario no puede cambiar su propio rol desde Edit.
16. Un Usuario no puede cambiar su propia Empresa desde Edit.
17. Usuario inactivo no puede iniciar sesión.
18. Empresa inactiva bloquea Login de usuarios empresariales no SuperAdmin.
19. Login utiliza rate limiting y lockoutOnFailure.
20. Registro público de Empresa está implementado.
21. El primer Usuario del Registro recibe AdminEmpresa.
22. Recuperación de contraseña por Email está implementada.
23. Cambio de contraseña autenticado está implementado.
24. No existe actualmente auditoría administrativa completa de todas las acciones.
25. No existe actualmente asignación por Sucursal.
26. No existe actualmente sistema de permisos granulares por Usuario.

---

# 39. Casos de error relevantes

- Usuario autenticado inexistente.
- Empresa inexistente o inactiva.
- Email duplicado.
- Rol inexistente.
- Intento de AdminEmpresa de operar sobre otra Empresa.
- Intento de AdminEmpresa de asignar/administrar SuperAdmin.
- Intento de desactivarse a sí mismo.
- Contraseña que no cumple las políticas de Identity.
- Imagen de perfil inválida.
- Credenciales incorrectas.
- Usuario inactivo.
- Empresa inactiva.
- Token de recuperación inválido o expirado.
- Contraseña actual incorrecta al cambiarla.

---

# 40. Estado actual

✅ Gestión administrativa de Usuarios implementada.

✅ Filtros y paginación implementados.

✅ Seguridad multiempresa implementada.

✅ Roles mediante ASP.NET Core Identity implementados.

✅ Desactivación/reactivación lógica implementada.

✅ Imagen de perfil implementada.

✅ Login implementado.

✅ Protección de Usuario y Empresa inactivos implementada.

✅ Rate limiting de autenticación implementado.

✅ Lockout on failure integrado con Identity.

✅ Registro self-service de Empresa implementado.

✅ Inicialización automática de Empresa implementada.

✅ Recuperación de contraseña por Email implementada.

✅ Restablecimiento mediante token de Identity implementado.

✅ Cambio de contraseña autenticado implementado.

🚧 Permisos granulares pendientes.

🚧 Auditoría administrativa/historial de accesos pendiente.

🚧 2FA e identidad externa pendientes.

🚧 Gestión avanzada de sesiones/dispositivos pendiente.