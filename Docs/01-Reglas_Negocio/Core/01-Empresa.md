# Módulo Empresa

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Empresa representa cada organización que utiliza Veltika y constituye la raíz del aislamiento de datos de la arquitectura SaaS multiempresa.

Cada empresa posee su propia información operativa y de configuración. Los datos de una empresa no deben ser accesibles ni relacionarse accidentalmente con los de otra.

---

# 2. Alcance actual

El módulo permite al `SuperAdmin`:

- Visualizar empresas.
- Buscar empresas por nombre.
- Filtrar por activas, inactivas o todas.
- Consultar el detalle de una empresa.
- Crear empresas.
- Editar empresas.
- Desactivar empresas mediante baja lógica.
- Reactivar empresas desde la edición.

La creación de una empresa también ejecuta una inicialización automática de datos base necesarios para comenzar a operar.

---

# 3. Actores y permisos

## SuperAdmin

El `EmpresaController` se encuentra protegido mediante:

```text
[Authorize(Roles = "SuperAdmin")]
```

Por lo tanto, la administración global de empresas corresponde actualmente al rol `SuperAdmin`.

Puede:

- Listar empresas.
- Consultar detalles.
- Crear empresas.
- Editar empresas.
- Activar o desactivar empresas.

## Administrador de empresa

La administración global de registros `Empresa` no se realiza mediante `EmpresaController`.

Los datos y preferencias propias de una empresa que puedan ser modificados por sus administradores deben gestionarse mediante los módulos específicos de configuración y respetando el contexto de la empresa autenticada.

---

# 4. Modelo actual

La entidad `Empresa` contiene actualmente:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| Nombre | string | Obligatorio, máximo 50 caracteres |
| Estado | bool | Indica si la empresa está activa |
| FechaAlta | DateTime | Se asigna automáticamente al crear la empresa |

La entidad también posee relaciones con distintos módulos del sistema, entre ellos:

- Usuarios.
- Productos.
- Categorías.
- Clientes.
- Proveedores.
- Ventas.
- Compras.
- Movimientos de stock.
- Cajas.
- Medios de pago.
- Categorías de gasto.
- Turnos de caja.
- Cobros de ventas.
- Pagos a proveedores.
- Reintegros de ventas.
- Reintegros a proveedores.
- Transferencias de caja.
- Movimientos de caja.
- Configuración de empresa.

---

# 5. Listado y consulta

El listado de empresas permite actualmente:

- Mostrar empresas activas por defecto.
- Filtrar empresas inactivas.
- Mostrar todas las empresas.
- Buscar por nombre.
- Ordenar alfabéticamente por nombre.
- Paginar resultados.

La paginación actual utiliza 20 registros por página.

Las consultas de listado utilizan `AsNoTracking()` al no requerir seguimiento de cambios.

---

# 6. Creación de empresa

## Datos ingresados

Actualmente el alta administrativa solicita únicamente:

- Nombre.

El servidor establece automáticamente:

- `FechaAlta = DateTime.Now`.
- `Estado = true`.

El cliente no controla estos valores durante el alta.

## Validaciones

- El nombre es obligatorio.
- El nombre admite como máximo 50 caracteres.
- No puede existir otra empresa con el mismo nombre ignorando diferencias entre mayúsculas y minúsculas.

## Transacción

La creación de la empresa y su inicialización se ejecutan dentro de una transacción.

Si la inicialización falla, la operación debe revertirse para evitar una empresa creada parcialmente.

---

# 7. Inicialización automática

Después de guardar una nueva empresa, `EmpresaInicializacionService` genera los datos base necesarios para comenzar a operar.

Actualmente inicializa:

## Medios de pago predeterminados

Se crean los medios de pago definidos por la configuración inicial cuando todavía no existen para la empresa.

## Categoría predeterminada

Se crea:

```text
Sin categoría
```

si todavía no existe.

Esto permite disponer de una categoría base para productos desde el comienzo.

## Caja principal

Se crea:

```text
Caja principal
```

con las siguientes características iniciales:

- Tipo Efectivo.
- Permite turnos.
- Fondo fijo inicial 0.
- Estado activo.
- Fecha de alta igual a la fecha de creación de la empresa.

## Asociación con efectivo

La Caja principal se vincula con el medio de pago de tipo Efectivo cuando esa asociación todavía no existe.

---

# 8. Edición y reactivación

Actualmente pueden modificarse:

- Nombre.
- Estado.

La fecha de alta original no se modifica.

Al editar:

- Debe existir la empresa solicitada.
- El `Id` de la ruta debe coincidir con el registro enviado.
- El nuevo nombre no puede duplicar el de otra empresa.

Una empresa inactiva puede reactivarse estableciendo nuevamente su estado como activo desde la edición.

---

# 9. Desactivación

La eliminación administrativa es lógica.

La acción de desactivación:

```text
Estado = false
```

No elimina físicamente la empresa ni sus datos asociados.

Esto permite preservar:

- Información histórica.
- Relaciones existentes.
- Trazabilidad.
- Posibilidad de reactivación.

---

# 10. Reglas de negocio

1. Una empresa constituye el límite lógico principal de datos dentro de Veltika.
2. Los datos dependientes deben pertenecer a la empresa correspondiente.
3. Ningún usuario debe acceder a información de otra empresa.
4. La seguridad multiempresa debe validarse en el servidor.
5. El nombre de empresa debe ser único según la validación administrativa actual.
6. Una empresa nueva comienza activa.
7. La fecha de alta se genera en el servidor.
8. La desactivación no elimina información físicamente.
9. La reactivación debe conservar los datos históricos existentes.
10. La creación debe completar correctamente la inicialización de datos base o revertirse completa.

---

# 11. Seguridad

La administración global del módulo está restringida a `SuperAdmin`.

Los formularios POST utilizan protección antiforgery.

Los valores sensibles para el estado inicial de una empresa no deben confiarse al cliente durante el alta.

La entidad Empresa es especialmente sensible porque representa el límite de aislamiento SaaS. Cualquier funcionalidad futura relacionada con empresas debe preservar este principio.

---

# 12. Casos de error relevantes

- Nombre vacío.
- Nombre con más de 50 caracteres.
- Empresa duplicada.
- Empresa inexistente.
- ID de edición inconsistente.
- Error durante la inicialización de datos base.
- Error de persistencia durante creación, edición o desactivación.
- Usuario sin rol `SuperAdmin` intentando acceder al módulo administrativo.

Los errores durante la creación no deben dejar datos parciales debido al uso de transacción.

---

# 13. Integraciones actuales

Empresa se relaciona transversalmente con prácticamente toda la aplicación.

Entre los módulos principales:

- Usuarios e Identity.
- Configuración de empresa.
- Categorías.
- Productos.
- Clientes.
- Proveedores.
- Ventas.
- Compras.
- Stock.
- Cajas.
- Medios de pago.
- Turnos de caja.
- Gastos.
- Cobros.
- Pagos.
- Reintegros.
- Reportes.
- Dashboard.

---

# 14. Capacidades no implementadas en este módulo

Los siguientes conceptos forman parte de la evolución posible de Veltika, pero no deben interpretarse como campos o funcionalidades actuales del modelo `Empresa`:

- CUIT y datos fiscales completos.
- Razón social.
- Dirección comercial completa.
- País, provincia y localidad como estructura formal.
- Logo almacenado directamente en `Empresa`.
- Moneda configurable por empresa.
- Zona horaria configurable.
- Idioma configurable.
- Plan contratado.
- Suscripción SaaS.
- Facturación automática de la suscripción.
- Multi-sucursal.

Estas capacidades deberán implementarse únicamente cuando exista el modelo y la regla de negocio correspondiente.

---

# 15. Evolución futura

La evolución del concepto Empresa se gestiona mediante el Roadmap general y GitHub Issues.

Entre las líneas futuras se encuentran:

- Planes y suscripciones SaaS.
- Período de prueba.
- Estado comercial de la suscripción.
- Límites por plan cuando sean necesarios.
- Sucursales.
- Configuración fiscal avanzada.
- Personalización adicional.
- Métricas de adopción y uso.

No se mantiene un roadmap de versiones independiente dentro de este documento.

---

# 16. Estado

✅ Módulo administrativo implementado.

✅ Baja lógica y reactivación implementadas.

✅ Búsqueda, filtros y paginación implementados.

✅ Inicialización automática de datos base implementada.

✅ Restricción administrativa a `SuperAdmin` implementada.

🚧 Evolución comercial SaaS pendiente para etapas posteriores al MVP.