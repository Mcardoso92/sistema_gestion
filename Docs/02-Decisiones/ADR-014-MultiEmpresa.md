# ADR-014 - Arquitectura Multiempresa

Última revisión: 01/09/2026

## Estado

Supersedido por `ADR-001-Arquitectura-SaaS-Multiempresa.md`.

## Decisión histórica

Este documento registró la decisión de utilizar:

```text
una única base de datos compartida
+
aislamiento por EmpresaId
```

## Motivos registrados

- Menor costo.
- Administración simplificada.
- Escalabilidad suficiente para la primera versión.

## Motivo de supersesión

Durante la auditoría documental del 01/09/2026 se confirmó que este ADR describe la misma decisión arquitectónica que `ADR-001` y no una evolución independiente.

Para evitar dos fuentes vigentes sobre la estrategia multiempresa, la decisión fue consolidada en `ADR-001-Arquitectura-SaaS-Multiempresa.md`.

Este archivo se conserva únicamente como registro histórico de la documentación anterior.