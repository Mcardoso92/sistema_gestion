# Módulo Detalle de Compra

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Detalle de Compra almacena cada línea de producto confirmada dentro de una compra.

Mientras `Compra` representa la operación comercial completa con un proveedor, `DetalleCompra` conserva:

- Producto adquirido.
- Cantidad.
- Costo unitario histórico.
- Subtotal.
- Costo del producto anterior a la compra.
- Cambios opcionales de precio de venta realizados durante la compra.

Su objetivo es preservar la trazabilidad económica de cada ingreso de mercadería y aportar información para restauraciones seguras cuando una compra se anula.

---

# 2. Alcance actual

Actualmente:

- Los detalles se generan automáticamente durante el alta de una Compra.
- No existe un CRUD independiente de `DetalleCompra`.
- Los detalles se consultan desde la vista de detalle de Compra.
- Cada Compra debe contener al menos una línea válida.
- Un mismo producto no puede aparecer más de una vez en la misma Compra.
- El subtotal se calcula en servidor.
- Cada detalle conserva el costo anterior del Producto.
- Cada detalle puede conservar un cambio de precio de venta.
- Los detalles participan de la lógica de anulación y restauración de costos/precios.
- Los detalles se relacionan con devoluciones de Compra.

---

# 3. Acceso y permisos

`DetalleCompra` no posee controller administrativo independiente.

El acceso se realiza mediante el módulo Compra, protegido actualmente con:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto, no existe actualmente un permiso independiente para un rol `Responsable de Compras`.

Las reglas de seguridad y aislamiento multiempresa se heredan de Compra.

---

# 4. Modelo actual

La entidad `DetalleCompra` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| CompraId | int | Compra propietaria |
| ProductoId | int | Producto adquirido |
| Cantidad | int | Mayor o igual a 1 |
| PrecioUnitario | decimal | No puede ser negativo |
| Subtotal | decimal | Cantidad × PrecioUnitario |
| PrecioCostoAnterior | decimal | Costo del producto antes de la Compra |
| PrecioVentaAnterior | decimal? | Precio de venta anterior cuando la Compra lo modifica |
| PrecioVentaNuevo | decimal? | Nuevo precio de venta aplicado desde la Compra |

Relaciones:

- Compra.
- Producto.
- Detalles de devolución de Compra.

---

# 5. Generación del detalle

Los detalles se construyen dentro de `CompraController.Create` luego de validar empresa, proveedor y productos.

Por cada línea se calcula:

```text
Subtotal = Cantidad × PrecioUnitario
```

Y se conserva:

```text
PrecioCostoAnterior = producto.PrecioCosto
```

Luego el costo actual del producto pasa a ser:

```text
producto.PrecioCosto = PrecioUnitario
```

---

# 6. Cantidad

La cantidad debe cumplir:

```text
Cantidad >= 1
```

El modelo y el flujo de Compra rechazan cantidades menores o iguales a cero.

---

# 7. Precio unitario de costo

`PrecioUnitario` representa el costo informado para ese producto en la Compra.

La regla actual es:

```text
PrecioUnitario >= 0
```

Por lo tanto, técnicamente el costo `0` está permitido actualmente por las validaciones del modelo y del controller.

No puede ser negativo.

El costo queda almacenado históricamente aun cuando `Producto.PrecioCosto` cambie posteriormente.

---

# 8. Subtotal

El subtotal se calcula en servidor:

```text
Subtotal = Cantidad × PrecioUnitario
```

No debe depender de un subtotal calculado o manipulado por el navegador.

El total de la Compra se obtiene mediante la suma de los subtotales de sus detalles.

---

# 9. Productos duplicados

Un mismo Producto no puede aparecer más de una vez dentro de una Compra.

El servidor valida:

```text
Detalles
    .GroupBy(d => d.ProductoId)
    .Any(g => g.Count() > 1)
```

Si encuentra duplicados, rechaza la Compra completa.

Por ejemplo:

```text
Producto 10 - Cantidad 5
Producto 10 - Cantidad 3
```

no se convierte automáticamente en una línea de cantidad 8.

El usuario debe mantener una única línea por Producto.

---

# 10. Costo histórico

El detalle conserva el costo aplicado al momento de la Compra:

```text
DetalleCompra.PrecioUnitario
```

Ejemplo:

```text
Costo anterior: $800
Compra nueva: $950
```

El detalle conserva:

```text
PrecioCostoAnterior = 800
PrecioUnitario = 950
```

Y el Producto queda con:

```text
PrecioCosto = 950
```

Si posteriormente otra Compra cambia el costo, este detalle no se modifica.

---

# 11. Precio de venta opcional

Durante la Compra puede proponerse un nuevo precio de venta.

Si se informa un valor diferente a `Producto.PrecioVenta`, el detalle guarda:

```text
PrecioVentaAnterior
PrecioVentaNuevo
```

Y actualiza:

```text
Producto.PrecioVenta = PrecioVentaNuevo
```

Si no existe cambio real de precio, ambos campos permanecen en `null`.

---

# 12. Uso durante la anulación

Los valores históricos de `DetalleCompra` permiten que Compra pueda revertir determinados cambios sin pisar información posterior.

## Stock

La cantidad almacenada se utiliza para retirar el stock que la Compra había incorporado.

## Costo

`PrecioCostoAnterior` puede utilizarse para restaurar el costo anterior únicamente cuando no existe una Compra activa posterior y el costo actual continúa siendo el costo establecido por la Compra anulada.

## Precio de venta

`PrecioVentaAnterior` puede restaurarse cuando:

- La Compra había aplicado un `PrecioVentaNuevo`.
- No existe un cambio posterior válido proveniente de otra Compra activa.
- El Producto todavía mantiene ese `PrecioVentaNuevo`.

Esto evita que anular una Compra antigua sobrescriba información más reciente.

---

# 13. Relación con stock

`DetalleCompra` describe qué producto y cantidad ingresaron, pero la trazabilidad física se registra en `MovimientoStock`.

Durante la Compra:

```text
StockPosterior = StockAnterior + Cantidad
```

se genera un movimiento de tipo:

```text
Compra
```

Durante la anulación:

```text
StockPosterior = StockAnterior - Cantidad
```

se genera un movimiento de tipo:

```text
AnulacionCompra
```

El detalle histórico no reemplaza al historial de movimientos de stock.

---

# 14. Relación con devoluciones

`DetalleCompra` posee una colección de:

```text
DetalleDevolucionCompra
```

Esto permite vincular devoluciones posteriores con las líneas originales de la Compra.

La lógica específica de cantidades devueltas, stock y estado pertenece al módulo DevolucionCompra.

---

# 15. Inmutabilidad operativa

No existe actualmente una acción administrativa para editar un `DetalleCompra` confirmado.

Una Compra histórica no debe corregirse modificando directamente:

- Cantidad.
- Costo.
- Subtotal.
- Producto.

Las correcciones deben realizarse mediante los flujos previstos, como:

- Anulación de Compra.
- Devolución de Compra.
- Reintegro del proveedor cuando corresponda.

De esta forma se preserva la trazabilidad.

---

# 16. Seguridad multiempresa

`DetalleCompra` no contiene `EmpresaId` propio.

La empresa se determina mediante:

```text
DetalleCompra -> Compra -> EmpresaId
```

Durante la creación, además, cada Producto se valida explícitamente contra la empresa de la Compra.

Las consultas deben realizarse únicamente a través de una Compra previamente autorizada para el tenant correspondiente.

---

# 17. Reglas de negocio

1. Cada detalle pertenece a una única Compra.
2. Cada detalle corresponde a un único Producto.
3. Una Compra debe contener al menos un detalle.
4. La cantidad debe ser mayor a cero.
5. El costo unitario no puede ser negativo.
6. Actualmente el costo cero está permitido técnicamente.
7. El subtotal se calcula en servidor.
8. Un Producto no puede aparecer repetido dentro de la misma Compra.
9. Los Productos duplicados no se consolidan: la Compra se rechaza.
10. El Producto debe existir, estar activo y pertenecer a la misma empresa.
11. El costo unitario queda preservado históricamente.
12. `PrecioCostoAnterior` conserva el costo previo a la Compra.
13. La Compra actualiza `Producto.PrecioCosto` con `PrecioUnitario`.
14. Puede registrarse opcionalmente un cambio de precio de venta.
15. Los cambios de precio de venta conservan valor anterior y nuevo.
16. Los detalles confirmados no tienen edición administrativa independiente.
17. La corrección se realiza mediante anulación/devolución/reintegro según corresponda.
18. La restauración de costos y precios durante la anulación debe respetar cambios posteriores.
19. El detalle participa en devoluciones posteriores mediante `DetalleDevolucionCompra`.
20. La trazabilidad física del inventario corresponde a `MovimientoStock`.

---

# 18. Casos de error relevantes

- Compra sin detalles.
- Producto inexistente.
- Producto inactivo.
- Producto perteneciente a otra empresa.
- Producto duplicado dentro de la Compra.
- Cantidad menor o igual a cero.
- Costo unitario negativo.
- Nuevo precio de venta negativo.
- Error de persistencia durante la Compra.

Los errores durante el alta deben provocar rollback de la transacción completa de Compra.

---

# 19. Integraciones actuales

DetalleCompra se integra con:

- Compra.
- Producto.
- MovimientoStock durante creación/anulación.
- DetalleDevolucionCompra.
- DevolucionCompra.
- Reportes de compras y costos.
- Información histórica de abastecimiento.

---

# 20. Capacidades no implementadas

Actualmente no posee campos propios para:

- Descuento por línea.
- Impuestos desglosados.
- Recargos.
- Costos logísticos distribuidos.
- Costos de importación distribuidos.
- Lote.
- Número de serie.
- Fecha de vencimiento.
- Observaciones específicas de línea.
- Cantidad pedida vs recibida.
- Recepciones parciales.

---

# 21. Evolución futura

La evolución se administra mediante Roadmap y GitHub Issues.

Entre las capacidades posibles se encuentran:

- Historial y análisis de variaciones de costo.
- Costos promedio u otras metodologías si el negocio lo requiere.
- Márgenes y rentabilidad.
- Costos adicionales distribuidos.
- Lotes y vencimientos.
- Números de serie.
- Órdenes de compra y recepciones parciales.
- Mayor trazabilidad de devoluciones.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 22. Estado

✅ Generación automática implementada.

✅ Costo unitario histórico implementado.

✅ Costo anterior implementado.

✅ Subtotal calculado en servidor implementado.

✅ Rechazo server-side de productos duplicados implementado.

✅ Actualización del costo actual del Producto implementada.

✅ Cambio opcional e historial básico de precio de venta implementados.

✅ Integración con stock y anulación implementada.

✅ Integración con devoluciones implementada.

🚧 Costos avanzados, lotes, series y recepciones parciales reservados para evolución futura.