# Módulo Stock

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Stock permite consultar el inventario actual de los productos de una empresa dentro de Veltika.

Actualmente el stock operativo no se modela mediante una entidad `Stock` independiente.

La existencia actual se almacena directamente en:

```text
Producto.Stock
```

Mientras que la trazabilidad histórica de sus cambios se conserva en:

```text
MovimientoStock
```

---

# 2. Arquitectura actual

El modelo actual utiliza dos conceptos complementarios:

```text
Producto.Stock -> estado actual
MovimientoStock -> historial de cambios
```

Esto permite:

- Obtener rápidamente la existencia vigente desde Producto.
- Reconstruir cómo cambió mediante MovimientoStock.

No existe actualmente una tabla o entidad `Stock` separada.

Tampoco existe stock por Sucursal o Depósito.

---

# 3. Alcance actual

Actualmente el módulo permite:

- Consultar stock actual.
- Buscar productos por nombre.
- Buscar productos por código de barras.
- Visualizar categoría.
- Visualizar empresa.
- Visualizar punto de reposición.
- Visualizar estado del producto.
- Clasificar productos según situación de stock.
- Filtrar por empresa para `SuperAdmin`.
- Consultar historial de movimientos.
- Acceder a ajustes manuales para `AdminEmpresa`.

El stock se modifica mediante los flujos funcionales correspondientes y no mediante edición directa del campo desde un CRUD genérico.

---

# 4. Acceso y permisos

La consulta de stock se encuentra actualmente en `MovimientoStockController`, protegido por:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede:

- Consultar productos de todas las empresas.
- Filtrar por empresa.
- Consultar historiales.

## AdminEmpresa

Puede:

- Consultar stock de su empresa.
- Consultar historial.
- Registrar ajustes manuales de entrada y salida.

Actualmente no existen permisos directos en este controller para:

- Responsable de Depósito.
- Cajero.
- Vendedor.

La disponibilidad de productos puede ser utilizada internamente por otros módulos, pero eso no equivale a acceso al módulo administrativo Stock.

---

# 5. Campo de stock actual

`Producto` contiene actualmente:

```text
public int Stock { get; set; }
```

Con validación:

```text
Stock >= 0
```

Por lo tanto, el modelo actual no admite valores negativos.

---

# 6. Punto de reposición

`Producto` también posee:

```text
public int PuntoReposicion { get; set; }
```

Con regla:

```text
PuntoReposicion >= 0
```

Este campo ya está implementado y se utiliza para determinar cuándo un producto tiene stock bajo.

No debe documentarse como una funcionalidad futura de stock mínimo genérico sin distinguirlo del concepto actual de `PuntoReposicion`.

---

# 7. Clasificación actual del stock

El listado clasifica los productos de la siguiente forma:

## Sin stock

```text
Stock == 0
```

## Stock bajo

```text
Stock > 0
AND
Stock <= PuntoReposicion
```

## Con stock

```text
Stock > PuntoReposicion
```

Estas categorías se utilizan como filtro en la pantalla de stock.

---

# 8. Búsqueda y filtros

El listado actual permite buscar por:

- Nombre de Producto.
- Código de barras.

Permite filtrar por:

- Todos.
- Con stock.
- Stock bajo.
- Sin stock.
- Empresa para `SuperAdmin`.

Los productos se ordenan por nombre.

La paginación actual utiliza:

```text
20 productos por página
```

---

# 9. Información mostrada

La consulta de stock expone actualmente:

- ProductoId.
- Nombre.
- Código de barras.
- Categoría.
- Empresa.
- Stock actual.
- Punto de reposición.
- Estado de stock calculado.
- Estado activo/inactivo del Producto.

---

# 10. Producto inactivo

Desactivar un Producto no elimina su stock ni su historial.

El Producto conserva:

```text
Producto.Stock
```

Y sus movimientos históricos continúan disponibles.

Sin embargo, determinados flujos operativos restringen el uso de productos inactivos.

Por ejemplo, no puede realizarse un ajuste manual de stock sobre un Producto inactivo desde el flujo actual.

---

# 11. Modificación de stock

El valor de `Producto.Stock` puede cambiar mediante operaciones controladas del sistema.

Entre los orígenes actualmente implementados se encuentran:

- Stock inicial.
- Ajuste de entrada.
- Ajuste de salida.
- Venta.
- Anulación de Venta.
- Compra.
- Anulación de Compra.
- Reintegro de Venta.
- Anulación de Reintegro de Venta.
- Devolución de Compra.
- Anulación de Devolución de Compra.

Cada uno debe generar su correspondiente `MovimientoStock`.

---

# 12. Stock inicial

El stock de un Producto no está obligado a comenzar siempre en cero.

Los flujos actuales de creación/importación pueden admitir stock inicial.

Cuando existe stock inicial debe quedar trazabilidad mediante:

```text
TipoMovimientoStock.StockInicial
```

Por lo tanto, la afirmación antigua de que el primer movimiento necesariamente será una Compra o un ajuste ya no representa correctamente la implementación.

---

# 13. Compras

Al registrar una Compra:

```text
Producto.Stock = StockAnterior + CantidadComprada
```

Y se genera:

```text
MovimientoStock.Tipo = Compra
```

La anulación de Compra retira esas cantidades si existe stock suficiente y genera:

```text
AnulacionCompra
```

---

# 14. Ventas

Al registrar una Venta:

```text
Producto.Stock = StockAnterior - CantidadVendida
```

La Venta valida que exista disponibilidad suficiente.

El movimiento generado es:

```text
Venta
```

La anulación restaura las unidades mediante:

```text
AnulacionVenta
```

---

# 15. Ajustes manuales

Los ajustes de stock están implementados.

Actualmente sólo `AdminEmpresa` puede utilizar este flujo.

## Entrada

```text
StockPosterior = StockAnterior + Cantidad
```

Tipo:

```text
AjusteEntrada
```

## Salida

```text
StockPosterior = StockAnterior - Cantidad
```

La cantidad a retirar no puede superar el stock existente.

Tipo:

```text
AjusteSalida
```

El ajuste exige un motivo mediante su ViewModel.

---

# 16. Reintegros y devoluciones

Los flujos actuales también contemplan movimientos de inventario para:

```text
ReintegroVenta
AnulacionReintegroVenta
DevolucionCompra
AnulacionDevolucionCompra
```

Estos procesos permiten corregir parcialmente operaciones comerciales sin modificar directamente el historial previo.

---

# 17. Stock no negativo

El comportamiento actual evita stock negativo.

El modelo exige:

```text
Stock >= 0
```

Además:

- Venta valida disponibilidad.
- AjusteSalida valida disponibilidad.
- Anulación de Compra valida que existan unidades suficientes para retirar.
- Otros flujos de salida deben mantener la misma regla.

Actualmente no existe una configuración empresarial para habilitar stock negativo.

---

# 18. Historial

Toda variación operativa de stock debe tener trazabilidad mediante `MovimientoStock`.

Cada movimiento registra al menos:

- Producto.
- Empresa.
- Tipo.
- Cantidad.
- Stock anterior.
- Stock posterior.
- Fecha.
- Usuario.

Además puede contener:

- Motivo.
- VentaId.
- CompraId.
- ReintegroVentaId.
- DevolucionCompraId.

---

# 19. Stock como estado actual

El stock mostrado en las pantallas no se recalcula sumando todos los movimientos en cada consulta.

Se obtiene directamente desde:

```text
Producto.Stock
```

`MovimientoStock` funciona como trazabilidad histórica, no como única fuente operativa para calcular cada lectura del stock actual.

Ejemplo conceptual:

```text
StockInicial +20
Compra +10
Venta -4

Producto.Stock = 26
```

Los movimientos permiten explicar el valor 26, pero la aplicación consulta el campo actual del Producto.

---

# 20. Seguridad multiempresa

El stock queda aislado por la relación:

```text
Producto -> EmpresaId
```

Y los movimientos poseen además:

```text
MovimientoStock.EmpresaId
```

Para `AdminEmpresa`, el listado y el historial se restringen a:

```text
usuario.EmpresaId
```

Nunca debe permitirse modificar o consultar administrativamente stock de otro tenant mediante manipulación de IDs.

---

# 21. Sucursales y depósitos

Actualmente Veltika no posee stock por Sucursal.

No existen en la implementación actual campos como:

```text
SucursalId
DepositoId
StockPorSucursal
StockPorDeposito
```

El stock actual es único por Producto dentro de su Empresa.

La futura incorporación de sucursales o depósitos requerirá rediseñar esta estructura, probablemente desacoplando el stock de `Producto` hacia existencias por ubicación.

Ese cambio no debe asumirse como implementado hoy.

---

# 22. Valorización de inventario

La valorización del inventario ya existe a nivel de Reportes de Stock.

El sistema puede utilizar los datos actuales del Producto para obtener valores como:

```text
Stock × PrecioCosto
Stock × PrecioVenta
```

Por lo tanto, `ValorInventario` no necesita existir como un campo persistido dentro de una entidad Stock para poder generar reportes de valuación.

---

# 23. Reglas de negocio

1. Actualmente no existe una entidad `Stock` independiente.
2. El stock actual se almacena en `Producto.Stock`.
3. Cada Producto posee un único stock dentro de su Empresa.
4. Actualmente no existe stock por Sucursal ni Depósito.
5. El stock no puede ser negativo.
6. `PuntoReposicion` ya está implementado.
7. El estado de stock se calcula comparando Stock y PuntoReposicion.
8. Las variaciones operativas deben generar MovimientoStock.
9. El historial no reemplaza al campo actual `Producto.Stock`.
10. Los Productos inactivos conservan stock e historial.
11. No se permiten ajustes manuales sobre Productos inactivos.
12. Los ajustes manuales están implementados para `AdminEmpresa`.
13. El stock inicial puede registrarse mediante movimiento `StockInicial`.
14. Las Ventas reducen stock.
15. Las anulaciones de Venta restauran stock.
16. Las Compras aumentan stock.
17. Las anulaciones de Compra reducen stock si existe disponibilidad.
18. Los reintegros y devoluciones poseen movimientos específicos.
19. La seguridad multiempresa se aplica mediante EmpresaId.
20. La valorización se calcula desde los datos actuales y no requiere un campo ValorInventario persistido.

---

# 24. Casos de error relevantes

- Producto inexistente.
- Producto perteneciente a otra empresa.
- Usuario no autenticado.
- Usuario sin permisos administrativos.
- Intento de ajuste sobre Producto inactivo.
- Salida superior al stock disponible.
- Cantidad de ajuste inválida.
- Tipo de ajuste inválido.
- Operación comercial con stock insuficiente.
- Error de persistencia al modificar inventario.

---

# 25. Integraciones actuales

El concepto de Stock se integra con:

- Producto.
- MovimientoStock.
- Venta.
- Compra.
- Ajustes de Stock.
- ReintegroVenta.
- DevolucionCompra.
- Reportes de Stock.
- Dashboard y alertas básicas de stock bajo.

Actualmente no se integra con Sucursal.

---

# 26. Capacidades no implementadas

Actualmente no existen:

- Stock por Sucursal.
- Stock por Depósito.
- Stock máximo.
- Stock reservado.
- Stock comprometido.
- Reservas de inventario.
- Transferencias entre ubicaciones.
- Inventario físico formal.
- Conteos cíclicos.
- Lotes.
- Vencimientos.
- Series.
- Políticas configurables de stock negativo.
- Costo promedio persistido.
- FechaUltimoMovimiento persistida como campo de Stock.

---

# 27. Evolución futura

La evolución se administra mediante Roadmap y GitHub Issues.

Entre las mejoras previstas o posibles se encuentran:

- Sucursales y depósitos.
- Existencias independientes por ubicación.
- Transferencias.
- Stock reservado.
- Stock disponible calculado.
- Inventario físico.
- Conteos cíclicos.
- Lotes y vencimientos.
- Números de serie.
- Reposición sugerida.
- Días de stock.
- Rotación y productos sin movimiento.
- Alertas más avanzadas.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 28. Estado

✅ Stock actual en Producto implementado.

✅ Punto de reposición implementado.

✅ Clasificación sin stock/bajo/con stock implementada.

✅ Búsqueda y paginación implementadas.

✅ Historial de movimientos implementado.

✅ Ajustes manuales implementados.

✅ Stock inicial trazable implementado.

✅ Integración con Venta y Compra implementada.

✅ Integración con anulaciones, reintegros y devoluciones implementada.

✅ Valorización de inventario disponible mediante Reportes.

🚧 Stock por sucursal/depósito, reservas, inventario físico y trazabilidad avanzada quedan para evolución futura.