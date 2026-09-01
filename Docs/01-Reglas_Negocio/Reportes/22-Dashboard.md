# Módulo Dashboard

Última actualización: 01/09/2026

---

# 1. Objetivo

El Dashboard proporciona una vista resumida del estado comercial y operativo de Veltika mediante indicadores y rankings construidos a partir de los datos existentes.

Su objetivo actual es ofrecer una lectura rápida de:

- Ventas del día.
- Ventas del mes.
- Productos con Stock bajo.
- Productos más vendidos.
- Clientes frecuentes.
- Evolución de Ventas de los últimos 7 días.

El Dashboard no reemplaza a los reportes detallados ni constituye todavía una plataforma completa de Business Intelligence.

---

# 2. Arquitectura actual

No existe una entidad persistida `Dashboard`.

El módulo está implementado mediante:

```text
DashboardController
+ DashboardVM
+ ViewModels auxiliares
+ consultas de solo lectura
+ vista Razor
```

Los datos se calculan al solicitar la pantalla a partir de las entidades operativas existentes.

---

# 3. Acceso

`DashboardController` utiliza actualmente:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto, los roles autorizados hoy son:

- SuperAdmin.
- AdminEmpresa.

No existe actualmente autorización general para cualquier Usuario autenticado ni permisos granulares por widget dentro de este controller.

---

# 4. Alcance multiempresa

## AdminEmpresa

Las consultas de Ventas, Productos y DetallesVenta se restringen mediante la Empresa del usuario autenticado.

Conceptualmente:

```text
EmpresaId == usuario.EmpresaId
```

## SuperAdmin

Actualmente obtiene una vista global porque las consultas no se restringen por Empresa.

La vista recibe:

```text
ViewBag.EsVistaGlobal = true
```

cuando el usuario es SuperAdmin.

Actualmente Dashboard no posee selector de Empresa para SuperAdmin.

---

# 5. Naturaleza informativa

El Dashboard es una pantalla de consulta.

Las consultas revisadas utilizan:

```text
AsNoTracking()
```

No modifica Ventas, Productos, Clientes ni otras entidades del sistema.

Las operaciones administrativas deben realizarse en los módulos correspondientes.

---

# 6. Ventas consideradas

Todos los indicadores comerciales del Dashboard parten de Ventas con:

```text
Venta.Estado == true
```

Las Ventas anuladas no forman parte de los cálculos actuales.

---

# 7. Ventas del día

El Dashboard calcula:

```text
TotalVentasDia
CantidadVentasDia
```

El período se define desde:

```text
DateTime.Today
```

hasta el inicio del día siguiente.

Conceptualmente:

```text
Fecha >= hoy
Fecha < mañana
```

---

# 8. Ventas del mes

El Dashboard calcula:

```text
TotalVentasMes
CantidadVentasMes
```

El período comienza el primer día del mes actual y finaliza al terminar el día actual.

Conceptualmente:

```text
Fecha >= primer día del mes
Fecha < mañana
```

No calcula actualmente comparación contra el mes anterior.

---

# 9. Productos con Stock bajo

El Dashboard consulta Productos:

```text
Estado == true
Stock <= PuntoReposicion
```

Los resultados se ordenan por:

1. Stock ascendente.
2. Nombre ascendente.

Y se limita a:

```text
Top 5
```

Cada elemento muestra:

- ProductoId.
- Nombre.
- Código de barras.
- Stock.
- Punto de reposición.

Esta consulta incluye también Productos con Stock igual a cero, porque cumplen la condición `Stock <= PuntoReposicion`.

---

# 10. Productos más vendidos

El ranking de Productos más vendidos está implementado actualmente.

Se construye sobre `DetalleVenta` perteneciente a Ventas activas.

Se agrupa por:

```text
ProductoId
Producto.Nombre
```

Y se calcula:

```text
CantidadVendida = SUM(DetalleVenta.Cantidad)
ImporteVendido = SUM(DetalleVenta.Subtotal)
```

Los resultados se ordenan principalmente por CantidadVendida descendente y se limita a:

```text
Top 5
```

---

# 11. Alcance temporal de Productos más vendidos

Actualmente el ranking de Productos más vendidos no está limitado al día ni al mes.

Utiliza todas las Ventas activas disponibles dentro del alcance empresarial del Dashboard.

Por lo tanto debe interpretarse como un ranking histórico acumulado según los datos existentes, no como "productos más vendidos del mes".

Una futura evolución puede agregar filtros temporales o comparaciones por período.

---

# 12. Clientes frecuentes

El ranking de Clientes frecuentes está implementado.

Sólo se consideran Ventas que:

```text
ClienteId != null
Cliente.Estado == true
```

Las Ventas a Consumidor Final no forman parte de este ranking.

---

# 13. Métricas de Cliente frecuente

Los Clientes se agrupan por identidad y se calcula:

```text
CantidadCompras = cantidad de Ventas
ImporteComprado = suma de Venta.Total
```

El orden actual es:

1. CantidadCompras descendente.
2. ImporteComprado descendente.

Y se limita a:

```text
Top 5
```

Cada registro muestra:

- ClienteId.
- Nombre completo.
- Cantidad de compras.
- Importe comprado.

---

# 14. Alcance temporal de Clientes frecuentes

Actualmente el ranking utiliza todas las Ventas activas disponibles dentro del alcance empresarial correspondiente.

No está limitado al mes actual ni a los últimos días.

Por lo tanto representa frecuencia histórica acumulada sobre los datos vigentes.

---

# 15. Ventas de los últimos 7 días

El Dashboard calcula una serie temporal:

```text
VentasUltimosDias
```

El período incluye:

```text
hoy + los 6 días anteriores
```

para un total de:

```text
7 días
```

---

# 16. Días sin Ventas

La serie temporal genera explícitamente los 7 días del rango.

Si un día no posee Ventas, se asigna:

```text
Total = 0
```

Esto permite que la visualización mantenga continuidad temporal y no omita fechas sin actividad.

---

# 17. DashboardVM

El ViewModel principal contiene actualmente:

```text
TotalVentasDia
CantidadVentasDia
TotalVentasMes
CantidadVentasMes
ProductosStockBajo
ProductosMasVendidos
ClientesFrecuentes
VentasUltimosDias
```

No posee actualmente campos para:

- Compras del mes.
- Proveedores registrados.
- Total de Clientes.
- Total de Productos.
- Caja actual.
- Rentabilidad.
- Margen.
- Compras pendientes.
- Productos sin movimiento.
- Indicadores financieros avanzados.

---

# 18. ProductoStockBajoVM

Contiene:

```text
ProductoId
Nombre
CodigoBarra
Stock
PuntoReposicion
```

Su finalidad es mostrar un resumen operativo de Productos que requieren atención.

---

# 19. ProductoMasVendidoVM

Contiene:

```text
ProductoId
Nombre
CantidadVendida
ImporteVendido
```

El importe mostrado corresponde a la suma histórica de subtotales de DetalleVenta incluidos en el ranking actual.

---

# 20. ClienteFrecuenteVM

Contiene:

```text
ClienteId
NombreCompleto
CantidadCompras
ImporteComprado
```

Actualmente no calcula:

- Ticket promedio por Cliente.
- Fecha de última compra.
- Días desde última compra.
- Segmentación.
- Tendencia de compra.

Estas métricas pueden incorporarse posteriormente si aportan valor operativo.

---

# 21. VentaDiariaVM

Contiene:

```text
Fecha
Total
```

Actualmente la serie temporal sólo representa importe vendido por día.

No incluye en ese gráfico:

- Cantidad de Ventas.
- Ticket promedio diario.
- Margen.
- Comparación con período anterior.

---

# 22. Compras

Aunque la documentación anterior mencionaba `Compras del mes` como indicador inicial, actualmente DashboardController no consulta Compras.

Por lo tanto no existe hoy en Dashboard:

```text
TotalComprasMes
CantidadComprasMes
```

ni indicadores equivalentes.

---

# 23. Caja

Aunque la documentación anterior mencionaba `Caja actual`, DashboardController no consulta actualmente:

- Caja.
- TurnoCaja.
- MovimientoCaja.

Por lo tanto no muestra hoy saldo de Caja, turno abierto, ingresos/egresos ni diferencias de arqueo.

Estos indicadores pueden evaluarse posteriormente como parte de un resumen financiero operativo.

---

# 24. Clientes, Proveedores y Productos registrados

El Dashboard actual no calcula contadores generales de:

```text
Clientes registrados
Proveedores registrados
Productos registrados
```

Aunque esos datos existen en otros módulos, no deben documentarse como indicadores vigentes del Dashboard hasta que el controller los incorpore.

---

# 25. Alertas

El Dashboard actual expone visualmente información que puede requerir atención, especialmente:

```text
Productos con Stock bajo
```

Sin embargo, esto no equivale a un motor completo de alertas configurables.

Veltika posee otros componentes de notificaciones, pero DashboardController no implementa actualmente reglas configurables de alertas empresariales dentro de este módulo.

---

# 26. Accesos rápidos

La navegación desde tarjetas, rankings o elementos visuales depende de la implementación de la vista.

No debe establecerse como regla arquitectónica que absolutamente todos los indicadores deban ser enlaces.

Cuando un acceso directo mejore la operación puede dirigir al módulo detallado correspondiente.

---

# 27. Actualización de datos

Los datos se recalculan al ejecutar la acción `DashboardController.Index`.

Actualmente no existe en el controller:

- actualización en tiempo real;
- WebSockets/SignalR para métricas;
- polling automático;
- caché específica del Dashboard;
- almacenamiento de snapshots.

Una recarga de la página obtiene nuevamente los datos desde la base.

---

# 28. Personalización

Actualmente no existen configuraciones persistidas por usuario para decidir:

- Qué widgets mostrar.
- Orden de tarjetas.
- Tamaño de widgets.
- Métricas favoritas.
- Períodos predeterminados.

Un Dashboard personalizable queda como evolución futura y debe justificarse por necesidad real de usuarios.

---

# 29. Permisos granulares

Actualmente la seguridad del Dashboard es a nivel de controller mediante roles:

```text
SuperAdmin
AdminEmpresa
```

No existe lógica actual para ocultar cada indicador según permisos granulares del empleado.

Cuando se implemente el sistema futuro de permisos granulares deberá definirse si cada widget requiere autorización específica.

---

# 30. Sucursales

Actualmente Dashboard no trabaja con Sucursal porque el dominio multi-sucursal todavía no está implementado productivamente.

No existe actualmente:

- Dashboard por Sucursal.
- Ranking de Sucursales.
- Comparación entre Sucursales.
- selector de Sucursal.

Estas funcionalidades dependen primero del diseño del módulo Sucursal.

---

# 31. Rentabilidad y margen

Actualmente Dashboard no calcula:

- Margen bruto.
- Rentabilidad.
- Costo de mercadería vendida.
- Contribución por Producto.
- Beneficio estimado.

Estos indicadores forman parte de una futura capa de BI y requieren definir correctamente reglas históricas de costo antes de mostrarse como información confiable.

---

# 32. Comparativas

Actualmente no existen comparaciones automáticas como:

```text
Hoy vs ayer
Mes actual vs mes anterior
Últimos 7 días vs 7 días anteriores
```

Tampoco se calculan porcentajes de crecimiento o caída.

Estas capacidades pueden incorporarse posteriormente como evolución del Dashboard.

---

# 33. Predicciones e IA

Actualmente no existen en Dashboard:

- Predicción de Ventas.
- Forecast de Stock.
- Recomendaciones generadas por IA.
- Detección automática de anomalías.

Estas capacidades no forman parte del MVP actual y sólo deberían evaluarse después de contar con datos suficientes y una necesidad observada.

---

# 34. Reglas de negocio

1. Dashboard es una pantalla informativa y de solo lectura.
2. No existe una entidad Dashboard persistida.
3. El acceso actual corresponde a SuperAdmin y AdminEmpresa.
4. AdminEmpresa sólo visualiza datos de su Empresa.
5. SuperAdmin obtiene actualmente una vista global.
6. No existe selector de Empresa en Dashboard para SuperAdmin.
7. Sólo se consideran Ventas activas.
8. Ventas del día calcula total y cantidad.
9. Ventas del mes calcula total y cantidad.
10. Stock bajo utiliza `Stock <= PuntoReposicion`.
11. Stock bajo muestra hasta 5 Productos.
12. Productos más vendidos muestra Top 5 por cantidad acumulada.
13. Productos más vendidos no posee filtro temporal actualmente.
14. Clientes frecuentes muestra Top 5 por cantidad de compras e importe.
15. Clientes frecuentes excluye Consumidor Final y Clientes inactivos.
16. Clientes frecuentes no posee filtro temporal actualmente.
17. VentasUltimosDias representa 7 días incluyendo hoy.
18. Los días sin Ventas se muestran con Total cero.
19. Dashboard no consulta actualmente Compras.
20. Dashboard no consulta actualmente Caja.
21. Dashboard no calcula actualmente rentabilidad ni margen.
22. No existe personalización por Usuario.
23. No existen permisos granulares por widget.
24. No existe Dashboard por Sucursal.
25. Los datos se recalculan al cargar la pantalla.

---

# 35. Casos de error relevantes

- Usuario no autenticado.
- Usuario sin rol autorizado.
- Error de acceso a base de datos.

La ausencia de Ventas, Productos con Stock bajo o Clientes frecuentes no constituye un error.

En esos casos los indicadores pueden mostrar:

```text
0
```

o colecciones vacías según corresponda.

---

# 36. Integraciones actuales

Dashboard utiliza actualmente información de:

- Venta.
- DetalleVenta.
- Producto.
- Cliente.
- Empresa mediante el alcance multiempresa.
- Usuario para determinar contexto y rol.

No consulta directamente actualmente:

- Compra.
- Proveedor.
- Caja.
- TurnoCaja.
- MovimientoCaja.
- MovimientoStock.

---

# 37. Capacidades futuras

Entre las evoluciones posibles se encuentran:

- Comparativas entre períodos.
- Tendencias de Ventas.
- Ticket promedio.
- Margen y rentabilidad.
- Días de Stock.
- Baja rotación.
- Reposición sugerida.
- Indicadores de Caja.
- Diferencias de arqueo.
- Medios de pago.
- Compras y Proveedores.
- Metas comerciales.
- Notificaciones accionables.
- Dashboard personalizable.
- Permisos granulares por widget.
- Dashboard por Sucursal.
- Resumen operativo configurable.

Estas capacidades deben priorizarse según necesidades observadas y no asumirse como implementadas.

---

# 38. Estado actual

✅ Total de Ventas del día implementado.

✅ Cantidad de Ventas del día implementada.

✅ Total de Ventas del mes implementado.

✅ Cantidad de Ventas del mes implementada.

✅ Top 5 de Productos con Stock bajo implementado.

✅ Top 5 de Productos más vendidos implementado.

✅ Top 5 de Clientes frecuentes implementado.

✅ Serie de Ventas de los últimos 7 días implementada.

✅ Días sin actividad representados con valor cero.

✅ Seguridad multiempresa para AdminEmpresa implementada.

✅ Vista global para SuperAdmin implementada.

🚧 Comparativas históricas pendientes.

🚧 Rentabilidad y margen pendientes.

🚧 Indicadores de Caja pendientes.

🚧 Indicadores de Compras pendientes.

🚧 Personalización pendiente.

🚧 BI avanzado pendiente.