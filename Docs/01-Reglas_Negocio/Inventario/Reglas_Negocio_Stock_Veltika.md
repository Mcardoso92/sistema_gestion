# Reglas Generales de Negocio — Inventario y Stock

**Proyecto:** Veltika  
**Última actualización:** 01/09/2026  
**Estado:** Alineado con la implementación actual

---

# 1. Propósito

Este documento resume las reglas generales del dominio de Inventario y Stock de Veltika.

No reemplaza la documentación específica de:

- `14-MovimientoStock.md`
- `15-Stock.md`
- `16-AjusteStock.md`

Cuando exista una regla más detallada en esos documentos, debe considerarse esa documentación junto con el código actual como fuente de verdad.

---

# 2. Principio general

La arquitectura actual utiliza:

```text
Producto.Stock -> existencia actual
MovimientoStock -> historial de variaciones
```

No existe una entidad `Stock` independiente.

No existe una entidad `AjusteStock` independiente.

Actualmente tampoco existe stock por Sucursal o Depósito.

---

# 3. Stock actual

1. `Producto.Stock` es el valor operativo actual del inventario.
2. El stock utiliza tipo `int`.
3. El stock no puede ser negativo.
4. Un Producto puede crearse con stock inicial cero o mayor.
5. Si existe stock inicial mayor a cero, debe generarse trazabilidad mediante `MovimientoStock.StockInicial`.
6. Un Producto inactivo conserva el stock que tenía.
7. Reactivar un Producto no reinicia su stock.
8. El stock actual no se recalcula sumando todos los movimientos cada vez que se consulta.
9. `MovimientoStock` explica cómo se llegó al valor almacenado en `Producto.Stock`.

---

# 4. Punto de reposición

`Producto.PuntoReposicion` está implementado.

La clasificación actual es:

```text
SinStock  -> Stock == 0
StockBajo -> Stock > 0 && Stock <= PuntoReposicion
ConStock  -> Stock > PuntoReposicion
```

Por lo tanto, el concepto de reposición mínima ya forma parte de la versión actual.

---

# 5. Trazabilidad

Toda variación operativa de inventario debe generar un `MovimientoStock` asociado al mismo cambio de stock.

Cada movimiento registra actualmente:

- Producto.
- Empresa.
- Tipo.
- Cantidad.
- Stock anterior.
- Stock posterior.
- Fecha.
- Usuario.
- Motivo opcional.
- Referencias comerciales opcionales según el origen.

Los movimientos no poseen edición ni eliminación administrativa.

Una corrección debe generar una operación compensatoria en lugar de alterar el historial anterior.

---

# 6. Tipos de movimiento actuales

El enum vigente contiene:

```text
StockInicial
AjusteEntrada
AjusteSalida
Venta
AnulacionVenta
Compra
AnulacionCompra
ReintegroVenta
AnulacionReintegroVenta
DevolucionCompra
AnulacionDevolucionCompra
```

La lista debe mantenerse sincronizada con `TipoMovimientoStock`.

---

# 7. Ajustes manuales

Actualmente los ajustes manuales están implementados.

Reglas principales:

1. Sólo `AdminEmpresa` puede utilizar actualmente `MovimientoStockController.Ajustar`.
2. El Producto debe pertenecer a la empresa del usuario.
3. El Producto debe estar activo.
4. Existen dos tipos: Entrada y Salida.
5. La cantidad debe ser mayor a cero.
6. El usuario no ingresa el stock final; el servidor lo calcula.
7. El motivo es obligatorio y admite hasta 250 caracteres.
8. Una salida no puede superar el stock disponible.
9. Cada ajuste genera un `MovimientoStock`.
10. Stock y movimiento se persisten dentro de una misma transacción.

---

# 8. Integración con Ventas

Al confirmar una Venta:

```text
Producto.Stock -= CantidadVendida
```

Reglas:

1. No puede venderse una cantidad superior al stock disponible.
2. Se genera un movimiento `Venta` por Producto afectado.
3. El movimiento puede referenciar `VentaId`.
4. Stock, Venta y movimientos se procesan dentro del flujo transaccional correspondiente.
5. Anular una Venta restaura las cantidades vendidas.
6. La restauración genera `AnulacionVenta`.
7. El movimiento original no se elimina.

---

# 9. Integración con Compras

Compras ya está implementado y no debe considerarse funcionalidad futura.

Al confirmar una Compra:

```text
Producto.Stock += CantidadComprada
```

Reglas:

1. Se genera un movimiento `Compra` por Producto afectado.
2. El movimiento puede referenciar `CompraId`.
3. La Compra actualiza además el costo del Producto según sus propias reglas.
4. Anular una Compra intenta retirar las unidades previamente ingresadas.
5. La anulación no puede provocar stock negativo.
6. La anulación genera `AnulacionCompra`.
7. Los movimientos originales permanecen en el historial.

---

# 10. Reintegros de Venta

El inventario contempla actualmente:

```text
ReintegroVenta
AnulacionReintegroVenta
```

`MovimientoStock` puede almacenar:

```text
ReintegroVentaId
```

Estos movimientos permiten registrar cambios parciales vinculados con reintegros sin confundirlos con la anulación total de una Venta.

---

# 11. Devoluciones de Compra

El inventario contempla actualmente:

```text
DevolucionCompra
AnulacionDevolucionCompra
```

`MovimientoStock` puede almacenar:

```text
DevolucionCompraId
```

Estos procesos poseen sus propias reglas comerciales y modifican el inventario mediante movimientos específicos.

---

# 12. Productos inactivos

1. Un Producto inactivo conserva su stock.
2. Conserva su historial de movimientos.
3. Puede consultarse su información histórica.
4. No puede recibir ajustes manuales mediante el flujo actual.
5. Otros flujos operativos también deben respetar las restricciones de Producto activo que correspondan.

---

# 13. Seguridad multiempresa

1. `Producto` pertenece a una Empresa mediante `EmpresaId`.
2. `MovimientoStock` también almacena `EmpresaId`.
3. `AdminEmpresa` sólo puede consultar y operar sobre inventario de su propia empresa.
4. Nunca debe confiarse en un `EmpresaId` enviado por el navegador para autorizar un cambio de stock.
5. El servidor debe validar la pertenencia del Producto antes de modificar inventario.
6. El acceso directo mediante IDs de otra empresa debe rechazarse.
7. `SuperAdmin` puede consultar stock de múltiples empresas mediante el contexto/filtro correspondiente.

---

# 14. Consulta y UX actual

El módulo permite actualmente:

- Listar productos con stock.
- Buscar por nombre.
- Buscar por código de barras.
- Filtrar por estado de stock.
- Filtrar por empresa para `SuperAdmin`.
- Consultar PuntoReposicion.
- Acceder al historial.
- Filtrar historial por Producto.
- Filtrar historial por tipo.
- Filtrar historial por fecha desde/hasta.
- Acceder al ajuste manual para `AdminEmpresa`.

El listado principal utiliza paginación de 20 productos por página.

El historial actual no posee paginación en el controller revisado.

---

# 15. Valorización

La valorización de inventario ya está disponible mediante Reportes de Stock.

Puede calcularse utilizando los datos actuales del Producto, por ejemplo:

```text
Valor a costo = Stock × PrecioCosto
Valor a venta = Stock × PrecioVenta
```

No existe un campo persistido `ValorInventario` dentro de una entidad Stock.

---

# 16. Integridad técnica

1. El servidor obtiene el stock actual desde base de datos antes de operaciones sensibles.
2. El servidor calcula el stock posterior.
3. No se confía en un stock final calculado por el navegador.
4. Usuario, empresa y fecha se determinan en servidor según el flujo.
5. Los cambios de inventario y sus movimientos deben mantenerse coordinados transaccionalmente.
6. Los movimientos no utilizan Soft Delete como mecanismo administrativo.
7. Los movimientos históricos no deben reescribirse para corregir una operación pasada.
8. Las salidas deben validar disponibilidad suficiente antes de persistirse.

---

# 17. Lo que no existe actualmente

No forman parte de la implementación vigente:

- Stock por Sucursal.
- Stock por Depósito.
- Transferencias entre ubicaciones.
- Stock reservado.
- Stock comprometido.
- Reservas de inventario.
- Inventario físico formal.
- Conteos cíclicos.
- Lotes.
- Vencimientos.
- Números de serie.
- Stock máximo.
- Política configurable de stock negativo.
- Ajustes masivos.
- Aprobación de ajustes.
- Evidencia fotográfica o adjuntos para ajustes.

---

# 18. Evolución futura

La evolución del inventario debe gestionarse desde el Roadmap y GitHub Issues, evitando duplicar planes de versiones en este documento.

Entre las capacidades previstas o posibles se encuentran:

- Inventario físico.
- Conteos cíclicos.
- Sucursales y depósitos.
- Existencias por ubicación.
- Transferencias.
- Reservas de stock.
- Lotes y vencimientos.
- Series.
- Reposición sugerida.
- Días de stock.
- Rotación.
- Productos sin movimiento.
- Alertas avanzadas.
- Permisos granulares.

---

# 19. Documentos relacionados

Para detalle funcional consultar:

```text
14-MovimientoStock.md
15-Stock.md
16-AjusteStock.md
```

También deben considerarse las reglas específicas de:

- Producto.
- Venta.
- Compra.
- ReintegroVenta.
- DevolucionCompra.
- Reportes.

---

# 20. Estado actual

✅ `Producto.Stock` como existencia actual.

✅ `MovimientoStock` como trazabilidad histórica.

✅ Punto de reposición.

✅ Stock inicial trazable.

✅ Ajustes manuales.

✅ Integración con Venta y anulación.

✅ Integración con Compra y anulación.

✅ Integración con ReintegroVenta.

✅ Integración con DevolucionCompra.

✅ Seguridad multiempresa.

✅ Consulta y filtros de inventario.

✅ Valorización mediante Reportes.

🚧 Inventario físico, ubicaciones múltiples, reservas y trazabilidad avanzada quedan para evolución futura.