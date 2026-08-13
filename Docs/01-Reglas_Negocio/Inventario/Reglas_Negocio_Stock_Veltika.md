# Reglas de Negocio --- Stock

**Proyecto:** Veltika\
**Sprint:** 3.0\
**Issue:** #10 --- Stock\
**Estado:** Reglas aprobadas para diseño e implementación

## A. Stock actual y producto

1.  `Producto.Stock` sigue siendo el valor oficial del stock actual.
2.  Ninguna operación normal puede modificar `Producto.Stock` sin
    generar un `MovimientoStock`.
3.  El stock nunca puede quedar negativo.
4.  `PuntoReposicion` se mantiene como concepto de stock mínimo.
5.  Stock `<= PuntoReposicion` se considera stock bajo.
6.  Stock `0` se considera sin stock.
7.  Un producto puede crearse con stock inicial `0` o mayor.
8.  Si el producto se crea con stock inicial mayor a `0`, Veltika genera
    automáticamente un `MovimientoStock` de tipo `StockInicial`.

> Se mantiene la experiencia actual de `Producto.Create`: el usuario
> puede indicar el stock inicial al crear el producto. La trazabilidad
> se incorpora internamente mediante `MovimientoStock`.

## B. Ajustes manuales

9.  Se permiten ajustes manuales de stock únicamente al `AdminEmpresa`.
10. Existen dos ajustes explícitos: **Entrada** y **Salida**.
11. El usuario ingresa una `Cantidad`; nunca escribe directamente el
    nuevo stock.
12. La cantidad del ajuste debe ser mayor que `0`.
13. En una salida manual se valida que exista stock suficiente.
14. Todo ajuste manual exige ingresar un motivo.
15. El motivo tiene una longitud máxima de 250 caracteres.
16. Antes de confirmar el ajuste se muestra el stock actual y el stock
    resultante.
17. Una vez confirmado, un movimiento no puede editarse.
18. Un movimiento no puede eliminarse.

## C. Historial

19. Cada movimiento guarda `StockAnterior`.
20. Cada movimiento guarda `StockPosterior`.
21. Cada movimiento guarda la cantidad involucrada.
22. Cada movimiento guarda fecha y hora.
23. Cada movimiento guarda el usuario que lo realizó.
24. Cada movimiento guarda la empresa.
25. Los movimientos se muestran del más reciente al más antiguo.
26. Se puede consultar el historial por producto.
27. Existe una pantalla general de movimientos de stock.
28. Se puede filtrar el historial por producto.
29. Se puede filtrar por tipo de movimiento.
30. Se puede filtrar por fecha desde/hasta.
31. `SuperAdmin` puede filtrar por empresa.
32. `AdminEmpresa` solamente puede consultar movimientos de su empresa.

## D. Integración con Ventas

33. Confirmar una venta genera un movimiento `Venta` por cada producto.
34. La venta descuenta `Producto.Stock`.
35. No se permite vender una cantidad superior al stock disponible.
36. Stock y venta se actualizan dentro de la misma transacción.
37. Si falla cualquier movimiento de stock, falla toda la venta.
38. Anular una venta genera movimientos `AnulacionVenta`.
39. La anulación devuelve exactamente las cantidades vendidas.
40. El movimiento de anulación referencia a la venta original.
41. Nunca se borra el movimiento original de venta cuando la venta se
    anula.

## E. Integración futura con Compras --- Issue #15

42. Confirmar una compra aumentará automáticamente el stock.
43. Se generará un movimiento `Compra` por cada producto.
44. Cada movimiento quedará relacionado con `CompraId`.
45. Anular una compra generará el movimiento inverso.
46. No se podrá anular una compra si quitar ese stock provoca stock
    negativo.

> Las reglas de precios, costo promedio, costo histórico y actualización
> del precio de venta se definirán en el Issue #15 --- Compras.

## F. Productos inactivos

47. Un producto inactivo conserva su stock histórico.
48. Se puede consultar su historial aunque esté inactivo.
49. Un producto inactivo no puede recibir ajustes manuales.
50. Un producto con stock mayor a `0` puede desactivarse.
51. Reactivar un producto conserva el stock que tenía.

## G. Seguridad

52. `AdminEmpresa` solamente manipula stock de su empresa.
53. Nunca se acepta como confiable un `EmpresaId` enviado por el
    navegador.
54. Se valida `Producto.EmpresaId` en servidor antes de ajustar stock.
55. El acceso directo mediante URL a movimientos o productos de otra
    empresa debe ser rechazado.
56. `SuperAdmin` puede consultar stock de todas las empresas.
57. Para operar sobre stock de una empresa, `SuperAdmin` debe hacerlo en
    el contexto explícito de esa empresa.

## H. Alertas y UX

58. El módulo Stock muestra todos los productos con su existencia.
59. Se distingue visualmente entre `Con stock`, `Stock bajo` y
    `Sin stock`.
60. Existe filtro para productos con stock bajo.
61. Existe filtro para productos sin stock.
62. Existe búsqueda por nombre.
63. Existe búsqueda por código de barras.
64. `SuperAdmin` dispone de filtro por empresa.
65. Desde el listado de Stock se puede acceder directamente a `Ajustar`.
66. Desde el listado se puede acceder al historial del producto.

> Para V1, las alertas son indicadores visuales y filtros. Las
> notificaciones automáticas quedan fuera de alcance.

## I. Integridad técnica

67. El ajuste vuelve a leer el stock desde base de datos antes de
    confirmar.
68. El servidor calcula `StockPosterior`; nunca se confía en un valor
    calculado por el navegador.
69. `EmpresaId`, `UsuarioId`, `Fecha`, `StockAnterior` y
    `StockPosterior` son determinados por el servidor.
70. La modificación del producto y la creación del movimiento ocurren
    dentro de la misma transacción.
71. Si falla el registro del movimiento, el stock no cambia.
72. Los movimientos de stock no utilizan Soft Delete.
73. Se crean índices adecuados para `ProductoId`, `EmpresaId`, `Fecha` y
    referencias de origen como `VentaId`.
74. El stock continúa siendo entero (`int`) para V1.

## Tipos de movimiento previstos

-   `StockInicial`
-   `AjusteEntrada`
-   `AjusteSalida`
-   `Venta`
-   `AnulacionVenta`
-   `Compra`
-   `AnulacionCompra`

## Principio general

`Producto.Stock` mantiene la existencia actual para consultas rápidas.
`MovimientoStock` constituye el historial inmutable y trazable que
explica cómo se llegó a esa existencia.
