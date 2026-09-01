# Módulo Ajuste de Stock

Última actualización: 01/09/2026

---

# 1. Objetivo

El ajuste de stock permite corregir manualmente la existencia de un Producto cuando la cantidad registrada en Veltika no coincide con la situación real.

Actualmente no existe una entidad `AjusteStock` independiente.

El ajuste se implementa como una operación del módulo `MovimientoStock` que modifica `Producto.Stock` y genera simultáneamente un `MovimientoStock` de tipo `AjusteEntrada` o `AjusteSalida`.

---

# 2. Arquitectura actual

El flujo utiliza:

```text
MovimientoStockController.Ajustar
StockAjusteVM
Producto.Stock
MovimientoStock
```

No existe actualmente:

```text
AjusteStock.cs
AjusteStockController
AjusteStockId
```

Por lo tanto, el ajuste no se persiste como un documento separado del movimiento generado.

El propio `MovimientoStock` constituye el registro histórico del ajuste.

---

# 3. Acceso y permisos

Las acciones GET y POST de `Ajustar` poseen actualmente:

```text
[Authorize(Roles = "AdminEmpresa")]
```

Por lo tanto, el único rol habilitado actualmente para registrar ajustes desde este flujo es:

- `AdminEmpresa`.

Aunque `MovimientoStockController` a nivel general admite `SuperAdmin,AdminEmpresa`, la autorización específica de `Ajustar` restringe esta acción a `AdminEmpresa`.

Actualmente no existen permisos de ajuste para:

- SuperAdmin.
- Responsable de Depósito.
- Cajero.
- Vendedor.
- Auditor.

---

# 4. Alcance actual

Actualmente se puede:

- Seleccionar un Producto desde el listado de stock.
- Registrar una entrada manual.
- Registrar una salida manual.
- Indicar cantidad.
- Indicar un motivo obligatorio.
- Actualizar el stock actual.
- Generar automáticamente el MovimientoStock correspondiente.
- Consultar posteriormente el ajuste mediante el historial de movimientos.

Actualmente no existe un listado independiente de documentos `AjusteStock` porque no existe dicha entidad.

---

# 5. ViewModel actual

`StockAjusteVM` contiene:

| Campo | Uso |
|---|---|
| ProductoId | Producto que se ajustará |
| ProductoNombre | Información de presentación |
| CodigoBarra | Información de presentación |
| StockActual | Stock mostrado al usuario |
| Tipo | Entrada o Salida |
| Cantidad | Cantidad a ajustar |
| Motivo | Justificación obligatoria |

No posee actualmente:

- EmpresaId enviado como dato editable.
- SucursalId.
- Observaciones separadas del Motivo.
- Estado.
- Aprobador.
- Archivo adjunto.
- InventarioId.

---

# 6. Tipos de ajuste

El enum `TipoAjusteStockVM` contiene actualmente:

```text
Entrada = 1
Salida = 2
```

El controller traduce estos valores a tipos históricos de `MovimientoStock`:

```text
Entrada -> TipoMovimientoStock.AjusteEntrada
Salida  -> TipoMovimientoStock.AjusteSalida
```

---

# 7. Validaciones generales

Para registrar un ajuste:

- El usuario debe estar autenticado.
- Debe poseer rol `AdminEmpresa`.
- El Producto debe existir.
- El Producto debe pertenecer a la empresa del usuario.
- El Producto debe estar activo.
- Debe seleccionarse un tipo válido.
- La cantidad debe ser mayor a cero.
- El motivo es obligatorio.
- El motivo no puede superar 250 caracteres.

---

# 8. Seguridad multiempresa

El `ProductoId` recibido no se utiliza sin validación.

El controller busca el Producto mediante:

```text
p.Id == ajusteVM.ProductoId
&&
p.EmpresaId == usuario.EmpresaId
```

Por lo tanto, un `AdminEmpresa` no puede ajustar mediante este flujo un Producto perteneciente a otro tenant manipulando el identificador enviado.

No se confía en un `EmpresaId` proveniente del formulario.

---

# 9. Producto activo

No puede ajustarse manualmente un Producto inactivo.

La regla se valida tanto al abrir la pantalla como al procesar el POST.

Si el Producto está inactivo, el ajuste se rechaza.

La reactivación del Producto debe resolverse desde su módulo correspondiente antes de realizar nuevas operaciones administrativas sobre él.

---

# 10. Cantidad

La cantidad posee la validación:

```text
Cantidad >= 1
```

No se utilizan cantidades negativas para representar salidas.

La dirección del ajuste se determina mediante `Tipo`.

Ejemplo:

```text
Tipo = Salida
Cantidad = 5
```

representa retirar cinco unidades.

---

# 11. Motivo

El motivo es obligatorio:

```text
[Required]
[StringLength(250)]
```

Al persistir el movimiento se almacena:

```text
Motivo = ajusteVM.Motivo.Trim()
```

Actualmente el motivo es texto libre.

No existe un catálogo persistido de motivos como:

- Rotura.
- Robo.
- Vencimiento.
- Diferencia de inventario.
- Consumo interno.

Esos valores pueden utilizarse conceptualmente, pero el sistema actual no obliga a seleccionar uno de una lista cerrada.

---

# 12. Ajuste de entrada

Cuando:

```text
Tipo = Entrada
```

el sistema calcula:

```text
StockPosterior = StockAnterior + Cantidad
```

Y genera:

```text
TipoMovimientoStock.AjusteEntrada
```

La entrada incrementa `Producto.Stock`.

---

# 13. Ajuste de salida

Cuando:

```text
Tipo = Salida
```

se valida primero:

```text
Cantidad <= StockAnterior
```

Si la cantidad supera el stock disponible, el ajuste se rechaza.

Si es válida:

```text
StockPosterior = StockAnterior - Cantidad
```

Y genera:

```text
TipoMovimientoStock.AjusteSalida
```

---

# 14. Stock negativo

El ajuste manual no permite dejar el stock por debajo de cero.

No existe actualmente una configuración empresarial que permita desactivar esta regla.

Por lo tanto:

```text
CantidadSalida > StockActual
```

siempre es inválido en el flujo actual.

---

# 15. Persistencia

Al confirmar un ajuste válido se realizan dos cambios coordinados:

```text
Producto.Stock = StockPosterior
```

Y se crea:

```text
MovimientoStock
```

con:

- ProductoId.
- EmpresaId.
- Tipo.
- Cantidad.
- StockAnterior.
- StockPosterior.
- Motivo.
- Fecha.
- UsuarioId.

---

# 16. Transacción

El cambio de stock y la creación del movimiento se ejecutan dentro de una transacción de base de datos.

El flujo realiza conceptualmente:

```text
BeginTransaction
Actualizar Producto.Stock
Agregar MovimientoStock
SaveChanges
Commit
```

Si ocurre un error:

```text
Rollback
```

Y el stock manejado en el contexto se restaura al valor anterior para volver a presentar correctamente la vista.

El objetivo es evitar inconsistencias como:

- Stock modificado sin movimiento.
- Movimiento creado sin actualización de stock.

---

# 17. Fecha y usuario

La fecha se genera en servidor:

```text
DateTime.Now
```

El usuario se obtiene desde el usuario autenticado:

```text
UsuarioId = usuario.Id
```

Estos datos no dependen de valores editables enviados por el formulario.

---

# 18. Historial

No existe una tabla independiente de ajustes para consultar.

Los ajustes quedan representados en `MovimientoStock` mediante:

```text
AjusteEntrada
AjusteSalida
```

Por lo tanto, pueden consultarse desde el historial general filtrando por esos tipos.

El historial conserva:

- Fecha.
- Producto.
- Empresa.
- Usuario.
- Tipo.
- Cantidad.
- Stock anterior.
- Stock posterior.
- Motivo.

---

# 19. Inmutabilidad

Actualmente no existen acciones para editar o eliminar un MovimientoStock generado por un ajuste.

Si posteriormente debe corregirse un ajuste equivocado, debe registrarse un nuevo ajuste compensatorio.

Ejemplo:

```text
AjusteEntrada +5
```

realizado por error puede corregirse mediante:

```text
AjusteSalida 5
```

siempre que exista stock suficiente.

De esta forma ambos eventos permanecen auditables.

---

# 20. Diferencia respecto de operaciones comerciales

El ajuste manual debe utilizarse para correcciones de inventario que no correspondan a una operación comercial normal.

No debe utilizarse para reemplazar flujos existentes como:

- Compra.
- Venta.
- Anulación de Compra.
- Anulación de Venta.
- Reintegro de Venta.
- Devolución de Compra.

Cada uno de esos procesos posee sus propios movimientos y reglas de negocio.

---

# 21. Sucursales y depósitos

Actualmente el ajuste no trabaja con Sucursal ni Depósito.

El stock pertenece al Producto dentro de su Empresa.

No existen campos como:

```text
SucursalId
DepositoId
```

en `StockAjusteVM` ni en `MovimientoStock`.

La futura incorporación de existencias por ubicación requerirá rediseñar también el flujo de ajustes para indicar qué ubicación se está corrigiendo.

---

# 22. Reglas de negocio

1. Actualmente no existe una entidad `AjusteStock` independiente.
2. El ajuste se realiza desde `MovimientoStockController`.
3. Sólo `AdminEmpresa` puede ejecutar actualmente la acción Ajustar.
4. El Producto debe pertenecer a la empresa del usuario.
5. El Producto debe estar activo.
6. El tipo debe ser Entrada o Salida.
7. La cantidad debe ser mayor a cero.
8. El motivo es obligatorio.
9. El motivo admite hasta 250 caracteres.
10. Una Entrada incrementa el stock.
11. Una Salida disminuye el stock.
12. Una Salida no puede superar el stock disponible.
13. No se permite stock negativo.
14. El stock anterior se obtiene desde el Producto en servidor.
15. El stock posterior se calcula en servidor.
16. Fecha y usuario se determinan en servidor.
17. Cada ajuste genera un MovimientoStock.
18. El MovimientoStock constituye el historial persistido del ajuste.
19. No existe edición administrativa del movimiento confirmado.
20. No existe eliminación administrativa del movimiento confirmado.
21. Las correcciones posteriores deben generar nuevos movimientos compensatorios.
22. Actualmente no existe ajuste por Sucursal o Depósito.

---

# 23. Casos de error relevantes

- Usuario no autenticado.
- Usuario sin rol `AdminEmpresa`.
- Producto inexistente.
- Producto perteneciente a otra empresa.
- Producto inactivo.
- Tipo de ajuste inválido.
- Cantidad menor a uno.
- Motivo vacío.
- Motivo superior a 250 caracteres.
- Salida superior al stock disponible.
- Error de persistencia durante la transacción.

---

# 24. Integraciones actuales

El ajuste se integra con:

- Producto.
- Empresa.
- Usuario autenticado.
- MovimientoStock.
- Historial de Stock.

No existe actualmente integración con:

- Sucursal.
- Depósito.
- Documento de inventario físico.
- Flujo de aprobación.

---

# 25. Capacidades no implementadas

Actualmente no existen:

- Ajustes masivos.
- Importación específica de ajustes desde Excel.
- Ajustes por Sucursal.
- Ajustes por Depósito.
- Flujo de aprobación.
- Usuario aprobador.
- Fecha de aprobación.
- Firma digital.
- Evidencia fotográfica.
- Archivos adjuntos.
- Inventario físico formal.
- Conteos cíclicos.
- Ajustes programados.
- Catálogo cerrado de motivos.
- Observaciones separadas del Motivo.

---

# 26. Evolución futura

La evolución se administra mediante Roadmap y GitHub Issues.

Entre las capacidades posibles se encuentran:

- Inventario físico.
- Conteos cíclicos.
- Ajustes masivos.
- Ajustes derivados de diferencias de inventario.
- Aprobaciones para ajustes sensibles.
- Evidencia o documentación adjunta.
- Ajustes por futuras sucursales/depósitos.
- Permisos granulares para empleados.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 27. Estado

✅ Ajuste manual implementado.

✅ Entrada de stock implementada.

✅ Salida de stock implementada.

✅ Motivo obligatorio implementado.

✅ Validación de stock suficiente implementada.

✅ Protección multiempresa implementada.

✅ Restricción a productos activos implementada.

✅ MovimientoStock automático implementado.

✅ Transacción para stock + movimiento implementada.

✅ Historial mediante MovimientoStock implementado.

🚧 Ajustes masivos, inventario físico, aprobaciones y ajustes por ubicación quedan para evolución futura.