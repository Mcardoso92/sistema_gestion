# ADR-010 - Transacciones en operaciones críticas

## Estado
Aceptado

## Contexto

Muchas operaciones de Veltika modifican varias entidades relacionadas. Una confirmación parcial podría dejar inconsistencias entre comprobantes, Stock, Caja, saldos o movimientos históricos.

Además, algunas validaciones dependen de información que puede cambiar entre la lectura inicial y la persistencia.

## Decisión

Las operaciones críticas que requieren consistencia entre múltiples escrituras se ejecutarán dentro de una transacción de base de datos.

Cuando exista riesgo de concurrencia sobre saldos, stock, cantidades disponibles o unicidad operativa, se utilizará el nivel de aislamiento apropiado; actualmente varios flujos críticos utilizan `IsolationLevel.Serializable` y revalidan las condiciones dentro de la transacción.

Esta política ya no se limita a Venta. Se aplica según corresponda a operaciones como:

- Ventas y Compras;
- Cobros y Pagos;
- Devoluciones y Reintegros;
- Transferencias de Caja;
- apertura/cierre y operaciones sensibles de TurnoCaja;
- otras operaciones que afecten varias piezas de estado de forma atómica.

## Consecuencias

- Si una parte de la operación falla, se realiza rollback del conjunto.
- Las validaciones sensibles deben repetirse dentro de la transacción cuando puedan haber quedado obsoletas.
- Se prioriza integridad sobre una optimización prematura de concurrencia.
- El aislamiento deberá revisarse si la escala futura demuestra contención significativa.
