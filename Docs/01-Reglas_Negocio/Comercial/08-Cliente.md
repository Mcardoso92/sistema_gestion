# Módulo Cliente

---

# 1. Objetivo

El módulo Cliente permite administrar la información de las personas o empresas que realizan compras dentro de Veltika.

Cada cliente pertenece exclusivamente a una empresa y podrá ser utilizado en las ventas, reportes, cuentas corrientes y futuras funcionalidades comerciales.

Este módulo constituye la base para la gestión de clientes y el análisis comercial.

---

# 2. Alcance

El módulo permite registrar, modificar, activar, desactivar y consultar clientes pertenecientes a una empresa.

Cada cliente podrá asociarse a múltiples ventas y conservar su historial de operaciones.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Vendedor
- Cajero

---

# 4. Permisos

## Super Administrador

✅ Visualizar clientes de cualquier empresa

✅ Crear clientes

✅ Editar clientes

✅ Activar clientes

✅ Desactivar clientes

## Administrador de Empresa

✅ Crear clientes

✅ Editar clientes

✅ Activar clientes

✅ Desactivar clientes

✅ Consultar clientes

## Vendedor

✅ Consultar clientes

✅ Registrar clientes

✅ Editar información básica

## Cajero

✅ Consultar clientes

✅ Registrar clientes durante una venta

❌ Desactivar clientes

---

# 5. Funcionalidades

Actualmente

- Registrar cliente
- Editar cliente
- Activar cliente
- Desactivar cliente
- Consultar clientes
- Buscar clientes

Versiones futuras

- Cuenta corriente
- Historial de compras
- Programa de puntos
- Descuentos personalizados
- Clasificación de clientes
- Importación masiva
- Exportación
- Integración con CRM
- Clientes frecuentes
- Etiquetas

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| Nombre | Nombre del cliente |
| Apellido | Apellido del cliente |
| Documento | DNI o documento identificatorio |
| Telefono | Teléfono |
| Email | Correo electrónico |
| Direccion | Dirección |
| Estado | Activo o Inactivo |

Campos futuros

- CUIT
- Razón Social
- FechaNacimiento
- Localidad
- Provincia
- País
- Código Postal
- Observaciones
- LimiteCredito
- SaldoCuentaCorriente
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El apellido es obligatorio.
- El documento no puede repetirse dentro de la misma empresa (si se informa).
- El correo electrónico debe tener un formato válido.
- La empresa debe existir.
- El estado inicial será Activo.

---

# 8. Reglas de negocio

- Cada cliente pertenece exclusivamente a una empresa.
- Un cliente puede realizar múltiples compras.
- Las ventas conservarán el cliente asociado aunque éste sea desactivado.
- Un cliente desactivado no podrá utilizarse en nuevas ventas.
- La eliminación física de clientes no estará permitida.

---

# 9. Casos de uso

## Crear cliente

El usuario registra un nuevo cliente.

Resultado esperado:

- Cliente creado correctamente.
- Disponible para futuras ventas.

---

## Editar cliente

Permite modificar la información del cliente.

---

## Desactivar cliente

El cliente deja de poder utilizarse en nuevas operaciones.

Las ventas históricas permanecen registradas.

---

## Consultar clientes

Permite visualizar todos los clientes registrados por la empresa.

---

## Buscar cliente

Permite localizar clientes mediante distintos criterios de búsqueda.

---

# 10. Casos de error

- Nombre vacío.
- Apellido vacío.
- Documento duplicado.
- Correo electrónico inválido.
- Empresa inexistente.
- Usuario sin permisos.
- Intento de modificar un cliente inexistente.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Clientes.
2. Selecciona "Nuevo Cliente".
3. Completa la información requerida.
4. El sistema valida los datos.
5. Se registra el cliente.
6. El cliente queda disponible para futuras ventas.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Ventas
- Caja
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Cuenta corriente.
- Programa de fidelización.
- Historial completo de compras.
- Ranking de clientes.
- Segmentación comercial.
- Integración con CRM.
- Envío de promociones.
- Estadísticas de consumo.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación
- Consulta
- Búsqueda

Versión 2.0

- Cuenta corriente
- Historial de compras
- Descuentos personalizados
- Clasificación de clientes

Versión 3.0

- CRM
- Programa de puntos
- Segmentación avanzada
- Marketing automatizado