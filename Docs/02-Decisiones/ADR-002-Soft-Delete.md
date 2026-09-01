# ADR-002 - Soft Delete

Última revisión: 01/09/2026

## Estado

Aceptado

## Contexto

Veltika necesita conservar historial y relaciones de entidades administrativas aunque un registro deje de utilizarse operativamente.

La eliminación física puede romper referencias, eliminar contexto histórico o producir pérdida accidental de información.

Al mismo tiempo, no todas las entidades del sistema representan maestros administrativos: ventas, cobros, pagos, movimientos de stock, movimientos de caja, turnos y otras operaciones históricas requieren reglas propias de anulación o reversión.

## Decisión

Los módulos administrativos utilizarán Soft Delete mediante una propiedad de estado cuando corresponda.

En esos módulos:

```text
Eliminar
    -> desactiva el registro
    -> no elimina físicamente la fila
```

Cuando el módulo lo permita, la reactivación se realizará desde Edit.

El Soft Delete no se aplicará automáticamente a toda entidad del sistema.

Las entidades transaccionales e históricas conservarán su trazabilidad mediante estados, anulaciones, reversiones u otras reglas específicas del dominio.

## Motivos

- Conservación del historial.
- Evitar pérdida accidental de información.
- Preservar relaciones existentes.
- Permitir reactivación de maestros administrativos.
- Evitar utilizar una única estrategia de borrado para entidades con naturaleza diferente.

## Consecuencias

### Positivas

- Los registros históricos continúan siendo interpretables.
- Las relaciones no se pierden por eliminación física de maestros.
- Los registros administrativos pueden volver a activarse cuando corresponda.

### Consideraciones

Las consultas operativas deben decidir explícitamente si incluyen:

```text
activos
inactivos
todos
```

La unicidad funcional puede necesitar considerar también registros inactivos para evitar duplicados que luego entren en conflicto al reactivarse.

## Alcance

La aplicación de Soft Delete debe definirse según la naturaleza de cada módulo y sus reglas de negocio.

No debe asumirse que poseer una propiedad `Estado` implica necesariamente el mismo comportamiento de eliminación lógica que un maestro administrativo.

## Nota de consolidación

Este ADR consolida la decisión que también había sido registrada posteriormente en `ADR-015-SoftDelete.md`.

`ADR-015` queda como registro histórico/supersedido.