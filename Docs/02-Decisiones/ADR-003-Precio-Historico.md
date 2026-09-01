# ADR-003 - Valores históricos en operaciones comerciales

## Estado
Aceptado

## Contexto

Los precios y costos vigentes de un Producto pueden cambiar con el tiempo. Las operaciones históricas no deben modificarse retroactivamente cuando cambian los datos maestros.

## Decisión

Los detalles de operaciones comerciales almacenarán los valores económicos utilizados en el momento de la operación.

En particular, `DetalleVenta` conserva `PrecioUnitario` y `Subtotal`. El mismo principio se aplica a operaciones posteriores que necesitan preservar el valor histórico correspondiente, como reintegros basados en los detalles originales.

Los reportes históricos deben utilizar los valores persistidos de la operación y no recalcularse utilizando el precio actual del Producto.

## Consecuencias

- Una modificación posterior de `Producto.PrecioVenta` no altera ventas históricas.
- Las devoluciones/reintegros pueden utilizar valores coherentes con la operación original.
- Se preserva trazabilidad económica.
- Existe cierta duplicación intencional de datos para conservar historia.

## Regla

Los datos maestros representan el estado actual; los documentos y movimientos históricos conservan el estado económico utilizado cuando fueron registrados.
