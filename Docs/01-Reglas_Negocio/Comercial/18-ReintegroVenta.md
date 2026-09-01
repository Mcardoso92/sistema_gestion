# Módulo ReintegroVenta

Última actualización: 01/09/2026

---

# 1. Objetivo

ReintegroVenta representa la devolución de dinero al Cliente asociada a productos previamente vendidos.

A diferencia de una simple corrección financiera, el flujo actual también reincorpora al stock las unidades reintegradas.

Conceptualmente:

```text
Venta
    -> egresa stock

ReintegroVenta
    -> devuelve dinero al Cliente
    -> reincorpora stock
```

---

# 2. Modelo actual

`ReintegroVenta` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| VentaId | Venta asociada |
| EmpresaId | Empresa propietaria |
| CajaId | Caja desde la cual se devuelve el dinero |
| MedioPagoId | Medio utilizado para el reintegro |
| TurnoCajaId | Turno asociado cuando corresponde |
| UsuarioId | Usuario que registra |
| Fecha | Fecha/hora del reintegro |
| Importe | Importe reintegrado |
| Estado | Activo/Anulado |
| FechaAnulacion | Fecha/hora de anulación |
| UsuarioAnulacionId | Usuario que anuló |
| MotivoAnulacion | Motivo, máximo 500 caracteres |

Relaciones actuales:

- Venta.
- Empresa.
- Caja.
- MedioPago.
- TurnoCaja opcional.
- Usuario.
- MovimientoCaja.
- MovimientoStock.
- UsuarioAnulacion.
- DetalleReintegroVenta.

---

# 3. Autorización

`ReintegroVentaController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Además se aplican controles multiempresa dentro de cada acción.

---

# 4. Multiempresa

AdminEmpresa sólo puede operar reintegros pertenecientes a:

```text
usuario.EmpresaId
```

Venta, Caja, MedioPago, Turno y Productos deben pertenecer al mismo contexto empresarial.

---

# 5. Venta válida

No se puede registrar un ReintegroVenta si la Venta:

- no existe;
- pertenece a otra Empresa fuera del alcance del Usuario;
- está anulada.

Estas condiciones se validan antes de mostrar el formulario y al registrar.

---

# 6. Importe disponible para reintegrar

El importe máximo disponible se obtiene mediante:

```text
VentaSaldoService.ObtenerImporteDisponibleReintegro(venta.Id)
```

La lógica económica se mantiene centralizada en `VentaSaldoService`.

El sistema sólo permite reintegrar dinero que esté efectivamente respaldado por los cobros y la situación actual de la Venta.

---

# 7. Condición económica mínima

Debe cumplirse:

```text
ImporteDisponible > 0
```

Si no existe importe disponible, no se puede iniciar el reintegro.

---

# 8. Reintegro por productos

El Usuario no ingresa libremente un importe aislado.

Selecciona productos y cantidades de la Venta.

El sistema calcula el Importe en base a:

```text
PrecioUnitario histórico * CantidadReintegrar
```

---

# 9. Cantidades ya reintegradas

Para cada Producto de la Venta se suman los `DetalleReintegroVenta` pertenecientes a ReintegroVenta activos.

Conceptualmente:

```text
CantidadDisponible = CantidadVendida - CantidadYaReintegrada
```

---

# 10. Múltiples reintegros

Una misma Venta puede poseer varios ReintegroVenta parciales.

Ejemplo:

```text
Venta: 10 unidades

Reintegro 1: 2 unidades
Reintegro 2: 3 unidades

Disponibles para reintegrar: 5 unidades
```

---

# 11. Sin productos disponibles

Si todos los productos ya fueron reintegrados mediante reintegros activos, no puede iniciarse otro reintegro.

El sistema informa que todos los productos de la Venta ya fueron reintegrados.

---

# 12. Selección mínima

Debe existir al menos un producto con:

```text
CantidadReintegrar > 0
```

No se permite registrar un reintegro vacío.

---

# 13. Validación de pertenencia

Cada Producto solicitado debe pertenecer realmente a los detalles de la Venta.

El backend no confía en los identificadores enviados desde la vista.

---

# 14. Cantidad máxima por producto

Debe cumplirse:

```text
CantidadReintegrar <= CantidadVendida - CantidadYaReintegrada
```

No se puede reintegrar más cantidad que la originalmente vendida.

---

# 15. Precio histórico

El cálculo usa `DetalleVenta.PrecioUnitario`.

No utiliza el precio actual del Producto.

Esto conserva el valor económico real de la Venta original.

---

# 16. Importe del reintegro

Se calcula en servidor:

```text
ImporteReintegro = Σ(
    PrecioUnitarioVenta * CantidadReintegrar
)
```

Debe ser mayor a cero.

---

# 17. Límite económico

Además de las cantidades físicas debe cumplirse:

```text
ImporteReintegro <= ImporteDisponible
```

Por lo tanto una cantidad históricamente reintegrable no habilita por sí sola a devolver dinero si la situación financiera de la Venta no lo respalda.

---

# 18. Caja válida

La Caja seleccionada debe:

```text
pertenecer a venta.EmpresaId
Estado == true
```

---

# 19. MedioPago válido

El MedioPago debe estar asociado a la Caja mediante `CajaMedioPago`.

Se valida:

- Caja activa.
- MedioPago activo.
- misma Empresa.
- asociación Caja-MedioPago existente.

---

# 20. TurnoCaja

Si:

```text
Caja.PermiteTurnos == true
```

el Usuario debe tener un TurnoCaja propio abierto para la misma Caja.

Si la Caja no utiliza Turnos, `TurnoCajaId` puede quedar null.

---

# 21. Saldo disponible de Caja

Como ReintegroVenta devuelve dinero al Cliente, representa un Egreso.

Antes de registrar se calcula:

```text
CajaSaldoService.CalcularSaldoDisponible(...)
```

Debe cumplirse:

```text
ImporteReintegro <= SaldoDisponibleCaja
```

---

# 22. Transacción Serializable

El registro utiliza:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- importe disponible actual;
- cantidades ya reintegradas actuales;
- cantidades disponibles por Producto;
- saldo actual de Caja.

Esto protege frente a reintegros o movimientos concurrentes.

---

# 23. Registro del ReintegroVenta

Al persistir se establece:

```text
VentaId
EmpresaId
CajaId
MedioPagoId
TurnoCajaId
UsuarioId
Fecha = DateTime.Now
Importe calculado
Estado = Activo
```

Los datos de anulación comienzan en null.

---

# 24. DetalleReintegroVenta

Por cada Producto reintegrado se registra un detalle con:

```text
ProductoId
Cantidad
PrecioUnitario histórico
Subtotal
ReintegroVentaId
```

---

# 25. Reincorporación de stock

Registrar un ReintegroVenta incrementa el stock:

```text
StockPosterior = StockAnterior + CantidadReintegrada
```

Esto representa el retorno de los productos a la Empresa.

---

# 26. MovimientoStock automático

Por cada detalle se genera:

```text
Tipo = ReintegroVenta
Cantidad = detalle.Cantidad
StockAnterior
StockPosterior
VentaId
ReintegroVentaId
```

El movimiento conserva Usuario, Empresa, Fecha y Motivo.

---

# 27. MovimientoCaja automático

El reintegro genera:

```text
Tipo = ReintegroVenta
Direccion = Egreso
Importe = importeReintegro
CajaId
MedioPagoId
TurnoCajaId
ReintegroVentaId
```

El Concepto es equivalente a:

```text
Reintegro de venta #<VentaId>
```

---

# 28. Atomicidad

Dentro de la misma transacción se persisten:

```text
ReintegroVenta
DetalleReintegroVenta
actualización Producto.Stock
MovimientoStock
MovimientoCaja
```

Todo debe completarse correctamente o revertirse.

---

# 29. Estado

ReintegroVenta utiliza `EstadoReintegro`.

Estados actuales:

```text
Activo
Anulado
```

No existe eliminación física como corrección normal.

---

# 30. Anulación

Un reintegro incorrecto se corrige mediante Anular.

No se modifica libremente el registro histórico.

---

# 31. MovimientoCaja original requerido

La anulación requiere encontrar el MovimientoCaja original:

```text
ReintegroVentaId == reintegro.Id
Tipo == ReintegroVenta
```

Si no existe, se rechaza la operación.

---

# 32. Prevención de doble reversión

Se verifica que no exista ya un MovimientoCaja cuyo:

```text
MovimientoOrigenId == movimientoOriginal.Id
```

Si existe, el reintegro ya fue financieramente revertido y no puede repetirse.

---

# 33. Stock requerido para anular

Registrar el reintegro reincorporó productos al stock.

Anularlo debe volver a retirar esas unidades.

Por eso, para cada detalle debe cumplirse:

```text
Producto.Stock >= CantidadReintegrada
```

Si el stock actual es insuficiente, la anulación se bloquea.

---

# 34. Motivo del control de stock

Ejemplo:

```text
Reintegro original: +2 unidades al stock

Luego esas 2 unidades son vendidas nuevamente.
Stock actual: 0
```

No se puede anular directamente el reintegro porque implicaría descontar 2 unidades inexistentes y dejar stock negativo.

---

# 35. Revalidación de stock

El stock se valida antes de abrir la transacción y nuevamente dentro de la transacción Serializable.

Esto evita inconsistencias por cambios concurrentes.

---

# 36. Caja original para anular

La Caja asociada al movimiento original debe continuar:

```text
existiendo
activa
perteneciendo a la Empresa
```

---

# 37. Turno al anular

Si la Caja utiliza Turnos, el Usuario que anula debe tener un TurnoCaja propio abierto para esa misma Caja.

La reversión se asocia al Turno operativo actual.

---

# 38. Datos de anulación

Al anular:

```text
Estado = Anulado
FechaAnulacion = DateTime.Now
UsuarioAnulacionId = usuario.Id
MotivoAnulacion = motivo
```

---

# 39. Stock al anular

Por cada detalle:

```text
StockPosterior = StockAnterior - CantidadReintegrada
```

Es decir, se revierte la reincorporación de stock realizada por el reintegro original.

---

# 40. MovimientoStock de anulación

Por cada detalle se genera:

```text
Tipo = AnulacionReintegroVenta
Cantidad
StockAnterior
StockPosterior
VentaId
ReintegroVentaId
```

El movimiento original se conserva.

---

# 41. Reversión financiera

El ReintegroVenta original fue:

```text
Direccion = Egreso
```

Su anulación genera:

```text
Tipo = ReversionReintegroVenta
Direccion = Ingreso
```

Esto reincorpora a Caja el dinero que se había devuelto al Cliente.

---

# 42. MovimientoOrigenId

La reversión financiera referencia explícitamente al movimiento original:

```text
MovimientoOrigenId = movimientoCaja.Id
```

Esto mantiene trazabilidad completa.

---

# 43. Secuencia completa

Conceptualmente:

```text
Venta
    - stock

CobroVenta
    + dinero Caja

ReintegroVenta
    + stock
    - dinero Caja

AnulacionReintegroVenta
    - stock

ReversionReintegroVenta
    + dinero Caja
```

---

# 44. Relación con CobroVenta

El dinero disponible para reintegrar depende de la situación financiera real de la Venta.

Por eso `VentaSaldoService` mantiene la coherencia entre:

- Venta.
- CobroVenta activos.
- ReintegroVenta activos.

No se puede devolver al Cliente más dinero del que corresponda según esos movimientos.

---

# 45. No edición histórica

ReintegroVenta no funciona como CRUD tradicional.

No debe modificarse libremente:

- Venta.
- Productos.
- Cantidades.
- PrecioUnitario.
- Importe.
- Caja.
- MedioPago.
- Fecha.
- Usuario.

Si fue incorrecto:

```text
Anular reintegro
+
Registrar reintegro correcto
```

---

# 46. Seguridad

Las reglas críticas se validan en backend:

- Empresa correcta.
- Venta activa.
- importe disponible real.
- Producto perteneciente a Venta.
- cantidad disponible por Producto.
- precios históricos server-side.
- cálculo del importe server-side.
- Caja activa.
- MedioPago válido para Caja.
- Turno propio cuando corresponde.
- saldo suficiente de Caja.
- revalidaciones dentro de transacción Serializable.
- MovimientoCaja original requerido al anular.
- prevención de doble reversión.
- stock suficiente para anular.

---

# 47. Reglas de negocio actuales

1. ReintegroVenta pertenece a una Venta y Empresa.
2. Sólo puede registrarse sobre Venta activa.
3. Debe existir importe financiero disponible para reintegrar.
4. El reintegro se basa en productos y cantidades de la Venta.
5. Se permiten reintegros parciales.
6. No puede reintegrarse más cantidad que la vendida menos reintegros activos previos.
7. Sólo pueden seleccionarse Productos de la Venta.
8. El PrecioUnitario se toma del DetalleVenta histórico.
9. El Importe se calcula en servidor.
10. El Importe no puede superar el disponible financiero.
11. Caja y MedioPago deben ser válidos para la Empresa.
12. MedioPago debe estar asociado a Caja.
13. Si Caja utiliza Turnos, se requiere Turno propio abierto.
14. Caja debe tener saldo suficiente para registrar el reintegro.
15. Registro utiliza transacción Serializable.
16. ReintegroVenta reincorpora stock.
17. Cada detalle genera MovimientoStock de tipo ReintegroVenta.
18. ReintegroVenta genera MovimientoCaja de Egreso.
19. La corrección se realiza mediante Anulación.
20. Anular requiere MovimientoCaja original.
21. No se permite doble reversión financiera.
22. Anular exige stock suficiente para retirar las unidades previamente reincorporadas.
23. La anulación genera MovimientoStock de tipo AnulacionReintegroVenta.
24. La anulación genera MovimientoCaja de tipo ReversionReintegroVenta.
25. La reversión financiera tiene Dirección Ingreso.
26. La reversión referencia al MovimientoCaja original.
27. Los registros históricos se conservan.
28. AdminEmpresa sólo opera dentro de su Empresa.

---

# 48. Evolución futura

Posibles mejoras futuras:

- motivos de devolución/reintegro tipificados;
- estado físico de producto devuelto;
- productos no reintegrables a stock;
- devoluciones sin reposición de stock por rotura/merma;
- nota de crédito fiscal;
- comprobante digital de reintegro;
- integración con Mercado Pago u otros proveedores;
- devoluciones con acreditación diferida;
- permisos granulares para registrar/anular reintegros.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 49. Estado actual

✅ Reintegros parciales por producto implementados.

✅ Control de cantidades ya reintegradas implementado.

✅ Importe disponible centralizado en VentaSaldoService implementado.

✅ Precio histórico de Venta implementado.

✅ Reincorporación automática de stock implementada.

✅ MovimientoStock de ReintegroVenta implementado.

✅ MovimientoCaja de Egreso implementado.

✅ Validación de saldo de Caja implementada.

✅ Integración con TurnoCaja implementada.

✅ Transacción Serializable implementada.

✅ Anulación lógica implementada.

✅ Control de stock para anular implementado.

✅ MovimientoStock de AnulacionReintegroVenta implementado.

✅ MovimientoCaja de ReversionReintegroVenta como Ingreso implementado.

✅ Prevención de doble reversión implementada.

✅ Trazabilidad financiera y de stock implementada.

🚧 Nota de crédito/facturación electrónica pendiente.

🚧 Tratamiento avanzado de mercadería dañada/no reintegrable pendiente.

🚧 Permisos granulares pendientes.