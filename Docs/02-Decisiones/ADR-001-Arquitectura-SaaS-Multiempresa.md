# ADR-001 - Arquitectura SaaS Multiempresa

## Estado
Aceptado

## Contexto
VELTIKA fue diseñado como un sistema SaaS donde múltiples empresas comparten la misma aplicación.

## Decisión
Todas las entidades de negocio pertenecerán a una Empresa mediante `EmpresaId`.
Todas las consultas deberán filtrar por empresa, excepto para SuperAdmin.

## Consecuencias
- Aislamiento de datos.
- Escalabilidad.
- Seguridad por diseño.
