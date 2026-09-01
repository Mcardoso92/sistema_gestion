# ADR-004 - Inmutabilidad de operaciones históricas

## Estado
Aceptado

## Contexto

Editar directamente una operación comercial o financiera ya registrada puede alterar stock, saldos, caja, reportes y trazabilidad.

## Decisión

Las operaciones históricas relevantes no se corregirán modificando o eliminando físicamente el registro original.

La corrección se realizará mediante mecanismos explícitos según el módulo: anulación, devolución, reintegro o movimiento de reversión.

La Venta es uno de los casos principales de esta política y no debe tratarse como un CRUD administrativo convencional.

## Consecuencias

- El registro original permanece disponible para auditoría.
- Los efectos inversos quedan registrados explícitamente.
- Stock y Caja mantienen una trazabilidad comprensible.
- La lógica de corrección es más rigurosa que un simple `Update` o `Delete`.

## Alcance

Esta decisión no implica que todas las entidades sean inmutables. Los maestros administrativos pueden utilizar edición y Soft Delete cuando corresponda, según ADR-002.
