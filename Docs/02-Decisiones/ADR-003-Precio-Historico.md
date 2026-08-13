# ADR-003 - Precio Histórico

## Estado
Aceptado

## Contexto
El precio de un producto puede cambiar con el tiempo.

## Decisión
`DetalleVenta` almacenará `PrecioUnitario` y `Subtotal`.

## Consecuencias
Las ventas históricas nunca cambian aunque cambie el precio del producto.
