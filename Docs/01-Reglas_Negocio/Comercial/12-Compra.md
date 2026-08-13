# Módulo Compra

---

# 1. Objetivo

El módulo Compra permite registrar el ingreso de mercadería adquirida a proveedores dentro de Veltika.

Cada compra representa una operación comercial entre la empresa y un proveedor, registrando los productos adquiridos, cantidades, costos y demás información necesaria para actualizar el stock y mantener el historial de abastecimiento.

Este módulo constituye la base del proceso de reposición de mercadería.

---

# 2. Alcance

El módulo permite registrar, consultar y anular compras realizadas por una empresa.

Cada compra genera automáticamente el ingreso de stock de los productos involucrados y mantiene el historial de abastecimiento con cada proveedor.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Responsable de Compras

---

# 4. Permisos

## Super Administrador

✅ Consultar todas las compras

✅ Anular compras

## Administrador de Empresa

✅ Registrar compras

✅ Consultar compras

✅ Anular compras

## Responsable de Compras

✅ Registrar compras

✅ Consultar compras

❌ Anular compras (configurable)

---

# 5. Funcionalidades

Actualmente

- Registrar compra
- Consultar compras
- Buscar compras
- Filtrar compras
- Visualizar detalle
- Anular compra

Versiones futuras

- Ordenes de compra
- Recepción parcial
- Compras pendientes
- Costos adicionales
- Gastos de importación
- Compras internacionales
- Integración con facturación
- Importación desde Excel

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal de destino |
| ProveedorId | Proveedor asociado |
| UsuarioId | Usuario que registra la compra |
| FechaCompra | Fecha y hora |
| Total | Importe total |
| Estado | Activa o Anulada |

Campos futuros

- Número de factura
- Tipo de comprobante
- Condición de pago
- Fecha de vencimiento
- Descuento
- Impuestos
- Observaciones
- Moneda
- Cotización

---

# 7. Validaciones

- Debe existir un proveedor.
- Debe existir al menos un producto.
- El usuario debe estar activo.
- La empresa debe existir.
- La sucursal debe existir.
- El total debe ser mayor a cero.
- No puede registrarse una compra sin detalle.

---

# 8. Reglas de negocio

- Cada compra pertenece exclusivamente a una empresa.
- Cada compra pertenece a una sucursal.
- Toda compra debe estar asociada a un proveedor.
- Cada compra posee uno o varios productos.
- Al confirmar la compra se incrementará automáticamente el stock.
- Una compra anulada revertirá el ingreso de stock.
- Las compras nunca podrán eliminarse físicamente.

---

# 9. Casos de uso

## Registrar compra

El usuario registra una nueva compra de mercadería.

Resultado esperado:

- Compra registrada correctamente.
- Stock actualizado.
- Compra disponible para consultas.

---

## Consultar compra

Permite visualizar una compra registrada.

---

## Buscar compra

Permite localizar compras mediante distintos filtros.

---

## Anular compra

Permite invalidar una compra registrada.

El movimiento permanecerá almacenado para fines históricos.

---

# 10. Casos de error

- Compra sin productos.
- Producto inexistente.
- Proveedor inexistente.
- Usuario sin permisos.
- Compra inexistente.
- Error al actualizar el stock.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Compras.
2. Selecciona el proveedor.
3. Agrega los productos.
4. Ingresa cantidades.
5. Ingresa el costo de cada producto.
6. El sistema calcula el total.
7. El usuario confirma la compra.
8. Se registra la compra.
9. Se incrementa el stock.
10. La compra queda disponible para futuras consultas.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Sucursal
- Usuario
- Proveedor
- DetalleCompra
- Producto
- Stock
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Ordenes de compra.
- Recepciones parciales.
- Compras automáticas.
- Integración con proveedores.
- Facturación electrónica.
- Importación masiva.
- Costos logísticos.
- Compras internacionales.

---

# 14. Roadmap

Versión 1.0

- Registro
- Consulta
- Anulación
- Actualización de stock

Versión 2.0

- Ordenes de compra
- Compras pendientes
- Facturación
- Importación

Versión 3.0

- Automatización
- Integraciones
- Costos avanzados
- API de compras

---

# 15. Decisiones de Arquitectura

## Costo histórico

Cada detalle de compra almacenará el costo utilizado al momento de registrar la compra.

Las modificaciones posteriores del costo del producto no alterarán las compras históricas.

---

## Actualización del costo del producto

Al confirmar una compra, el sistema actualizará automáticamente el costo actual del producto utilizando el costo ingresado en la compra.

Esta funcionalidad permitirá mantener actualizado el costo de reposición de cada producto.

---

## Incremento automático del stock

Al confirmar la compra, el stock aumentará automáticamente según las cantidades registradas.

No existirá una operación manual para incrementar stock.

---

## Compra inmutable

Una compra confirmada no podrá editarse.

Si existe un error, deberá anularse y registrarse nuevamente.

---

## Integridad histórica

Las compras conservarán permanentemente la información original utilizada al momento de su registro.

Los cambios posteriores en proveedores, productos o costos no modificarán el historial.

---

## Eliminación lógica

Las compras nunca podrán eliminarse físicamente.

Únicamente podrán anularse para preservar la trazabilidad del sistema.

---

## Origen del stock

Todo ingreso de mercadería deberá provenir de una compra, una devolución de cliente o una transferencia entre sucursales (funcionalidad futura).

No existirá un incremento manual de stock, salvo mediante un módulo específico de Ajuste de Stock que será implementado en versiones futuras y quedará completamente auditado.