# ADR-010 - Transacciones en Ventas

## Estado
Aceptado

## Decisión
Registrar una venta deberá ejecutarse dentro de una única transacción:
- Crear Venta.
- Crear Detalles.
- Actualizar Stock.

## Consecuencias
Consistencia de datos ante errores.
