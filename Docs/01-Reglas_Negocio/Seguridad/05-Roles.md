# Módulo Roles

Última actualización: 01/09/2026

---

# 1. Objetivo

Los Roles determinan actualmente grandes niveles de acceso dentro de Veltika mediante ASP.NET Core Identity.

La implementación vigente utiliza roles globales del sistema y atributos de autorización en controllers/actions.

No existe todavía un módulo administrativo completo para crear roles empresariales personalizados ni una matriz persistida de permisos granulares.

---

# 2. Implementación actual

Los roles se implementan mediante:

```text
ASP.NET Core Identity
IdentityRole
RoleManager<IdentityRole>
UserManager<Usuario>
Authorize(Roles = "...")
```

No existe actualmente una entidad de dominio propia `Rol` con campos adicionales de Veltika.

---

# 3. Roles iniciales

`IdentitySeeder` garantiza actualmente la existencia de tres roles:

```text
SuperAdmin
AdminEmpresa
Empleado
```

Si alguno no existe, se crea mediante:

```text
RoleManager.CreateAsync(new IdentityRole(nombre))
```

---

# 4. Roles globales

Los `IdentityRole` actuales son globales a la instalación de Veltika.

No poseen actualmente:

```text
EmpresaId
Descripcion
Estado
EsRolSistema
Prioridad
Color
Icono
FechaAlta
```

Por lo tanto no existe hoy un conjunto independiente de roles por Empresa.

---

# 5. SuperAdmin

`SuperAdmin` representa el nivel administrativo global de Veltika.

Según el controller correspondiente, puede operar transversalmente sobre distintas Empresas y acceder a funcionalidades reservadas al administrador general.

Ejemplos frecuentes de autorización:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Algunos flujos distinguen explícitamente SuperAdmin para permitir alcance global o selección de Empresa.

---

# 6. AdminEmpresa

`AdminEmpresa` representa actualmente al administrador de una Empresa.

Su acceso suele combinar dos controles:

1. Autorización por rol.
2. Restricción por `Usuario.EmpresaId` en servidor.

Por lo tanto el rol por sí solo no define el tenant: el aislamiento multiempresa también depende de EmpresaId y de las validaciones de cada flujo.

---

# 7. Empleado

El rol `Empleado` existe actualmente en el seeding de Identity.

Sin embargo, buena parte de los controllers administrativos revisados autorizan sólo:

```text
SuperAdmin
AdminEmpresa
```

Por lo tanto la mera existencia de `Empleado` no significa que actualmente posea acceso completo a módulos operativos.

El alcance futuro de Empleado debe definirse junto con permisos granulares.

---

# 8. Asignación de rol a Usuario

La creación y edición administrativa de Usuario utiliza `RoleManager` y `UserManager`.

Antes de asignar un rol se verifica:

```text
RoleManager.RoleExistsAsync(nombreRol)
```

La asignación se realiza mediante:

```text
UserManager.AddToRoleAsync(usuario, rol)
```

---

# 9. Restricción de SuperAdmin

Un `AdminEmpresa` no puede asignar el rol:

```text
SuperAdmin
```

Además, cuando un AdminEmpresa consulta los roles disponibles desde UsuarioController, SuperAdmin se excluye del listado.

La validación vuelve a realizarse en servidor al crear o editar usuarios.

---

# 10. Rol principal por Usuario

Aunque ASP.NET Core Identity técnicamente soporta múltiples roles por usuario, los flujos administrativos actuales de Veltika trabajan conceptualmente con un único rol principal.

Al editar un Usuario:

1. Se obtienen sus roles actuales.
2. Se toma el primero como rol actual.
3. Si cambia, se remueve ese rol.
4. Se agrega el nuevo rol.

Por lo tanto el modelo funcional actual de Veltika debe considerarse de un rol principal por Usuario.

---

# 11. Cambio del propio rol

Un Usuario no puede cambiar su propio rol mediante el flujo administrativo de edición de Usuario.

Cuando el Usuario editado coincide con el Usuario autenticado, el controller conserva el rol actual y elimina ese campo de las validaciones recibidas desde la vista.

Esta regla evita pérdida accidental de privilegios o modificaciones sensibles sobre la propia identidad.

---

# 12. CRUD de Roles

La documentación anterior indicaba que actualmente se podía:

- Crear roles.
- Editar roles.
- Activar/desactivar roles.
- Consultar roles desde un módulo propio.
- Crear roles personalizados por Empresa.

Estas funcionalidades no corresponden a la implementación actual.

No existe actualmente un `RolController` administrativo productivo ni un CRUD empresarial de roles.

Los roles base son creados por el seeder y consumidos por el sistema.

---

# 13. Estado de Rol

`IdentityRole` no posee en la implementación actual un campo propio:

```text
Estado
```

Por lo tanto no existe el concepto actual de activar/desactivar un rol manteniéndolo almacenado.

Tampoco existe Soft Delete para roles.

---

# 14. Empresa propietaria del Rol

Los roles actuales no poseen:

```text
EmpresaId
```

Consecuentemente no existen hoy reglas como:

```text
Rol pertenece a Empresa A
Rol pertenece a Empresa B
```

La separación multiempresa se aplica sobre el Usuario y los datos de negocio, no sobre `IdentityRole`.

---

# 15. Permisos asociados

Actualmente un rol no contiene una colección propia de permisos de negocio persistidos por Veltika.

La autorización está codificada principalmente en atributos y lógica de servidor, por ejemplo:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

junto con comprobaciones adicionales como:

```text
UserManager.IsInRoleAsync(...)
```

---

# 16. Rol y permiso no son equivalentes

En el diseño actual:

```text
Rol = categoría amplia de acceso
```

mientras que un futuro sistema de permisos granulares deberá representar acciones específicas.

Ejemplos futuros:

```text
Ventas.Ver
Ventas.Crear
Ventas.Anular
Stock.Ajustar
Productos.VerCosto
Caja.CerrarTurno
Reportes.VerRentabilidad
```

No deben modelarse cientos de combinaciones creando un rol distinto para cada caso si una matriz de permisos resulta más apropiada.

---

# 17. Autorización actual

La seguridad vigente no debe depender sólo de ocultar botones en Razor.

Las restricciones críticas se realizan en servidor mediante:

- `Authorize`.
- comprobación de rol.
- validación de EmpresaId.
- validación del recurso solicitado.

La interfaz puede acompañar estas reglas, pero no reemplazarlas.

---

# 18. Multiempresa

`AdminEmpresa` no obtiene acceso a otras Empresas simplemente por compartir el mismo nombre de rol con otros administradores.

El aislamiento se consigue porque cada Usuario posee `EmpresaId` y los controllers aplican filtros y validaciones sobre dicho contexto.

Por lo tanto:

```text
Rol != Tenant
```

---

# 19. Registro público

El registro self-service de una Empresa asigna automáticamente al primer Usuario el rol:

```text
AdminEmpresa
```

El Usuario no elige el rol durante ese registro.

---

# 20. Seeder

El `IdentitySeeder` inicializa los roles necesarios si no existen.

Actualmente declara:

```text
SuperAdmin
AdminEmpresa
Empleado
```

El seeder también contiene usuarios/Empresas de datos iniciales o demostración según el entorno/configuración de ejecución.

La existencia de esos datos de seed no debe interpretarse como un CRUD de roles disponible para usuarios finales.

---

# 21. Roles personalizados

No existen actualmente roles personalizados por Empresa.

Una futura solución podría permitir que cada Empresa defina perfiles como:

```text
Cajero
Encargado
Vendedor
Responsable de compras
Supervisor
```

pero esta evolución debe diseñarse junto con la matriz de permisos para evitar roles rígidos o una proliferación innecesaria.

---

# 22. Roles por Sucursal

No existe actualmente asignación de roles por Sucursal.

El dominio Sucursal todavía no está implementado productivamente.

Si posteriormente se requiere restringir un Usuario a determinadas Sucursales, debería analizarse si corresponde modelar:

```text
Usuario ↔ Sucursal
```

y/o alcance de permisos sobre recursos concretos, en lugar de duplicar roles por Sucursal.

---

# 23. Roles temporales

No existen actualmente:

- Roles temporales.
- Fecha de vigencia del rol.
- Expiración automática de privilegios.
- Elevación temporal de permisos.

Estas capacidades sólo deberían agregarse si aparecen necesidades operativas concretas.

---

# 24. Auditoría de roles

Actualmente no existe un historial administrativo específico que registre automáticamente:

- Quién cambió el rol de un Usuario.
- Rol anterior.
- Rol nuevo.
- Fecha del cambio.

Este tipo de auditoría sería recomendable si se incorporan permisos más sensibles o administración delegada de empleados.

---

# 25. Reglas de negocio actuales

1. Los roles se implementan con ASP.NET Core Identity.
2. Los roles base actuales son SuperAdmin, AdminEmpresa y Empleado.
3. Los roles son globales, no pertenecen a una Empresa.
4. No existe una entidad de dominio Rol propia con EmpresaId/Estado/Descripción.
5. No existe actualmente CRUD administrativo de Roles.
6. No existe Soft Delete de Roles.
7. Los Usuarios administrativos trabajan funcionalmente con un rol principal.
8. Un AdminEmpresa no puede asignar SuperAdmin.
9. Un AdminEmpresa no puede administrar usuarios SuperAdmin.
10. Un Usuario no puede cambiar su propio rol desde Edit.
11. El rol debe existir antes de ser asignado.
12. La autorización se complementa con reglas multiempresa en servidor.
13. Empleado existe como rol, pero su alcance funcional todavía es limitado/no granular.
14. No existe actualmente matriz de permisos asociada a roles.
15. No existen roles por Sucursal ni roles temporales.

---

# 26. Evolución recomendada

La evolución prevista no debería centrarse inicialmente en construir un CRUD genérico de roles aislado.

Primero conviene definir el modelo de permisos granulares requerido por las Empresas reales.

Una arquitectura posible podría separar:

```text
Permiso
Rol/Perfil
RolPermiso
UsuarioRol o UsuarioPerfil
Alcance empresarial
```

manteniendo las restricciones críticas del servidor.

El diseño definitivo debe resolverse cuando se implemente la funcionalidad correspondiente y después de validar casos reales.

---

# 27. Funcionalidades futuras posibles

- Permisos granulares por módulo y acción.
- Perfiles personalizados por Empresa.
- Roles/perfiles predefinidos.
- Duplicación de perfiles.
- Auditoría de cambios de permisos.
- Asignación de alcance por Sucursal cuando exista el dominio.
- Permisos sensibles separados, por ejemplo costos, anulaciones y cierres.
- Eventual vigencia temporal de accesos si existe necesidad.

No se consideran implementadas actualmente.

---

# 28. Estado actual

✅ ASP.NET Core Identity para roles implementado.

✅ SuperAdmin implementado.

✅ AdminEmpresa implementado.

✅ Empleado creado como rol base.

✅ Asignación de rol al crear Usuario implementada.

✅ Cambio de rol al editar Usuario implementado.

✅ Protección para impedir que AdminEmpresa asigne SuperAdmin implementada.

✅ Restricciones multiempresa complementarias implementadas en controllers.

🚧 Permisos granulares pendientes.

🚧 Perfiles personalizados por Empresa pendientes.

🚧 Auditoría de cambios de rol pendiente.

🚧 Roles/perfiles por alcance de Sucursal pendientes de la futura arquitectura multi-sucursal.

❌ CRUD empresarial de Roles no existe actualmente.

❌ Activación/desactivación de Roles no existe actualmente.