# ADR-013 - Entity Framework Core Code First y migraciones

## Estado
Aceptado

## Contexto

El esquema de base de datos debe evolucionar junto con el código de Veltika de forma versionada, reproducible y revisable.

## Decisión

Se utilizará Entity Framework Core con enfoque Code First.

Los cambios estructurales de base de datos se representarán mediante migraciones versionadas dentro del repositorio.

Los ambientes no deben modificarse manualmente como mecanismo normal para introducir cambios de esquema.

En despliegues productivos, las migraciones deben aplicarse de manera controlada y respaldada por el proceso de deploy, incluyendo backup previo cuando corresponda.

## Motivos

- Versionado del esquema junto al código.
- Cambios reproducibles entre ambientes.
- Integración con Git y revisiones de código.
- Reducción de diferencias manuales entre desarrollo y producción.

## Consecuencias

- Todo cambio de modelo que afecte persistencia debe evaluar si requiere migración.
- Las migraciones deben revisarse antes de llegar a producción.
- No deben eliminarse o reescribirse migraciones ya aplicadas en producción sin una estrategia explícita.
- El proceso de deploy debe mantener sincronizados aplicación y esquema.
