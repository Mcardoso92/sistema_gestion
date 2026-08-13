# Módulo Sucursal

## Estado de implementación

- Documentado para arquitectura futura.
- No incluido en el alcance actual de la V1.
- La primera versión opera con una única unidad lógica por empresa.

---

# 1. Objetivo

El módulo Sucursal permite administrar los distintos establecimientos físicos de una empresa dentro de Veltika.

Cada sucursal funciona como una unidad operativa independiente, permitiendo gestionar usuarios, ventas, compras, stock y caja de manera separada.

Este módulo brinda la posibilidad de escalar el sistema desde pequeños comercios hasta empresas con múltiples sucursales.

---

# 2. Alcance

El módulo permite registrar, modificar, activar, desactivar y consultar las sucursales pertenecientes a una empresa.

Cada sucursal podrá administrar sus propias operaciones comerciales manteniendo la información organizada dentro de la misma empresa.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa

---

# 4. Permisos

## Super Administrador

✅ Visualizar sucursales de cualquier empresa

✅ Crear sucursales

✅ Editar sucursales

✅ Activar sucursales

✅ Desactivar sucursales

## Administrador de Empresa

✅ Crear sucursales

✅ Editar sucursales

✅ Activar sucursales

✅ Desactivar sucursales

✅ Consultar sucursales

❌ Acceder a sucursales de otras empresas

---

# 5. Funcionalidades

Actualmente

- Registrar sucursal
- Editar sucursal
- Activar sucursal
- Desactivar sucursal
- Consultar sucursales
- Buscar sucursales

Versiones futuras

- Casa Central
- Transferencias entre sucursales
- Stock por sucursal
- Caja por sucursal
- Reportes individuales
- Horarios de atención
- Geolocalización
- Integración con Google Maps

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| Nombre | Nombre de la sucursal |
| Direccion | Dirección física |
| Telefono | Teléfono de contacto |
| Estado | Activa o Inactiva |

Campos futuros

- Email
- Localidad
- Provincia
- País
- Código Postal
- Responsable
- HorarioApertura
- HorarioCierre
- Latitud
- Longitud
- EsCasaCentral
- FechaAlta
- FechaModificacion
- UsuarioAlta
- UsuarioModificacion

---

# 7. Validaciones

- El nombre es obligatorio.
- La dirección es obligatoria.
- No puede existir otra sucursal con el mismo nombre dentro de la misma empresa.
- La empresa debe existir.
- El estado inicial será Activo.

---

# 8. Reglas de negocio

- Cada sucursal pertenece a una única empresa.
- Una empresa puede tener múltiples sucursales.
- Cada usuario podrá estar asociado a una sucursal.
- Cada venta pertenece a una sucursal.
- Cada compra pertenece a una sucursal.
- Cada caja pertenece a una sucursal.
- Una sucursal desactivada no podrá registrar nuevas operaciones.
- La eliminación física de una sucursal no estará permitida.

---

# 9. Casos de uso

## Crear sucursal

El administrador registra una nueva sucursal para la empresa.

Resultado esperado:

- Sucursal creada correctamente.
- Disponible para operar dentro del sistema.

---

## Editar sucursal

Permite modificar la información general de la sucursal.

---

## Desactivar sucursal

La sucursal deja de poder operar.

La información histórica permanece registrada.

---

## Consultar sucursales

Permite visualizar todas las sucursales registradas por la empresa.

---

# 10. Casos de error

- Nombre vacío.
- Dirección vacía.
- Sucursal duplicada.
- Empresa inexistente.
- Usuario sin permisos.
- Intento de eliminar una sucursal con operaciones asociadas.

---

# 11. Flujo funcional

1. El administrador ingresa al módulo Sucursales.
2. Selecciona "Nueva Sucursal".
3. Completa la información requerida.
4. El sistema valida los datos.
5. Se registra la sucursal.
6. La sucursal queda disponible para futuras operaciones.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Usuarios
- Productos
- Stock
- Ventas
- Compras
- Caja
- Clientes
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Stock independiente por sucursal.
- Transferencias entre sucursales.
- Caja independiente.
- Reportes individuales.
- Geolocalización.
- Integración con Google Maps.
- Gestión de depósitos.
- Indicadores de rendimiento por sucursal.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación
- Consulta

Versión 2.0

- Stock por sucursal
- Caja por sucursal
- Reportes
- Transferencias

Versión 3.0

- Multi depósito
- Geolocalización
- Integración con mapas
- Administración avanzada