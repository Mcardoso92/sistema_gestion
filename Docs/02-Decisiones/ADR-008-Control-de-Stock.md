# ADR-008 - Control y trazabilidad de Stock

## Estado
Aceptado para el MVP actual

## Contexto

El stock afecta ventas, compras, devoluciones, reintegros, ajustes e importaciones. Permitir cantidades inconsistentes comprometería inventario y reportes.

## Decisión

Veltika no permitirá que una operación deje el stock de un Producto por debajo de cero.

Toda operación que reduzca stock debe validar en el servidor la disponibilidad actual antes de confirmar.

Los cambios de stock originados por operaciones de negocio deben generar `MovimientoStock` cuando corresponda, conservando trazabilidad del origen, cantidad y stock anterior/posterior.

Las operaciones de reversión deben registrar el movimiento inverso en lugar de borrar movimientos históricos.

## Consecuencias

- Una Venta no puede consumir más unidades que las disponibles.
- Una devolución a Proveedor no puede retirar más unidades que las actualmente existentes ni más que las compradas pendientes de devolver.
- La anulación de un ReintegroVenta requiere stock suficiente para retirar nuevamente las unidades previamente restituidas.
- Ajustes y cargas iniciales deben quedar trazables mediante movimientos específicos.

## Evolución futura

La posibilidad de permitir stock negativo podría evaluarse como política configurable sólo si existe una necesidad real validada. Mientras tanto, no forma parte del comportamiento del MVP.
