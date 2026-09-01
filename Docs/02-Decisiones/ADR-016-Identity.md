# ADR-016 - ASP.NET Core Identity para autenticación y usuarios

## Estado
Aceptado

## Contexto

Veltika necesita autenticación, administración de credenciales, recuperación de contraseña, usuarios y Roles sin implementar un sistema de identidad propio.

## Decisión

Se utilizará ASP.NET Core Identity como base del sistema de autenticación y gestión de usuarios.

`Usuario` extiende `IdentityUser` con información propia de Veltika, incluyendo datos personales operativos, estado y asociación con Empresa.

La autorización actual combina Roles de Identity (`SuperAdmin`, `AdminEmpresa`, `Empleado`) con validaciones server-side de Empresa y de los recursos involucrados.

Los Roles son globales y no se crearán combinaciones de Roles para representar cada posible permiso individual.

Los permisos granulares configurables para empleados son una evolución futura y, si se implementan, deberán complementar Identity en lugar de reemplazarlo innecesariamente.

## Motivos

- Solución oficial y madura del ecosistema ASP.NET Core.
- Gestión segura de contraseñas y autenticación.
- Soporte para Roles y recuperación de contraseña.
- Integración directa con Entity Framework Core.

## Consecuencias

- No se implementará autenticación propia desde cero.
- Las reglas de autorización de negocio siguen siendo responsabilidad de Veltika y no deben depender únicamente del Role.
- La seguridad multiempresa se rige además por ADR-006.
- Cambios futuros de permisos deberán evitar explosión de Roles y mantener un modelo comprensible y auditable.
