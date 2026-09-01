# Módulo Sucursal

Última actualización: 01/09/2026

---

# Estado

🚧 **Diseño futuro — no implementado actualmente.**

Veltika opera hoy con una única unidad lógica por empresa y no posee todavía una entidad `Sucursal`, un `SucursalController` ni relaciones productivas por sucursal en los módulos operativos.

Este documento describe una posible evolución futura del sistema y no debe utilizarse como referencia del comportamiento actual.

---

# 1. Objetivo futuro

El módulo Sucursal permitiría administrar múltiples establecimientos físicos u operativos dentro de una misma empresa.

La incorporación de sucursales tendría como objetivo permitir que una empresa pueda separar y consolidar operaciones por ubicación, manteniendo siempre el aislamiento SaaS a nivel Empresa.

Ejemplo conceptual:

```text
Empresa
├── Sucursal A
├── Sucursal B
└── Sucursal C
```

Una sucursal nunca reemplazaría a `Empresa` como límite principal de seguridad multiempresa.

---

# 2. Situación actual

Actualmente:

- No existe entidad `Sucursal` en el modelo productivo.
- No existe `SucursalId` en las entidades operativas actuales como regla vigente.
- Productos pertenecen directamente a una Empresa.
- Stock se administra actualmente a nivel Empresa/Producto.
- Ventas pertenecen actualmente a una Empresa.
- Compras pertenecen actualmente a una Empresa.
- Cajas pertenecen actualmente a una Empresa.
- Usuarios pertenecen actualmente a una Empresa.
- Reportes y Dashboard utilizan actualmente el contexto de Empresa.

Por lo tanto, cualquier documentación antigua que mencione campos `SucursalId` como si estuvieran implementados debe considerarse pendiente de corrección.

---

# 3. Motivo para no implementarlo todavía

La arquitectura multi-sucursal agrega complejidad transversal porque afecta prácticamente todos los módulos principales.

Implementarla correctamente requeriría definir antes, entre otras cosas:

- Qué información pertenece a Empresa y cuál a Sucursal.
- Cómo se administra el stock por ubicación.
- Cómo se manejan transferencias internas.
- Cómo funcionan cajas y turnos por sucursal.
- Cómo se asignan usuarios.
- Qué operaciones pueden consultar otras sucursales.
- Cómo funcionan reportes consolidados.
- Cómo se resuelven compras centralizadas o por sucursal.
- Cómo se manejan productos compartidos entre sucursales.

Por este motivo, no debe incorporarse sólo como un campo adicional `SucursalId` sin definir previamente las reglas de negocio completas.

---

# 4. Principio arquitectónico

Cuando se implemente, la jerarquía deberá respetar:

```text
Empresa
  ↓
Sucursal
  ↓
Operaciones dependientes de ubicación
```

La empresa continuará siendo el tenant SaaS.

Una sucursal siempre deberá pertenecer a una única Empresa.

Toda validación de acceso deberá comprobar primero Empresa y luego, cuando corresponda, Sucursal.

Nunca deberá ser posible relacionar una sucursal con información perteneciente a otra empresa.

---

# 5. Modelo conceptual posible

Un modelo inicial podría contemplar:

| Campo | Descripción |
|---|---|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| Nombre | Nombre de la sucursal |
| Direccion | Dirección física opcional según definición futura |
| Telefono | Teléfono opcional |
| Estado | Activa o inactiva |
| EsCasaCentral | Identificación de sucursal principal, si se decide utilizar este concepto |
| FechaAlta | Fecha de creación |

Estos campos son **propuestos**, no implementados.

Antes de desarrollar deberá definirse el modelo definitivo mediante Issue y, si corresponde, ADR.

---

# 6. Áreas que podría afectar

## Usuarios

Deberá definirse si:

- Un usuario pertenece a una sola sucursal.
- Puede operar varias sucursales.
- Existen permisos diferentes según sucursal.

No debe asumirse anticipadamente una relación uno-a-uno entre Usuario y Sucursal.

## Productos

La definición más probable es mantener el catálogo de productos a nivel Empresa y separar únicamente las existencias por ubicación.

Esto evitaría duplicar productos idénticos para cada sucursal.

La decisión definitiva deberá validarse antes de implementar.

## Stock

Multi-sucursal probablemente requerirá evolucionar del stock actual por Producto hacia un modelo de existencias por ubicación.

Ejemplo conceptual:

```text
Producto
├── Stock Sucursal A
├── Stock Sucursal B
└── Stock Sucursal C
```

También deberá definirse la diferencia entre:

- Sucursal.
- Depósito.
- Ubicación de stock.

No necesariamente deben representar el mismo concepto.

## Ventas

Una venta podría quedar asociada a la sucursal donde se realizó.

Debe conservarse además `EmpresaId` o una forma equivalente de validar eficientemente el tenant y mantener seguridad multiempresa.

## Compras

Deberá definirse si una compra:

- Pertenece a una sucursal.
- Pertenece a la empresa y luego se distribuye.
- Puede recibirse parcialmente en diferentes depósitos o sucursales.

## Caja

Las cajas podrían pertenecer a una sucursal.

Esto permitiría:

- Turnos por sucursal.
- Cierres independientes.
- Transferencias entre cajas de distintas ubicaciones.
- Reportes financieros por sucursal.

## Reportes y Dashboard

Deberían soportar al menos dos niveles de análisis:

- Vista de una sucursal.
- Vista consolidada de toda la empresa.

---

# 7. Transferencias

La existencia de sucursales probablemente requerirá transferencias de stock entre ubicaciones.

Una transferencia debería preservar trazabilidad completa:

- Empresa.
- Ubicación origen.
- Ubicación destino.
- Producto.
- Cantidad.
- Fecha.
- Usuario responsable.
- Estado de transferencia.

Dependiendo del flujo futuro podría necesitar estados como:

- Pendiente.
- En tránsito.
- Recibida.
- Cancelada.

Esto todavía no está definido ni implementado.

---

# 8. Seguridad futura

La seguridad deberá impedir:

- Acceder a sucursales de otra empresa.
- Enviar un `SucursalId` perteneciente a otro tenant.
- Consultar stock de ubicaciones no autorizadas.
- Registrar operaciones en una sucursal no permitida.

La validación debe ejecutarse siempre en servidor.

La incorporación futura de permisos granulares deberá considerar también el alcance por sucursal si el producto lo necesita.

---

# 9. Baja lógica

Si se implementa, una sucursal con información histórica no debería eliminarse físicamente.

La estrategia esperada sería baja lógica mediante un estado activo/inactivo.

Una sucursal desactivada debería conservar:

- Ventas históricas.
- Compras históricas.
- Stock/movimientos históricos.
- Cajas y cierres históricos.
- Relaciones con usuarios y operaciones pasadas.

Las reglas para desactivarla deberán definirse cuando se implemente.

---

# 10. Migración desde el modelo actual

Uno de los puntos más importantes antes de implementar Sucursal será migrar correctamente empresas existentes.

Una estrategia posible sería crear automáticamente una primera sucursal para cada empresa existente, por ejemplo:

```text
Casa Central
```

y asociar allí los datos que pasen a depender de una ubicación.

Sin embargo, esta estrategia todavía no está aprobada y deberá evaluarse cuidadosamente antes de crear migraciones.

La migración deberá evitar pérdida de:

- Stock.
- Ventas.
- Compras.
- Caja.
- Trazabilidad.

---

# 11. Dependencia con depósitos

Sucursal y Depósito no deben tratarse automáticamente como sinónimos.

Una empresa podría necesitar en el futuro:

```text
Empresa
└── Sucursal
    ├── Depósito principal
    ├── Depósito secundario
    └── Salón de venta
```

Por ese motivo, antes de desarrollar multi-sucursal deberá decidirse si el primer modelo necesita:

- Sólo Sucursal.
- Sólo Depósito/Ubicación.
- Ambos conceptos.

Agregar ambos prematuramente aumentaría la complejidad sin beneficio comprobado.

---

# 12. Condiciones para comenzar implementación

El módulo debería desarrollarse únicamente cuando exista una necesidad validada con usuarios reales o una exigencia comercial clara.

Antes de comenzar deberán definirse al menos:

1. Alcance exacto del MVP multi-sucursal.
2. Relación Empresa-Sucursal.
3. Modelo de stock por ubicación.
4. Relación de Caja con Sucursal.
5. Relación de Usuario con Sucursal.
6. Flujo de transferencias.
7. Estrategia de migración de empresas existentes.
8. Seguridad y permisos.
9. Reportes consolidados.
10. Impacto sobre ventas y compras.

---

# 13. Capacidades posibles posteriores

Una vez implementada una base multi-sucursal, podrían evaluarse:

- Múltiples depósitos.
- Transferencias de stock.
- Compras con recepción por ubicación.
- Stock reservado.
- Roles y permisos por sucursal.
- Objetivos de venta por sucursal.
- Comparación entre sucursales.
- Indicadores de rendimiento.
- Horarios de atención.
- Dirección y geolocalización.

Funciones como mapas o geolocalización no son requisitos estructurales y sólo deberían implementarse si aportan valor real.

---

# 14. Relación con el Roadmap

La evolución multi-sucursal se mantiene en el Roadmap general dentro de la línea de:

- Sucursales.
- Depósitos.
- Stock independiente.
- Transferencias.
- Caja por sucursal.
- Reportes consolidados.

Este documento no mantiene un roadmap por versiones independiente.

---

# 15. Estado final del documento

❌ Entidad Sucursal no implementada.

❌ CRUD de Sucursal no implementado.

❌ Stock por sucursal no implementado.

❌ Ventas por sucursal no implementadas.

❌ Compras por sucursal no implementadas.

❌ Cajas por sucursal no implementadas.

❌ Transferencias de stock entre sucursales no implementadas.

✅ Concepto identificado como posible evolución post-MVP.

✅ Principios de seguridad multiempresa definidos como requisito obligatorio para una futura implementación.

✅ Necesidad de diseñar migración desde el modelo actual identificada.

**Este documento es una especificación conceptual futura y no describe funcionalidad disponible actualmente en Veltika.**