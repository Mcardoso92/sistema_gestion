# ADR-002 - Soft Delete

## Estado
Aceptado

## Contexto
La eliminación física compromete el historial y las relaciones.

## Decisión
Los módulos administrativos utilizarán Soft Delete mediante la propiedad `Estado`.
La reactivación se realizará desde Edit.

## Consecuencias
- Conservación del historial.
- Menor riesgo de pérdida de información.
