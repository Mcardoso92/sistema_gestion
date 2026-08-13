# Módulo Caja

---

# 1. Objetivo

El módulo Caja permite administrar el estado de las cajas operativas de cada sucursal dentro de Veltika.

Cada caja representa el punto donde se registran los ingresos y egresos de dinero generados por las operaciones comerciales.

Su función principal es controlar el saldo actual de la caja y su disponibilidad para operar.

---

# 2. Alcance

El módulo permite abrir, consultar y cerrar cajas de una sucursal.

No registra directamente ingresos o egresos de dinero, ya que dichas operaciones son administradas por el módulo MovimientoCaja.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Cajero

---

# 4. Permisos

## Super Administrador

✅ Consultar todas las cajas.

✅ Abrir cajas.

✅ Cerrar cajas.

## Administrador de Empresa

✅ Consultar cajas.

✅ Abrir cajas.

✅ Cerrar cajas.

## Cajero

✅ Consultar su caja.

✅ Abrir su caja (según configuración).

✅ Cerrar su caja.

❌ No puede modificar saldos manualmente.

---

# 5. Funcionalidades

Actualmente

- Apertura de caja.
- Consulta de caja.
- Cierre de caja.
- Consulta del saldo actual.

Versiones futuras

- Arqueo de caja.
- Turnos de caja.
- Múltiples cajas por sucursal.
- Caja compartida.
- Conciliación bancaria.
- Integración con terminales POS.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal |
| UsuarioId | Cajero responsable |
| FechaApertura | Fecha y hora de apertura |
| FechaCierre | Fecha y hora de cierre |
| SaldoInicial | Monto inicial |
| SaldoActual | Monto actualizado automáticamente |
| Estado | Abierta o Cerrada |

Campos futuros

- SaldoEsperado
- Diferencia
- Observaciones
- Turno
- CajaFisica
- CajaVirtual

---

# 7. Validaciones

- Debe existir una empresa.
- Debe existir una sucursal.
- Debe existir un usuario.
- El saldo inicial no podrá ser negativo.
- No podrá abrirse una nueva caja si ya existe una caja abierta para el mismo usuario (según configuración).
- Solo podrá cerrarse una caja abierta.

---

# 8. Reglas de negocio

- Cada caja pertenece a una empresa.
- Cada caja pertenece a una sucursal.
- Cada caja tendrá un usuario responsable.
- El saldo actual será actualizado automáticamente mediante los movimientos de caja.
- No podrá modificarse manualmente el saldo actual.
- Una caja cerrada no permitirá registrar nuevos movimientos.

---

# 9. Casos de uso

## Abrir caja

El usuario inicia una nueva jornada de trabajo.

Resultado esperado:

- Caja abierta.
- Saldo inicial registrado.

---

## Consultar caja

Permite visualizar el estado actual de la caja.

---

## Cerrar caja

Finaliza la jornada operativa.

Resultado esperado:

- Caja cerrada.
- Se registra la fecha y hora de cierre.

---

# 10. Casos de error

- Usuario inexistente.
- Caja ya abierta.
- Caja inexistente.
- Caja ya cerrada.
- Usuario sin permisos.

---

# 11. Flujo funcional

1. El usuario inicia su jornada.
2. Abre la caja.
3. Ingresa el saldo inicial.
4. El sistema habilita las operaciones.
5. Durante el día se generan movimientos.
6. El saldo se actualiza automáticamente.
7. El usuario realiza el cierre de caja.
8. La caja queda bloqueada para nuevas operaciones.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Sucursal
- Usuario
- MovimientoCaja
- Venta
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Arqueo automático.
- Múltiples turnos.
- Conciliación bancaria.
- Integración con POS.
- Integración con Mercado Pago.
- Caja virtual.
- Caja bancaria.

---

# 14. Roadmap

Versión 1.0

- Apertura.
- Consulta.
- Cierre.

Versión 2.0

- Arqueo.
- Turnos.
- Conciliación.

Versión 3.0

- POS.
- Mercado Pago.
- Automatización.

---

# 15. Decisiones de Arquitectura

## Caja como estado

La caja representa únicamente el estado actual de una jornada operativa.

No almacena ingresos ni egresos.

---

## Saldo automático

El saldo actual será calculado automáticamente mediante los movimientos registrados.

No podrá modificarse manualmente.

---

## Una caja abierta

En la versión 1.0, un usuario solo podrá tener una caja abierta al mismo tiempo.

---

## Caja cerrada

Una vez cerrada la caja:

- No podrán registrarse nuevas ventas.
- No podrán registrarse nuevos movimientos.
- No podrá reabrirse.

Si el usuario necesita continuar operando, deberá abrir una nueva caja.

---

## Apertura obligatoria

Para registrar ventas será obligatorio contar con una caja abierta.

Si no existe una caja abierta, el sistema impedirá continuar con la operación.

---

## Historial permanente

Las cajas nunca serán eliminadas.

Cada apertura y cierre formará parte del historial operativo de la empresa.

---

## Integridad financiera

El saldo de una caja siempre deberá poder justificarse mediante la suma de todos los movimientos registrados durante su período de apertura.