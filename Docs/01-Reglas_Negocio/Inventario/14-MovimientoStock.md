# Módulo Movimiento de Stock

---

# 1. Objetivo

El módulo Movimiento de Stock registra todos los cambios producidos sobre el inventario de los productos dentro de Veltika.

Cada modificación del stock genera un movimiento que indica el motivo, el usuario responsable, la fecha y la cantidad afectada.

Este módulo constituye la base de la trazabilidad del inventario y garantiza que toda variación del stock pueda ser auditada.

---

# 2. Alcance

El módulo permite registrar y consultar todos los movimientos de stock generados automáticamente por el sistema.

Los movimientos nunca serán creados manualmente, salvo aquellos correspondientes a Ajustes de Stock autorizados.

Cada movimiento representa una modificación puntual sobre un producto.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Responsable de Depósito
- Auditor

---

# 4. Permisos

## Super Administrador

✅ Consultar todos los movimientos

✅ Registrar ajustes de stock

## Administrador de Empresa

✅ Consultar movimientos

✅ Registrar ajustes de stock

## Responsable de Depósito

✅ Consultar movimientos

✅ Registrar ajustes (según permisos)

## Auditor

✅ Consultar movimientos

❌ No puede modificar información

---

# 5. Funcionalidades

Actualmente

- Registrar movimientos automáticos
- Consultar movimientos
- Buscar movimientos
- Filtrar movimientos

Versiones futuras

- Ajustes masivos
- Transferencias entre sucursales
- Inventarios físicos
- Conteos cíclicos
- Alertas automáticas
- Exportación
- Dashboard de movimientos

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal afectada |
| ProductoId | Producto involucrado |
| UsuarioId | Usuario responsable |
| TipoMovimiento | Motivo del movimiento |
| Cantidad | Cantidad modificada |
| StockAnterior | Stock antes del movimiento |
| StockNuevo | Stock luego del movimiento |
| FechaMovimiento | Fecha y hora |
| Observaciones | Motivo adicional |

Campos futuros

- CompraId
- VentaId
- AjusteId
- TransferenciaId
- Lote
- NumeroSerie
- DocumentoReferencia
- IPUsuario

---

# 7. Validaciones

- Debe existir un producto.
- Debe existir un usuario.
- La cantidad no puede ser cero.
- El tipo de movimiento es obligatorio.
- El stock resultante no podrá ser negativo (según configuración).
- Todo movimiento debe tener un origen.

---

# 8. Reglas de negocio

- Todo cambio de stock genera un movimiento.
- Los movimientos nunca podrán eliminarse.
- Los movimientos nunca podrán modificarse.
- El sistema registrará automáticamente la fecha y el usuario.
- Todo movimiento deberá indicar su origen.
- El historial de movimientos será permanente.

---

# 9. Casos de uso

## Registrar movimiento por compra

Al confirmar una compra, el sistema genera automáticamente un movimiento de ingreso.

Resultado esperado:

- Stock actualizado.
- Movimiento registrado.

---

## Registrar movimiento por venta

Al confirmar una venta, el sistema genera automáticamente un movimiento de egreso.

Resultado esperado:

- Stock actualizado.
- Movimiento registrado.

---

## Registrar ajuste de stock

Un usuario autorizado realiza una corrección del inventario.

Resultado esperado:

- Stock actualizado.
- Motivo registrado.
- Auditoría completa.

---

## Consultar movimientos

Permite visualizar el historial completo de movimientos de un producto.

---

# 10. Casos de error

- Producto inexistente.
- Usuario inexistente.
- Movimiento sin origen.
- Stock insuficiente.
- Usuario sin permisos.
- Tipo de movimiento inválido.

---

# 11. Flujo funcional

1. Se produce una operación (compra, venta o ajuste).
2. El sistema identifica el producto afectado.
3. Obtiene el stock actual.
4. Calcula el nuevo stock.
5. Actualiza el inventario.
6. Registra el movimiento.
7. Guarda el historial para futuras auditorías.

---

# 12. Integraciones

Este módulo se relaciona con:

- Producto
- Stock
- Compra
- Venta
- Ajuste de Stock
- Sucursal
- Auditoría
- Reportes

---

# 13. Mejoras futuras

- Inventarios físicos.
- Conteos cíclicos.
- Transferencias.
- Lotes.
- Series.
- Alertas de stock.
- Dashboard.
- IA para predicción de faltantes.

---

# 14. Roadmap

Versión 1.0

- Movimientos automáticos
- Consulta
- Historial

Versión 2.0

- Ajustes
- Inventarios
- Transferencias

Versión 3.0

- IA
- Predicción de stock
- Automatización
- Dashboard

---

# 15. Decisiones de Arquitectura

## El stock nunca se modifica directamente

El inventario de un producto únicamente podrá modificarse mediante un Movimiento de Stock.

No existirá una funcionalidad que permita editar manualmente el valor del stock.

---

## Origen obligatorio

Todo movimiento deberá indicar el motivo que lo generó.

Los tipos iniciales serán:

- Compra
- Venta
- Ajuste
- Devolución de Cliente
- Devolución a Proveedor
- Transferencia entre sucursales (futuro)

No se permitirán movimientos sin un origen definido.

---

## Historial inmutable

Una vez registrado un movimiento, no podrá modificarse ni eliminarse.

Esto garantiza la trazabilidad completa del inventario.

---

## Ajustes de stock

Los ajustes representan una excepción.

Solo podrán realizarlos usuarios autorizados.

Será obligatorio registrar un motivo.

Ejemplos:

- Rotura
- Robo
- Error de carga
- Diferencia de inventario
- Producto vencido

Todos los ajustes quedarán registrados para auditoría.

---

## Stock negativo

En la versión 1.0, Veltika no permitirá stock negativo.

Una venta no podrá confirmarse si la cantidad solicitada supera el stock disponible.

En futuras versiones esta regla podrá configurarse según las necesidades de cada empresa.

---

## Trazabilidad completa

Será posible reconstruir el historial completo de un producto consultando sus movimientos.

Ejemplo:

01/06 Compra +20

03/06 Venta -2

05/06 Venta -5

08/06 Ajuste -1

Stock actual: 12

---

## Auditoría

Cada movimiento almacenará:

- Usuario que realizó la operación.
- Fecha y hora.
- Tipo de movimiento.
- Stock anterior.
- Stock nuevo.

De esta forma será posible conocer exactamente cuándo, cómo y por qué cambió el inventario de cualquier producto.