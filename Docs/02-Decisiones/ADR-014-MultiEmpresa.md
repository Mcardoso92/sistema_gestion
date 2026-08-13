# ADR-014 - Arquitectura Multiempresa

**Estado:** Aceptado

## Decisión

Una única base de datos compartida con aislamiento por EmpresaId.

## Motivos

-   Menor costo.
-   Administración simplificada.
-   Escalabilidad suficiente para V1.

## Consecuencias

Toda consulta debe respetar el filtro por EmpresaId.
