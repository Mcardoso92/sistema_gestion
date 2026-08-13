# Módulo Venta

---

# 1. Objetivo

El módulo Venta permite registrar las operaciones comerciales realizadas por una empresa dentro de Veltika.

Cada venta representa una transacción entre la empresa y un cliente, registrando los productos vendidos, cantidades, precios, descuentos, formas de pago y demás información necesaria para el funcionamiento del sistema.

Este módulo constituye el proceso principal de Veltika.

---

# 2. Alcance

El módulo permite crear, consultar, anular y visualizar ventas realizadas por una empresa.

Cada venta genera automáticamente los movimientos correspondientes sobre el stock, la caja y los reportes del sistema.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Cajero
- Vendedor

---

# 4. Permisos

## Super Administrador

✅ Visualizar todas las ventas

✅ Consultar cualquier venta

✅ Anular ventas

## Administrador de Empresa

✅ Registrar ventas

✅ Consultar ventas

✅ Anular ventas

✅ Reimprimir comprobantes

## Cajero

✅ Registrar ventas

✅ Consultar ventas propias

✅ Reimprimir comprobantes

❌ Anular ventas (configurable)

## Vendedor

✅ Registrar ventas

✅ Consultar ventas propias

❌ Anular ventas

---

# 5. Funcionalidades

Actualmente

- Registrar venta
- Consultar ventas
- Buscar ventas
- Filtrar ventas
- Visualizar detalle
- Reimprimir comprobante
- Anular venta

Versiones futuras

- Facturación electrónica
- Presupuestos
- Reservas
- Notas de crédito
- Notas de débito
- Descuentos automáticos
- Promociones
- Ventas en cuotas
- Integración con Mercado Pago
- Integración con POS
- Integración con billeteras virtuales

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal donde se realizó |
| ClienteId | Cliente asociado |
| UsuarioId | Usuario que realizó la venta |
| FechaVenta | Fecha y hora |
| Total | Importe total |
| Estado | Activa o Anulada |

Campos futuros

- Número de comprobante
- Tipo de comprobante
- Observaciones
- Descuento
- Recargo
- Impuestos
- Forma de pago
- Estado de pago
- Moneda
- Cotización
- CajaId
- TurnoCajaId

---

# 7. Validaciones

- Debe existir al menos un producto.
- El usuario debe estar activo.
- La empresa debe existir.
- La sucursal debe existir.
- No pueden venderse productos desactivados.
- No puede registrarse una venta sin detalle.
- El total debe ser mayor a cero.

---

# 8. Reglas de negocio

- Cada venta pertenece exclusivamente a una empresa.
- Cada venta pertenece a una sucursal.
- Cada venta puede tener un cliente.
- Se permitirá vender como Consumidor Final.
- Cada venta posee uno o varios productos.
- Al confirmar la venta se descontará automáticamente el stock.
- La venta generará un movimiento de caja.
- Una venta anulada restaurará el stock (según configuración futura).
- Las ventas nunca podrán eliminarse físicamente.

---

# 9. Casos de uso

## Registrar venta

El usuario selecciona los productos, confirma la operación y registra la venta.

Resultado esperado:

- Venta registrada correctamente.
- Stock actualizado.
- Caja actualizada.
- Venta disponible para consultas.

---

## Consultar venta

Permite visualizar una venta registrada.

---

## Buscar venta

Permite localizar ventas mediante distintos filtros.

---

## Anular venta

Permite invalidar una venta ya registrada.

La operación permanecerá registrada para auditoría.

---

## Reimprimir comprobante

Permite emitir nuevamente el comprobante de una venta.

---

# 10. Casos de error

- Venta sin productos.
- Producto inexistente.
- Producto sin stock.
- Usuario sin permisos.
- Cliente inexistente.
- Caja cerrada.
- Venta inexistente.
- Error al actualizar stock.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Ventas.
2. Selecciona los productos.
3. Ingresa cantidades.
4. Selecciona el cliente (opcional).
5. Elige la forma de pago.
6. El sistema calcula el total.
7. El usuario confirma la operación.
8. Se registra la venta.
9. Se descuenta el stock.
10. Se registra el movimiento de caja.
11. Se genera el comprobante.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Sucursal
- Usuario
- Cliente
- Producto
- DetalleVenta
- Stock
- Caja
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Facturación electrónica.
- Presupuestos.
- Promociones.
- Descuentos automáticos.
- Ventas en cuotas.
- Integración con Mercado Pago.
- Integración con billeteras virtuales.
- Integración con impresoras fiscales.
- Firma digital.
- API de ventas.

---

# 14. Roadmap

Versión 1.0

- Registro
- Consulta
- Anulación
- Reimpresión
- Control de stock

Versión 2.0

- Facturación
- Promociones
- Descuentos
- Formas de pago avanzadas

Versión 3.0

- POS
- Mercado Pago
- Billeteras virtuales
- API pública
- Integraciones comerciales