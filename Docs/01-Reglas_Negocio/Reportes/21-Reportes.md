# Módulo Reportes

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Reportes permite consultar y exportar información operativa de Veltika sin modificar los datos de origen.

Actualmente el módulo posee reportes concretos sobre:

- Ventas.
- Stock.
- Clientes.

Los reportes trabajan directamente sobre la información persistida por los módulos operativos y respetan la separación multiempresa.

---

# 2. Arquitectura actual

No existe actualmente una entidad genérica `Reporte` con campos como Id, Nombre, Descripción, FechaGeneracion o UsuarioId.

El módulo está implementado mediante:

```text
ReporteController
+ ViewModels específicos
+ consultas de solo lectura
+ vistas Razor
+ exportaciones Excel
```

Por lo tanto, cada reporte se modela según la información que necesita mostrar y no mediante una tabla central de reportes.

---

# 3. Acceso

`ReporteController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede consultar información de múltiples Empresas y utilizar los filtros de Empresa disponibles en cada reporte.

## AdminEmpresa

Sólo puede consultar información correspondiente a su propia Empresa.

Actualmente otros roles no poseen acceso mediante este controller.

---

# 4. Principio de solo lectura

Los reportes utilizan consultas con:

```text
AsNoTracking()
```

en los flujos revisados.

El módulo no modifica Ventas, Productos, Clientes ni otras entidades operativas como parte de la generación de un reporte.

Las exportaciones también consultan los datos existentes y generan archivos a partir del resultado.

---

# 5. Seguridad multiempresa

Para AdminEmpresa, las consultas se restringen mediante:

```text
EmpresaId == usuario.EmpresaId
```

SuperAdmin puede seleccionar una Empresa concreta o, en determinados reportes, consultar un alcance mayor cuando no aplica filtro empresarial.

Cuando una Empresa es seleccionada explícitamente en los flujos de pantalla, se valida que exista y esté activa según las reglas del reporte.

Los filtros nunca deben utilizarse para ampliar el acceso del usuario fuera de su tenant autorizado.

---

# 6. Reporte de Ventas

El reporte de Ventas está implementado en:

```text
ReporteController.Ventas
```

Permite filtrar por:

- Fecha desde.
- Fecha hasta.
- Cliente.
- Empresa para SuperAdmin.

Sólo incluye Ventas con:

```text
Estado == true
```

---

# 7. Período por defecto de Ventas

Si el usuario no informa fechas, el reporte utiliza:

```text
FechaDesde = primer día del mes actual
FechaHasta = hoy
```

La fecha final se procesa como límite inclusivo a nivel de interfaz y se transforma internamente en un límite exclusivo del día siguiente.

Si:

```text
FechaHasta < FechaDesde
```

el reporte se considera inválido.

---

# 8. Datos mostrados por Venta

Cada fila del reporte incluye actualmente:

- VentaId.
- Fecha.
- Cliente.
- Usuario/Vendedor.
- Cantidad total de productos.
- Total de la Venta.

Cuando una Venta no posee Cliente se muestra:

```text
Consumidor final
```

La cantidad de productos se obtiene sumando las cantidades de los DetallesVenta.

---

# 9. Indicadores del reporte de Ventas

El reporte calcula actualmente:

```text
TotalVendido
CantidadVentas
TicketPromedio
```

Con la regla:

```text
TicketPromedio = TotalVendido / CantidadVentas
```

cuando existen Ventas.

Si no existen resultados:

```text
TicketPromedio = 0
```

---

# 10. Exportación Excel de Ventas

La exportación Excel está implementada mediante:

```text
ExportarVentasExcel
```

Utiliza `ClosedXML` y genera un archivo `.xlsx`.

Incluye actualmente las columnas:

- Número.
- Fecha.
- Cliente.
- Vendedor.
- Productos.
- Total.

El nombre del archivo sigue el formato:

```text
reporte-ventas-YYYYMMDD-YYYYMMDD.xlsx
```

Por lo tanto, la exportación Excel ya no debe figurar como funcionalidad futura.

---

# 11. Reporte de Stock

El reporte de Stock está implementado en:

```text
ReporteController.Stock
```

Trabaja sobre Productos activos.

Permite filtrar por:

- Categoría.
- Empresa para SuperAdmin.
- Situación de Stock.

---

# 12. Situaciones de Stock

Los valores admitidos actualmente son:

```text
todos
normal
bajo
sin-stock
```

Las reglas son:

```text
Normal:
Stock > PuntoReposicion

Bajo:
Stock > 0 && Stock <= PuntoReposicion

Sin stock:
Stock == 0
```

Un valor de filtro desconocido se normaliza a:

```text
todos
```

---

# 13. Datos mostrados por Producto

Cada fila de Stock incluye actualmente:

- ProductoId.
- Nombre.
- Código de barras.
- Categoría.
- Empresa.
- Stock actual.
- Punto de reposición.
- Precio de costo.
- Precio de venta.
- Valor al costo.
- Valor de venta.
- Situación.

---

# 14. Valorización de inventario

La valorización de inventario ya está implementada.

Por Producto:

```text
ValorCosto = PrecioCosto * Stock
ValorVenta = PrecioVenta * Stock
```

El reporte calcula además:

```text
ValorInventarioCosto
ValorInventarioVenta
```

como suma de los valores de los Productos incluidos en el resultado filtrado.

---

# 15. Indicadores del reporte de Stock

El reporte calcula actualmente:

```text
CantidadProductos
UnidadesStock
ProductosStockBajo
ValorInventarioCosto
ValorInventarioVenta
```

Estos indicadores se calculan sobre el conjunto resultante de los filtros aplicados.

---

# 16. Exportación Excel de Stock

La exportación está implementada mediante:

```text
ExportarStockExcel
```

Genera un archivo `.xlsx` utilizando `ClosedXML`.

Incluye columnas para:

- Código.
- Producto.
- Categoría.
- Empresa.
- Stock.
- Punto de reposición.
- Precio de costo.
- Precio de venta.
- Valor al costo.
- Valor de venta.
- Situación.

La exportación respeta los filtros de categoría, empresa y situación aplicados.

---

# 17. Reporte de Clientes

El reporte de Clientes también está implementado.

Utiliza:

```text
ReporteClientesVM
```

y permite analizar actividad comercial por Cliente.

Filtros actuales:

- Empresa.
- Estado.
- Actividad.
- Búsqueda.

---

# 18. Información mostrada por Cliente

Cada fila puede mostrar actualmente:

- ClienteId.
- Nombre completo.
- Documento.
- Email.
- Teléfono.
- Empresa.
- Cantidad de Compras/Ventas realizadas.
- Importe total comprado.
- Última compra.
- Estado del Cliente.

El reporte utiliza las Ventas asociadas al Cliente para construir sus métricas comerciales.

---

# 19. Indicadores del reporte de Clientes

`ReporteClientesVM` contempla actualmente:

```text
CantidadClientes
ClientesActivos
ClientesInactivos
ClientesConCompras
ImporteTotalComprado
```

Por lo tanto, Veltika ya dispone de una primera capa de análisis consolidado de Clientes.

Esto no equivale todavía a segmentación avanzada, cohortes, fidelización o BI completo.

---

# 20. Exportación Excel de Clientes

El reporte posee actualmente:

```text
ExportarClientesExcel
```

La exportación utiliza los mismos criterios principales de filtrado del reporte en pantalla:

- Empresa.
- Estado.
- Actividad.
- Búsqueda.

Por lo tanto, Clientes también posee exportación Excel implementada.

---

# 21. Filtros

Los filtros son específicos de cada reporte.

No existe actualmente una infraestructura genérica única que defina dinámicamente todos los filtros del sistema.

Ejemplos vigentes:

```text
Ventas   -> fechas, Cliente, Empresa
Stock    -> Categoría, situación, Empresa
Clientes -> estado, actividad, búsqueda, Empresa
```

Esto permite mantener cada consulta alineada con las necesidades del dominio correspondiente.

---

# 22. Exportación

Actualmente el formato implementado es:

```text
Excel (.xlsx)
```

No está implementada actualmente una exportación general a:

- PDF.
- CSV como estándar del módulo.
- Power BI.
- otros formatos configurables.

---

# 23. Generación de archivos

Las exportaciones Excel se generan en memoria y se devuelven directamente como archivo HTTP.

No se persiste actualmente una entidad de historial de exportaciones ni un archivo de reporte permanente en base de datos.

Por lo tanto no existen actualmente campos persistidos como:

```text
FechaGeneracion
CantidadRegistros
TiempoGeneracion
FormatoExportacion
Favorito
Programado
```

---

# 24. Ausencia de entidad Reporte

No debe documentarse un supuesto modelo:

```text
Reporte
```

con CRUD administrativo.

Los reportes actuales son casos de uso de consulta construidos sobre los modelos operativos existentes.

Esta estrategia evita duplicar información sólo para reportar.

---

# 25. Reportes no implementados todavía

Aunque existen módulos con información disponible, no debe asumirse que ya poseen un reporte dedicado en `ReporteController`.

Entre los reportes que pueden incorporarse posteriormente se encuentran:

- Compras por período/proveedor.
- Rentabilidad y margen.
- Caja y movimientos financieros consolidados.
- Diferencias de Caja históricas.
- Medios de pago.
- Proveedores.
- Movimiento de Stock dedicado.
- Productos sin movimiento.
- Rotación de inventario.
- Reposición sugerida.
- Auditoría administrativa.

---

# 26. Reportes gráficos y BI

Actualmente no existe un constructor de reportes gráficos ni una capa avanzada de Business Intelligence dentro de `ReporteController`.

Las visualizaciones resumidas existentes pertenecen principalmente al módulo Dashboard.

Quedan para evolución:

- Gráficos configurables.
- Comparaciones entre períodos.
- Tendencias.
- Rentabilidad.
- Contribución de margen.
- Días de stock.
- Baja rotación.
- Metas.
- Reportes personalizados.
- Integraciones con plataformas externas de BI si realmente aportan valor.

---

# 27. Programación y envío automático

Actualmente no están implementados en este módulo:

- Reportes programados.
- Envío periódico por email.
- Suscripciones a reportes.
- Reportes favoritos.

Estas funcionalidades pueden evaluarse posteriormente dentro del Roadmap de automatizaciones y notificaciones.

---

# 28. Sucursales

Actualmente los reportes revisados no poseen filtro por Sucursal porque el dominio Sucursal todavía no está implementado productivamente.

No debe documentarse `Sucursal` como filtro vigente.

Cuando exista multi-sucursal deberán revisarse todos los reportes para definir:

- alcance por Sucursal;
- vista consolidada por Empresa;
- permisos;
- stock y cajas por ubicación.

---

# 29. Reglas de negocio

1. Los reportes son consultas de solo lectura.
2. No existe una entidad genérica Reporte persistida.
3. El acceso actual corresponde a SuperAdmin y AdminEmpresa.
4. AdminEmpresa sólo consulta datos de su Empresa.
5. SuperAdmin puede utilizar alcance multiempresa según cada reporte.
6. Ventas sólo considera registros activos.
7. El período por defecto de Ventas va desde el primer día del mes hasta hoy.
8. FechaHasta no puede ser anterior a FechaDesde.
9. Ventas calcula TotalVendido, CantidadVentas y TicketPromedio.
10. Stock sólo trabaja con Productos activos.
11. Stock permite situación todos/normal/bajo/sin-stock.
12. La valorización de inventario al costo y a venta está implementada.
13. Clientes posee métricas de actividad comercial.
14. Ventas, Stock y Clientes poseen exportación Excel.
15. Las exportaciones deben respetar la seguridad multiempresa.
16. Las exportaciones no crean ni modifican entidades operativas.
17. No existe actualmente exportación PDF general.
18. No existen reportes programados ni enviados automáticamente.
19. No existe actualmente filtro productivo por Sucursal.

---

# 30. Casos de error relevantes

- Usuario no autenticado.
- Usuario sin rol autorizado.
- Empresa seleccionada inexistente o inactiva.
- FechaHasta anterior a FechaDesde.
- Filtros fuera de los valores admitidos.
- Intento de consultar datos de otra Empresa por un usuario sin autorización.

La ausencia de resultados no constituye necesariamente un error: un reporte puede devolver un conjunto vacío y totales en cero.

---

# 31. Integraciones actuales

Los reportes revisados utilizan información de:

- Empresa.
- Usuario.
- Venta.
- DetalleVenta.
- Cliente.
- Producto.
- Categoría.

Las exportaciones Excel utilizan:

```text
ClosedXML
```

Otros dominios podrán incorporarse a medida que se creen nuevos reportes específicos.

---

# 32. Capacidades futuras

Entre las evoluciones posibles se encuentran:

- Reporte de Compras.
- Reporte de Proveedores.
- Reportes financieros y de Caja.
- Reportes de MediosPago.
- Margen y rentabilidad.
- Rotación de inventario.
- Reposición sugerida.
- Productos sin movimiento.
- Comparaciones entre períodos.
- Exportación PDF.
- Reportes programados.
- Envío automático por email.
- Reportes personalizados.
- Permisos granulares por reporte.
- BI avanzado.

Estas capacidades deben gestionarse desde Roadmap/Issues y no asumirse como disponibles hasta estar implementadas.

---

# 33. Estado actual

✅ Reporte de Ventas implementado.

✅ Filtros de Ventas implementados.

✅ Total vendido, cantidad y ticket promedio implementados.

✅ Exportación Excel de Ventas implementada.

✅ Reporte de Stock implementado.

✅ Filtros de Stock implementados.

✅ Valorización de inventario al costo implementada.

✅ Valorización potencial a precio de venta implementada.

✅ Exportación Excel de Stock implementada.

✅ Reporte de Clientes implementado.

✅ Métricas básicas de Clientes implementadas.

✅ Exportación Excel de Clientes implementada.

✅ Seguridad multiempresa implementada.

🚧 Exportación PDF pendiente.

🚧 Reportes financieros/compras/proveedores especializados pendientes.

🚧 Programación y envío automático pendiente.

🚧 BI avanzado y reportes personalizados pendientes.