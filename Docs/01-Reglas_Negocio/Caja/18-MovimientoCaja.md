# Módulo Movimiento de Caja

---

# 1. Objetivo

El módulo Movimiento de Caja registra todos los ingresos y egresos de dinero producidos durante la operación de una caja.

Cada movimiento representa una transacción económica que afecta el saldo de una caja, permitiendo mantener un historial completo y auditable de todas las operaciones financieras.

Este módulo constituye la base para el control de caja y la conciliación de los movimientos monetarios.

---

# 2. Alcance

El módulo permite registrar y consultar todos los movimientos realizados sobre una caja abierta.

Los movimientos pueden originarse automáticamente por ventas o registrarse manualmente en casos autorizados, como ingresos o egresos extraordinarios.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Cajero

---

# 4. Permisos

## Super Administrador

✅ Consultar todos los movimientos.

✅ Registrar movimientos manuales.

## Administrador de Empresa

✅ Consultar movimientos.

✅ Registrar movimientos manuales.

## Cajero

✅ Consultar movimientos de su caja.

✅ Registrar ingresos y egresos manuales autorizados.

❌ No puede modificar ni eliminar movimientos.

---

# 5. Funcionalidades

Actualmente

- Registrar movimientos automáticos.
- Registrar ingresos manuales.
- Registrar egresos manuales.
- Consultar movimientos.
- Buscar movimientos.
- Filtrar movimientos.

Versiones futuras

- Integración bancaria.
- Integración con Mercado Pago.
- Conciliación automática.
- Exportación contable.
- Firma digital de movimientos.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| CajaId | Caja asociada |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal |
| UsuarioId | Usuario responsable |
| TipoMovimiento | Ingreso o Egreso |
| OrigenMovimiento | Venta, Ajuste, Retiro, etc. |
| Importe | Monto del movimiento |
| SaldoAnterior | Saldo previo |
| SaldoNuevo | Saldo posterior |
| Observaciones | Información adicional |
| FechaMovimiento | Fecha y hora |

Campos futuros

- VentaId
- CompraId
- MedioPagoId
- NumeroComprobante
- DocumentoReferencia
- Estado

---

# 7. Validaciones

- Debe existir una caja abierta.
- El importe debe ser mayor a cero.
- Debe existir un usuario autorizado.
- El tipo de movimiento es obligatorio.
- El origen del movimiento es obligatorio.
- El sistema registrará automáticamente la fecha del movimiento.

---

# 8. Reglas de negocio

- Todo movimiento modifica automáticamente el saldo de la caja.
- Los movimientos nunca podrán eliminarse.
- Los movimientos nunca podrán modificarse.
- Todo movimiento deberá indicar su origen.
- Los movimientos permanecerán almacenados permanentemente.

---

# 9. Casos de uso

## Registrar ingreso

Permite ingresar dinero a la caja.

Resultado esperado:

- Saldo actualizado.
- Movimiento registrado.

---

## Registrar egreso

Permite retirar dinero de la caja.

Resultado esperado:

- Saldo actualizado.
- Movimiento registrado.

---

## Consultar movimientos

Permite visualizar el historial completo de movimientos de una caja.

---

## Buscar movimientos

Permite localizar movimientos utilizando distintos filtros.

---

# 10. Casos de error

- Caja cerrada.
- Caja inexistente.
- Usuario sin permisos.
- Importe inválido.
- Tipo de movimiento inválido.

---

# 11. Flujo funcional

1. El usuario selecciona la caja.
2. Indica el tipo de movimiento.
3. Ingresa el importe.
4. Selecciona el origen.
5. Agrega observaciones (opcional).
6. Confirma la operación.
7. El sistema actualiza el saldo.
8. Se registra el movimiento.
9. El historial queda disponible para futuras consultas.

---

# 12. Integraciones

Este módulo se relaciona con:

- Caja
- Venta
- Compra
- Usuario
- Empresa
- Sucursal
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Conciliación bancaria.
- Integración con billeteras virtuales.
- Firma digital.
- Exportación contable.
- Alertas automáticas.
- Clasificación de movimientos.

---

# 14. Roadmap

Versión 1.0

- Registro.
- Consulta.
- Historial.

Versión 2.0

- Conciliación.
- Integraciones.
- Exportación.

Versión 3.0

- Automatización.
- IA.
- Dashboard financiero.

---

# 15. Decisiones de Arquitectura

## Toda operación financiera genera un movimiento

Todo ingreso o egreso deberá registrarse mediante un Movimiento de Caja.

No existirá otra forma de modificar el saldo.

---

## Origen obligatorio

Todo movimiento deberá indicar qué operación lo originó.

Orígenes iniciales:

- Venta
- Compra
- Apertura de caja
- Cierre de caja
- Ingreso manual
- Egreso manual
- Ajuste de caja

---

## Historial inmutable

Una vez registrado, un movimiento no podrá modificarse ni eliminarse.

Toda la información permanecerá disponible para auditorías futuras.

---

## Saldo automático

El saldo de la caja será actualizado automáticamente luego de registrar cada movimiento.

Nunca podrá editarse manualmente.

---

## Caja abierta obligatoria

No podrán registrarse movimientos si la caja se encuentra cerrada.

---

## Auditoría financiera

Cada movimiento almacenará:

- Usuario responsable.
- Fecha y hora.
- Caja afectada.
- Tipo de movimiento.
- Saldo anterior.
- Saldo nuevo.
- Observaciones.

---

## Integridad financiera

El saldo de una caja siempre deberá coincidir con la suma de todos los movimientos registrados desde su apertura hasta su cierre.