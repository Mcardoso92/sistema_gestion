# ADR-012 - Motor de Base de Datos

**Estado:** Aceptado

## Contexto

Se evaluó SQL Server y MySQL.

## Decisión

Se utilizará SQL Server.

## Motivos

-   Integración nativa con ASP.NET Core y Entity Framework Core.
-   Excelente soporte para Identity.
-   Menor complejidad para la primera versión.

## Consecuencias

-   Entity Framework Core Code First.
-   SQL Server Express en desarrollo.
