# ADR-006 - Seguridad y autorización multiempresa

## Estado
Aceptado

## Contexto

Compartir aplicación y base de datos entre Empresas exige impedir accesos cruzados incluso si un usuario manipula IDs, formularios, URLs o solicitudes HTTP.

El aislamiento definido por ADR-001 no puede depender de datos confiados al navegador.

## Decisión

Para usuarios de Empresa, el contexto de Empresa autorizado se obtiene del usuario autenticado y las operaciones deben validar server-side que los recursos consultados o modificados pertenecen a esa Empresa.

Los IDs enviados por el cliente se consideran referencias no confiables y nunca sustituyen la validación de pertenencia.

`SuperAdmin` puede realizar operaciones transversales únicamente en los flujos que explícitamente lo permiten. En esos casos la Empresa objetivo debe resolverse y validarse en el servidor.

La autorización combina:

- autenticación con ASP.NET Core Identity;
- Roles;
- validación de `EmpresaId`;
- validación de pertenencia del recurso;
- reglas adicionales propias de cada operación.

## Consecuencias

- Se reduce el riesgo de acceso horizontal entre Empresas.
- Los Controllers y Services deben aplicar aislamiento explícitamente en cada flujo relevante.
- Ocultar elementos en la interfaz no constituye una medida de autorización.
- Toda funcionalidad nueva debe revisarse desde el punto de vista multiempresa.

## Relación

Complementa ADR-001, que define la arquitectura de datos compartidos por Empresa.
