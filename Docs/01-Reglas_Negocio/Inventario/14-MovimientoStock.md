# Módulo Movimiento de Stock

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Movimiento de Stock registra la trazabilidad de las variaciones de inventario de los productos dentro de Veltika.

Cada movimiento conserva:

- Producto afectado.
- Empresa.
- Tipo de movimiento.
- Cantidad involucrada.
- Stock anterior.
- Stock posterior.
- Fecha.
- Usuario responsable.
- Motivo cuando corresponde.
- Referencias comerciales cuando existen.

Su función es permitir reconstruir cómo evolucionó el stock de un producto y qué operación originó cada cambio.

---

# 2. Alcance actual

Actualmente el módulo permite:

- Consultar el stock actual de productos.
- Buscar productos por nombre o código de barras.
- Filtrar productos por estado de stock.
- Consultar historial de movimientos.
- Filtrar movimientos por producto.
- Filtrar movimientos por tipo.
- Filtrar movimientos por fechas.
- Filtrar por empresa para `SuperAdmin`.
- Consultar el detalle de un movimiento.
- Registrar ajustes manuales de entrada y salida para `AdminEmpresa`.
- Registrar movimientos automáticos provenientes de operaciones comerciales.

Los movimientos no poseen CRUD de edición ni eliminación.

---

# 3. Acceso y permisos

El controller utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede:

- Consultar stock de todas las empresas.
- Filtrar por empresa.
- Consultar historiales y detalles.

Actualmente la acción `Ajustar` está restringida específicamente a:

```text
[Authorize(Roles = "AdminEmpresa")]
```

Por lo tanto, `SuperAdmin` no ingresa por esa acción mientras conserve esa autorización específica.

## AdminEmpresa

Puede:

- Consultar stock de su empresa.
- Consultar historial de su empresa.
- Consultar detalles de movimientos de su empresa.
- Registrar ajustes manuales de entrada y salida sobre productos activos de su empresa.

Actualmente no existen permisos propios para roles `Responsable de Depósito` o `Auditor`.

---

# 4. Modelo actual

La entidad `MovimientoStock` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| ProductoId | int | Producto afectado |
| EmpresaId | int | Empresa propietaria |
| Tipo | TipoMovimientoStock | Tipo/origen del movimiento |
| Cantidad | int | Mayor o igual a 1 |
| StockAnterior | int | No negativo |
| StockPosterior | int | No negativo |
| Motivo | string? | Opcional, máximo 250 caracteres |
| Fecha | DateTime | Fecha y hora del movimiento |
| UsuarioId | string | Usuario responsable |
| VentaId | int? | Venta asociada cuando corresponde |
| CompraId | int? | Compra asociada cuando corresponde |
| ReintegroVentaId | int? | Reintegro de Venta asociado cuando corresponde |
| DevolucionCompraId | int? | Devolución de Compra asociada cuando corresponde |

Relaciones:

- Producto.
- Empresa.
- Usuario.
- Venta opcional.
- Compra opcional.
- ReintegroVenta opcional.
- DevolucionCompra opcional.

Actualmente no existe `SucursalId`.

---

# 5. Tipos de movimiento actuales

El enum `TipoMovimientoStock` contiene actualmente:

```text
StockInicial = 1
AjusteEntrada = 2
AjusteSalida = 3
Venta = 4
AnulacionVenta = 5
Compra = 6
AnulacionCompra = 7
ReintegroVenta = 8
AnulacionReintegroVenta = 9
DevolucionCompra = 10
AnulacionDevolucionCompra = 11
```

Estos valores representan los tipos de movimiento reconocidos actualmente por el sistema.

---

# 6. Stock inicial

Al crear productos mediante los flujos que admiten stock inicial, el sistema puede registrar un movimiento de tipo:

```text
StockInicial
```

El objetivo es evitar que una existencia inicial aparezca en inventario sin trazabilidad.

La cantidad, stock anterior y stock posterior deben reflejar el estado real del producto en ese momento.

---

# 7. Movimientos por Venta

Al confirmar una Venta:

```text
StockPosterior = StockAnterior - CantidadVendida
```

Se genera un movimiento:

```text
Tipo = Venta
```

La Venta valida previamente que exista stock suficiente, por lo que el resultado no debe quedar negativo.

Cuando una Venta se anula y cumple sus reglas:

```text
StockPosterior = StockAnterior + CantidadVendida
```

Se genera:

```text
Tipo = AnulacionVenta
```

---

# 8. Movimientos por Compra

Al confirmar una Compra:

```text
StockPosterior = StockAnterior + CantidadComprada
```

Se genera:

```text
Tipo = Compra
```

Cuando una Compra se anula:

```text
StockPosterior = StockAnterior - CantidadComprada
```

Se genera:

```text
Tipo = AnulacionCompra
```

La anulación sólo puede realizarse si existe stock suficiente para retirar esas unidades.

---

# 9. Movimientos por reintegro de Venta

El sistema contempla tipos específicos para las variaciones producidas por reintegros:

```text
ReintegroVenta
AnulacionReintegroVenta
```

`MovimientoStock` puede conservar la referencia:

```text
ReintegroVentaId
```

Esto permite distinguir estos movimientos de una anulación completa de Venta.

---

# 10. Movimientos por devolución de Compra

El sistema contempla:

```text
DevolucionCompra
AnulacionDevolucionCompra
```

`MovimientoStock` puede conservar:

```text
DevolucionCompraId
```

La lógica comercial específica se encuentra en el módulo DevolucionCompra.

---

# 11. Ajustes manuales de stock

Los ajustes manuales ya están implementados.

Actualmente sólo `AdminEmpresa` puede utilizar las acciones `Ajustar`.

El producto debe:

- Existir.
- Pertenecer a la empresa del usuario.
- Estar activo.

Existen dos tipos de ajuste:

```text
Entrada -> AjusteEntrada
Salida -> AjusteSalida
```

Cada ajuste registra un `Motivo`.

---

# 12. Ajuste de entrada

Para una entrada manual:

```text
StockPosterior = StockAnterior + Cantidad
```

El movimiento generado utiliza:

```text
Tipo = AjusteEntrada
```

La cantidad debe ser válida según el ViewModel.

---

# 13. Ajuste de salida

Para una salida manual:

```text
StockPosterior = StockAnterior - Cantidad
```

Antes de aplicarla se valida:

```text
Cantidad <= StockAnterior
```

Si la cantidad solicitada supera el stock disponible, el ajuste se rechaza.

El movimiento utiliza:

```text
Tipo = AjusteSalida
```

---

# 14. Motivo de ajuste

En ajustes manuales el motivo se obtiene del ViewModel y se almacena en:

```text
MovimientoStock.Motivo
```

El modelo permite hasta 250 caracteres.

Los movimientos automáticos pueden no utilizar un motivo textual porque su origen se expresa mediante `Tipo` y, cuando corresponde, mediante una referencia comercial.

---

# 15. Stock no negativo

El modelo valida:

```text
StockAnterior >= 0
StockPosterior >= 0
```

Además, los flujos de salida revisados actualmente evitan stock negativo:

- Venta valida disponibilidad.
- Anulación de Compra valida disponibilidad.
- AjusteSalida valida disponibilidad.

Por lo tanto, el comportamiento operativo actual trabaja sin stock negativo.

---

# 16. Consulta de stock actual

`MovimientoStockController.Index` utiliza los Productos como fuente del stock actual.

Permite buscar por:

- Nombre.
- Código de barras.

Y clasifica el stock en:

```text
SinStock: Stock == 0
StockBajo: Stock > 0 y Stock <= PuntoReposicion
ConStock: Stock > PuntoReposicion
```

La página utiliza 20 productos por página.

La vista también informa si el Producto se encuentra activo.

---

# 17. Historial de movimientos

La acción `Historial` permite filtrar por:

- Empresa para `SuperAdmin`.
- Producto.
- Tipo de movimiento.
- Fecha desde.
- Fecha hasta.

Los resultados se ordenan por:

```text
Fecha descendente
Id descendente
```

Actualmente no se aplica paginación al listado del historial dentro del controller revisado.

---

# 18. Información presentada en historial

Cada fila expone actualmente:

- Id.
- Fecha.
- Producto.
- Código de barras.
- Empresa.
- Usuario.
- Tipo.
- Cantidad.
- Stock anterior.
- Stock posterior.
- Motivo.
- VentaId.

Aunque el modelo también posee `CompraId`, `ReintegroVentaId` y `DevolucionCompraId`, el ViewModel actual del historial revisado no proyecta todas esas referencias.

---

# 19. Consulta de detalle

La acción `Details` permite consultar un MovimientoStock puntual.

Para usuarios que no son `SuperAdmin`, la consulta queda limitada mediante:

```text
MovimientoStock.EmpresaId == usuario.EmpresaId
```

Se cargan actualmente:

- Empresa.
- Producto.
- Usuario.
- Venta.

El modelo posee otras referencias opcionales aunque la acción actual no incluya explícitamente todas sus navegaciones.

---

# 20. Seguridad multiempresa

`MovimientoStock` posee `EmpresaId` propio.

Para usuarios `AdminEmpresa`:

- Stock actual se filtra por su empresa.
- Historial se filtra por su empresa.
- Detalles se filtran por su empresa.
- Ajustes sólo permiten productos de su empresa.

Para `SuperAdmin` puede utilizarse filtro de empresa en las vistas correspondientes.

Nunca debe aceptarse un `ProductoId` de otro tenant para generar un movimiento.

---

# 21. Inmutabilidad

Actualmente no existen acciones de:

- Editar MovimientoStock.
- Eliminar MovimientoStock.

Una operación incorrecta debe resolverse mediante una operación compensatoria o la anulación funcional del documento comercial que originó el movimiento.

Ejemplos:

```text
Venta -> AnulacionVenta
Compra -> AnulacionCompra
ReintegroVenta -> AnulacionReintegroVenta
DevolucionCompra -> AnulacionDevolucionCompra
```

Esto conserva ambos eventos en lugar de modificar el historial anterior.

---

# 22. Relación entre stock actual e historial

El stock operativo actual se almacena en:

```text
Producto.Stock
```

`MovimientoStock` constituye el historial de cambios.

Por lo tanto:

- `Producto.Stock` permite conocer rápidamente la existencia actual.
- `MovimientoStock` permite reconstruir cómo se llegó a ese valor.

Los movimientos no sustituyen el campo de stock actual del Producto.

---

# 23. Atomicidad

Los flujos críticos revisados coordinan el cambio de `Producto.Stock` y la creación del MovimientoStock dentro de una transacción.

Esto aplica, entre otros, a:

- Ajustes manuales.
- Ventas.
- Compras.
- Anulaciones comerciales.

La intención es impedir que quede actualizado el stock sin su movimiento correspondiente o viceversa.

---

# 24. Reglas de negocio

1. Cada MovimientoStock pertenece a una Empresa.
2. Cada movimiento corresponde a un Producto.
3. Actualmente no existe relación con Sucursal.
4. La cantidad registrada es positiva.
5. La dirección del cambio se interpreta mediante `Tipo` y los stocks anterior/posterior.
6. StockAnterior y StockPosterior no pueden ser negativos según el modelo.
7. El movimiento registra fecha y usuario.
8. El motivo es opcional a nivel de entidad, pero forma parte del flujo de ajustes manuales.
9. Las operaciones automáticas generan movimientos específicos según su origen.
10. Existen tipos compensatorios para anulaciones.
11. Los movimientos no poseen edición administrativa.
12. Los movimientos no poseen eliminación administrativa.
13. El stock actual reside en `Producto.Stock`.
14. El historial reside en `MovimientoStock`.
15. Los ajustes de salida no pueden superar el stock disponible.
16. Sólo `AdminEmpresa` puede ejecutar actualmente `Ajustar`.
17. La seguridad multiempresa debe validarse antes de cualquier cambio de stock.
18. Las referencias comerciales son opcionales y dependen del tipo de movimiento.

---

# 25. Casos de error relevantes

- Producto inexistente.
- Producto perteneciente a otra empresa.
- Producto inactivo al intentar ajustarlo.
- Usuario no autenticado.
- Usuario sin rol permitido.
- Cantidad de ajuste inválida.
- Tipo de ajuste inválido.
- Salida superior al stock disponible.
- Movimiento inexistente al consultar detalle.
- Error de persistencia durante el ajuste.

Ante un error de persistencia en un ajuste, la transacción se revierte y se restaura el valor de stock manejado por el contexto.

---

# 26. Integraciones actuales

MovimientoStock se integra actualmente con:

- Producto.
- Empresa.
- Usuario.
- Venta.
- Compra.
- ReintegroVenta.
- DevolucionCompra.
- Ajustes manuales de stock.
- Reportes y consultas de inventario.

Actualmente no existe integración con Sucursal.

---

# 27. Capacidades no implementadas

Actualmente no forman parte de `MovimientoStock`:

- Transferencia de stock entre sucursales.
- Transferencia entre depósitos.
- Lotes.
- Números de serie.
- Documento de referencia genérico.
- IP del usuario.
- Inventario físico formal como documento.
- Conteos cíclicos formales.
- Ajustes masivos.
- Exportación específica del historial desde este controller.
- Dashboard específico de movimientos.

---

# 28. Evolución futura

La evolución del inventario se administra mediante Roadmap y GitHub Issues.

Entre las capacidades previstas o posibles se encuentran:

- Inventario físico.
- Conteos cíclicos.
- Transferencias entre futuras sucursales/depósitos.
- Lotes y vencimientos.
- Series.
- Stock reservado.
- Reposición sugerida.
- Alertas y análisis de rotación.
- Mejoras de exportación y auditoría.

No se mantiene un roadmap de versiones independiente dentro de este documento.

---

# 29. Estado

✅ Stock actual por Producto implementado.

✅ Historial de MovimientoStock implementado.

✅ Stock inicial implementado como tipo de movimiento.

✅ Ajustes manuales de entrada/salida implementados.

✅ Movimientos de Venta y anulación implementados.

✅ Movimientos de Compra y anulación implementados.

✅ Tipos para reintegros y anulaciones implementados.

✅ Tipos para devoluciones de Compra y anulaciones implementados.

✅ Seguridad multiempresa implementada.

✅ Filtros de historial implementados.

🚧 Transferencias, inventarios físicos, conteos cíclicos, lotes y series reservados para evolución futura.