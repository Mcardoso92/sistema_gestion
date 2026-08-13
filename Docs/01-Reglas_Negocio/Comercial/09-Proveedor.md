# Módulo Proveedor

---

# 1. Objetivo

El módulo Proveedor permite administrar la información de las personas o empresas que abastecen de productos a cada empresa dentro de Veltika.

Cada proveedor pertenece exclusivamente a una empresa y podrá asociarse a múltiples compras, permitiendo mantener un historial completo de abastecimiento.

Este módulo constituye la base para la gestión de compras y proveedores.

---

# 2. Alcance

El módulo permite registrar, modificar, activar, desactivar y consultar proveedores pertenecientes a una empresa.

Cada proveedor podrá utilizarse en futuras compras y mantener el historial de operaciones realizadas.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Responsable de Compras

---

# 4. Permisos

## Super Administrador

✅ Visualizar proveedores de cualquier empresa

✅ Crear proveedores

✅ Editar proveedores

✅ Activar proveedores

✅ Desactivar proveedores

## Administrador de Empresa

✅ Crear proveedores

✅ Editar proveedores

✅ Activar proveedores

✅ Desactivar proveedores

✅ Consultar proveedores

## Responsable de Compras

✅ Consultar proveedores

✅ Registrar proveedores

✅ Editar información básica

❌ Desactivar proveedores

---

# 5. Funcionalidades

Actualmente

- Registrar proveedor
- Editar proveedor
- Activar proveedor
- Desactivar proveedor
- Consultar proveedores
- Buscar proveedores

Versiones futuras

- Cuenta corriente
- Historial de compras
- Evaluación de proveedores
- Ranking de proveedores
- Importación masiva
- Exportación
- Gestión de contactos
- Catálogo de productos
- Integración con compras automáticas

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| Nombre | Nombre del proveedor |
| CUIT | CUIT del proveedor |
| Telefono | Teléfono |
| Email | Correo electrónico |
| Direccion | Dirección |
| Estado | Activo o Inactivo |

Campos futuros

- Razón Social
- Nombre de Fantasía
- Contacto Principal
- Localidad
- Provincia
- País
- Código Postal
- Sitio Web
- Condición IVA
- Observaciones
- CuentaCorriente
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- El CUIT no puede repetirse dentro de la misma empresa (si se informa).
- El correo electrónico debe tener un formato válido.
- La empresa debe existir.
- El estado inicial será Activo.

---

# 8. Reglas de negocio

- Cada proveedor pertenece exclusivamente a una empresa.
- Un proveedor puede estar asociado a múltiples compras.
- Las compras conservarán el proveedor asociado aunque éste sea desactivado.
- Un proveedor desactivado no podrá utilizarse en nuevas compras.
- La eliminación física de proveedores no estará permitida.

---

# 9. Casos de uso

## Crear proveedor

El usuario registra un nuevo proveedor.

Resultado esperado:

- Proveedor creado correctamente.
- Disponible para futuras compras.

---

## Editar proveedor

Permite modificar la información general del proveedor.

---

## Desactivar proveedor

El proveedor deja de poder utilizarse en nuevas compras.

Las compras históricas permanecen registradas.

---

## Consultar proveedores

Permite visualizar todos los proveedores registrados por la empresa.

---

## Buscar proveedor

Permite localizar proveedores mediante distintos criterios de búsqueda.

---

# 10. Casos de error

- Nombre vacío.
- CUIT duplicado.
- Correo electrónico inválido.
- Empresa inexistente.
- Usuario sin permisos.
- Intento de modificar un proveedor inexistente.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Proveedores.
2. Selecciona "Nuevo Proveedor".
3. Completa la información requerida.
4. El sistema valida los datos.
5. Se registra el proveedor.
6. El proveedor queda disponible para futuras compras.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Compras
- Stock
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Cuenta corriente.
- Historial de compras.
- Evaluación de proveedores.
- Ranking de proveedores.
- Gestión de contactos.
- Catálogo de productos.
- Integración con órdenes de compra.
- Compras automáticas.

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
- Gestión de contactos
- Evaluación de proveedores

Versión 3.0

- Catálogo de productos
- Compras automáticas
- Integración con proveedores
- Reportes avanzados