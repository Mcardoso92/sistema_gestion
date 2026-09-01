# Módulo DevolucionCompra

Última actualización: 01/09/2026

---

# 1. Objetivo

DevolucionCompra representa la devolución física y económica de productos previamente ingresados mediante una Compra.

Su objetivo es registrar que determinados productos dejan de permanecer en la Empresa y vuelven al Proveedor.

Conceptualmente:

```text
Compra
    -> ingresa stock

DevolucionCompra
    -> egresa stock hacia el Proveedor
```

La devolución modifica el valor neto de la Compra y participa en las reglas financieras relacionadas con PagoProveedor y ReintegroProveedor.

---

# 2. Modelo actual

`DevolucionCompra` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| CompraId | Compra asociada |
| EmpresaId | Empresa propietaria |
| UsuarioId | Usuario que registra |
| Fecha | Fecha/hora de devolución |
| Total | Valor económico devuelto |
| Estado | Activa/Anulada |
| Observaciones | Opcional, máximo 500 caracteres |
| FechaAnulacion | Fecha/hora de anulación |
| UsuarioAnulacionId | Usuario que anuló |
| MotivoAnulacion | Motivo, máximo 500 caracteres |

Relaciones actuales:

- Compra.
- Empresa.
- Usuario.
- UsuarioAnulacion.
- DetalleDevolucionCompra.
- MovimientoStock.

---

# 3. Autorización

`DevolucionCompraController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Además se aplican controles multiempresa dentro de cada acción.

---

# 4. Multiempresa

AdminEmpresa sólo puede operar sobre DevolucionCompra de:

```text
usuario.EmpresaId
```

La Compra y todos sus productos deben pertenecer al mismo contexto empresarial.

SuperAdmin puede operar globalmente según el contexto permitido por el controller.

---

# 5. Compra válida

No se puede registrar una devolución si la Compra:

- no existe;
- pertenece a otra Empresa fuera del alcance del Usuario;
- está anulada.

Estas condiciones se validan antes de mostrar el formulario y nuevamente al registrar.

---

# 6. Productos disponibles para devolver

Para cada DetalleCompra se calcula:

```text
CantidadDisponible = CantidadComprada - CantidadYaDevueltaActiva
```

Sólo se consideran devoluciones actualmente activas.

Una devolución anulada deja de consumir cantidad disponible.

---

# 7. Acumulación de devoluciones

Una misma Compra puede poseer múltiples DevolucionCompra.

Por eso, antes de permitir una nueva devolución, Veltika suma las cantidades ya devueltas por `DetalleCompraId`.

Ejemplo:

```text
Comprado: 10 unidades

Devolución 1: 3
Devolución 2: 2

Cantidad disponible para devolver: 5
```

---

# 8. Sin productos disponibles

Si todos los detalles de la Compra ya fueron devueltos en su totalidad mediante devoluciones activas, no se permite abrir un nuevo flujo de devolución.

El sistema informa:

```text
No quedan productos disponibles para devolver en esta compra.
```

---

# 9. Selección mínima

Para registrar una DevolucionCompra debe existir al menos un detalle con:

```text
CantidadDevolver > 0
```

No se permite crear una devolución vacía.

---

# 10. Validación de detalles

Dentro de la transacción se valida que:

- no existan `DetalleCompraId` duplicados en la solicitud;
- todos los detalles solicitados pertenezcan realmente a la Compra;
- la cantidad solicitada no supere la cantidad disponible.

Estas validaciones se realizan en backend.

---

# 11. Cantidad máxima por detalle

La cantidad a devolver debe cumplir:

```text
CantidadDevolver <= CantidadComprada - CantidadYaDevuelta
```

No se puede devolver más de lo comprado.

---

# 12. Stock físico disponible

Además de la cantidad histórica de la Compra, Veltika verifica el stock actual del Producto.

Debe cumplirse:

```text
CantidadDevolver <= Producto.Stock
```

Esto evita devolver físicamente al Proveedor unidades que la Empresa ya no posee en stock.

---

# 13. Motivo de la validación de stock

Ejemplo:

```text
Compra original: 10 unidades
Vendidas posteriormente: 8 unidades
Stock actual: 2 unidades
```

Aunque históricamente se compraron 10, no corresponde permitir una devolución de 5 porque sólo existen 2 unidades disponibles físicamente.

---

# 14. Detalle económico

Cada `DetalleDevolucionCompra` utiliza el PrecioUnitario histórico del `DetalleCompra` original.

Conceptualmente:

```text
Subtotal = CantidadDevuelta * PrecioUnitarioCompra
```

No se utiliza el precio actual del Producto.

---

# 15. Total de la devolución

El Total de DevolucionCompra se calcula sumando los subtotales de sus detalles:

```text
Total = Σ(Cantidad * PrecioUnitarioCompra)
```

Se calcula en servidor.

---

# 16. Impacto sobre stock

Registrar una DevolucionCompra reduce el stock actual.

Para cada producto:

```text
StockPosterior = StockAnterior - CantidadDevuelta
```

El Producto se actualiza dentro de la misma transacción.

---

# 17. MovimientoStock automático

Por cada detalle devuelto se genera un MovimientoStock con:

```text
Tipo = DevolucionCompra
Cantidad = CantidadDevuelta
StockAnterior
StockPosterior
EmpresaId
ProductoId
UsuarioId
Fecha
Motivo
```

Esto mantiene trazabilidad del egreso físico de mercadería.

---

# 18. Observaciones

La devolución puede registrar Observaciones opcionales.

Antes de persistir:

```text
vacío / espacios -> null
con contenido -> Trim()
```

El máximo del modelo es 500 caracteres.

---

# 19. Transacción

El registro utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- existencia de la Compra;
- Estado de la Compra;
- detalles solicitados;
- cantidades ya devueltas;
- cantidad disponible;
- stock actual.

Esto protege el proceso ante devoluciones concurrentes o cambios simultáneos de stock.

---

# 20. Atomicidad

La operación incluye dentro de la misma transacción:

```text
DevolucionCompra
DetalleDevolucionCompra
actualización Producto.Stock
MovimientoStock
```

Todo debe persistirse correctamente o revertirse.

---

# 21. Estado

DevolucionCompra utiliza actualmente:

```text
Estado = true  -> activa
Estado = false -> anulada
```

No se elimina físicamente.

---

# 22. Anulación

Una DevolucionCompra activa puede anularse mediante el flujo específico de Anular.

La anulación no elimina la devolución ni sus detalles.

Preserva el historial original y agrega información de anulación.

---

# 23. Datos de anulación

Al anular se establece:

```text
Estado = false
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
MotivoAnulacion = motivo
```

El Motivo posee máximo 500 caracteres.

---

# 24. Reposición de stock al anular

Anular una devolución significa que los productos vuelven a considerarse dentro de la Empresa.

Por cada detalle:

```text
StockPosterior = StockAnterior + CantidadDevuelta
```

Por lo tanto la anulación reincorpora stock.

---

# 25. MovimientoStock de anulación

La reposición genera nuevos MovimientoStock de tipo:

```text
AnulacionDevolucionCompra
```

con:

- ProductoId.
- EmpresaId.
- Cantidad.
- StockAnterior.
- StockPosterior.
- Motivo.
- Fecha.
- UsuarioId.

No se eliminan los movimientos originales.

---

# 26. Trazabilidad de stock

La secuencia correcta queda conceptualmente:

```text
Compra
    +10 stock

DevolucionCompra
    -3 stock

AnulacionDevolucionCompra
    +3 stock
```

Esto conserva la historia completa en lugar de modificar/eliminar movimientos anteriores.

---

# 27. Relación con PagoProveedor

Las devoluciones reducen el valor económico neto de la Compra.

Conceptualmente:

```text
TotalNetoCompra = Compra.Total - DevolucionesCompraActivas
```

con límite inferior en cero cuando las reglas financieras requieren normalización.

Este total neto participa en validaciones de pagos y reintegros del proveedor.

---

# 28. Relación con ReintegroProveedor

Si la Empresa ya pagó al Proveedor y luego devuelve mercadería, puede existir un importe que el Proveedor deba reintegrar.

Por eso DevolucionCompra y ReintegroProveedor están relacionados económicamente, aunque sean registros distintos:

```text
DevolucionCompra = corrección comercial/física
ReintegroProveedor = devolución efectiva de dinero
```

---

# 29. Protección al anular frente a reintegros

Antes de anular una DevolucionCompra se comprueba que los ReintegroProveedor activos continúen estando justificados.

Se calculan:

- total pagado activo;
- total reintegrado activo;
- total de devoluciones activas;
- devoluciones restantes luego de anular;
- nuevo total neto de Compra;
- máximo reintegrable resultante.

---

# 30. Total neto luego de anular

Se calcula conceptualmente:

```text
TotalDevolucionesLuegoDeAnular =
    TotalDevolucionesActivas - DevolucionActual.Total

TotalNetoLuegoDeAnular =
    max(0, Compra.Total - TotalDevolucionesLuegoDeAnular)
```

---

# 31. Máximo reintegrable luego de anular

Se calcula:

```text
MaximoReintegrableLuegoDeAnular =
    max(0, TotalPagado - TotalNetoLuegoDeAnular)
```

Si:

```text
TotalReintegrado > MaximoReintegrableLuegoDeAnular
```

la anulación se rechaza.

---

# 32. Motivo de esta regla

Ejemplo conceptual:

```text
Compra: $100
Pagado: $100
Devolución: $30
Reintegro proveedor: $30
```

Si se anulara la devolución sin anular previamente el reintegro, quedarían:

```text
Compra neta: $100
Pagado: $100
Reintegrado: $30
```

lo que dejaría el reintegro sin justificación económica.

Por eso el sistema obliga primero a corregir el reintegro correspondiente.

---

# 33. Anulación transaccional

La anulación también utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- que la devolución siga activa;
- pagos actuales;
- reintegros actuales;
- devoluciones actuales;
- Compra correspondiente;
- coherencia financiera resultante.

Luego se repone el stock y se generan movimientos de anulación.

---

# 34. No edición histórica

DevolucionCompra no funciona como CRUD editable.

No debe modificarse libremente:

- Compra.
- detalles.
- cantidades.
- precios históricos.
- Total.
- Fecha.
- Usuario.

Si la devolución fue incorrecta:

```text
Anular devolución
+
Registrar devolución correcta
```

---

# 35. Seguridad

Las reglas críticas se validan en backend:

- Empresa correcta.
- Compra existente y activa.
- detalles pertenecientes a la Compra.
- ausencia de detalles duplicados.
- cantidades históricamente disponibles.
- stock físico disponible.
- cálculo económico server-side.
- actualización de stock atómica.
- generación de MovimientoStock.
- coherencia con PagoProveedor.
- coherencia con ReintegroProveedor.
- revalidación mediante transacción Serializable.

---

# 36. Reglas de negocio actuales

1. DevolucionCompra pertenece a una Compra y Empresa.
2. Una Compra puede tener múltiples devoluciones.
3. Sólo pueden devolverse productos pertenecientes a la Compra.
4. Debe devolverse al menos un producto.
5. No se permiten detalles duplicados en una misma solicitud.
6. No puede devolverse más cantidad que la comprada menos devoluciones activas previas.
7. No puede devolverse más cantidad que el stock actual disponible.
8. El PrecioUnitario se toma del DetalleCompra histórico.
9. El Total se calcula en servidor.
10. Registrar la devolución reduce Producto.Stock.
11. Cada detalle genera MovimientoStock de tipo DevolucionCompra.
12. Registro completo utiliza transacción Serializable.
13. La devolución no se elimina físicamente.
14. La corrección se realiza mediante Anulación.
15. La anulación registra Fecha, Usuario y Motivo.
16. La anulación reincorpora stock.
17. La anulación genera MovimientoStock de tipo AnulacionDevolucionCompra.
18. Los movimientos históricos originales se conservan.
19. Devoluciones activas reducen el total neto de Compra.
20. La anulación no puede dejar ReintegroProveedor activos sin justificación.
21. La coherencia financiera se recalcula dentro de transacción Serializable.
22. AdminEmpresa sólo opera dentro de su Empresa.

---

# 37. Evolución futura

Posibles mejoras futuras:

- motivos de devolución tipificados;
- estado de recepción/aceptación por proveedor;
- devolución parcial pendiente de envío;
- generación de nota de devolución;
- relación con comprobante fiscal/nota de crédito;
- lotes y vencimientos;
- seriales;
- devoluciones por futura Sucursal/Depósito;
- permisos granulares para registrar/anular devoluciones.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 38. Estado actual

✅ Devoluciones parciales implementadas.

✅ Múltiples devoluciones por Compra implementadas.

✅ Control de cantidades previamente devueltas implementado.

✅ Control de stock físico disponible implementado.

✅ Reducción automática de stock implementada.

✅ MovimientoStock de DevolucionCompra implementado.

✅ Transacción Serializable implementada.

✅ Anulación lógica implementada.

✅ Reposición automática de stock al anular implementada.

✅ MovimientoStock de AnulacionDevolucionCompra implementado.

✅ Coherencia con PagoProveedor y ReintegroProveedor implementada.

✅ Trazabilidad de Usuario/Fecha/Motivo implementada.

🚧 Integración fiscal/nota de crédito pendiente.

🚧 Flujo logístico avanzado pendiente.

🚧 Permisos granulares pendientes.