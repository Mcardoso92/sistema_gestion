# ADR-015 - Eliminación lógica

Última revisión: 01/09/2026

## Estado

Supersedido por `ADR-002-Soft-Delete.md`.

## Decisión histórica

Este documento registró que las entidades principales utilizarían eliminación lógica cuando correspondiera para conservar historial y evitar pérdida accidental de información.

## Motivo de supersesión

Durante la auditoría documental del 01/09/2026 se confirmó que este ADR no representa una decisión diferente de `ADR-002`.

Además, la formulación "entidades principales" resulta demasiado amplia para el comportamiento real de Veltika, donde las entidades administrativas pueden utilizar Soft Delete mientras que las operaciones transaccionales e históricas poseen reglas específicas de anulación y reversión.

La decisión vigente y su alcance actualizado se encuentran en `ADR-002-Soft-Delete.md`.

Este archivo se conserva únicamente como registro histórico.