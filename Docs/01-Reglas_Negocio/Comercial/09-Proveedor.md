# Módulo Proveedor

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Proveedor administra las personas o empresas que abastecen de productos o servicios a una empresa dentro de Veltika.

Cada proveedor pertenece a una única empresa y puede asociarse a múltiples compras, preservando el historial comercial aunque posteriormente sea desactivado.

---

# 2. Alcance actual

Actualmente permite:

- Listar proveedores.
- Buscar proveedores.
- Filtrar por estado.
- Filtrar por empresa para `SuperAdmin`.
- Consultar detalle.
- Crear proveedores.
- Editar proveedores.
- Desactivar proveedores mediante baja lógica.
- Reactivar proveedores desde edición.
- Asociar proveedores a compras.

---

# 3. Actores y permisos

El módulo administrativo está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto, el CRUD actual no está habilitado directamente para un rol separado de `Responsable de Compras`.

## SuperAdmin

Puede:

- Consultar proveedores de todas las empresas.
- Filtrar por empresa.
- Crear proveedores para una empresa activa.
- Editar proveedores de cualquier empresa.
- Desactivar y reactivar proveedores.

## AdminEmpresa

Puede:

- Consultar únicamente proveedores de su empresa.
- Crear proveedores para su empresa.
- Editar proveedores de su empresa.
- Desactivar y reactivar proveedores de su empresa.

Para `AdminEmpresa`, el `EmpresaId` utilizado en creación se obtiene del usuario autenticado.

---

# 4. Modelo actual

La entidad `Proveedor` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| RazonSocial | string | Obligatoria, máximo 150 caracteres |
| NombreFantasia | string? | Opcional, máximo 150 caracteres |
| CUIT | string? | Opcional, máximo 11 dígitos almacenados |
| Email | string? | Opcional, formato email, máximo 150 caracteres |
| Telefono | string? | Opcional, máximo 50 caracteres |
| Direccion | string? | Opcional, máximo 200 caracteres |
| Localidad | string? | Opcional, máximo 100 caracteres |
| Provincia | string? | Opcional, máximo 100 caracteres |
| CodigoPostal | string? | Opcional, máximo 20 caracteres |
| Observaciones | string? | Opcional, máximo 500 caracteres |
| Estado | bool | Activo o inactivo |
| FechaAlta | DateTime | Fecha de creación |
| EmpresaId | int | Empresa propietaria |

Relaciones:

- Empresa.
- Compras.

---

# 5. Listado y búsqueda

El listado muestra proveedores activos por defecto.

Permite filtrar por:

- Activos.
- Inactivos.
- Todos.
- Empresa para `SuperAdmin`.

La búsqueda permite coincidencias en:

- Razón social.
- Nombre de fantasía.
- Email.
- CUIT.

Para búsquedas por CUIT, el texto ingresado se normaliza conservando únicamente dígitos.

Los resultados se ordenan por razón social.

La paginación actual utiliza 20 registros por página.

Las consultas de listado utilizan `AsNoTracking()` y proyectan los resultados a `ProveedorIndexItemVM`.

---

# 6. Creación

La creación utiliza `ProveedorCreateVM`.

Puede ingresarse:

- Razón social.
- Nombre de fantasía.
- CUIT.
- Email.
- Teléfono.
- Dirección.
- Localidad.
- Provincia.
- Código postal.
- Observaciones.
- Empresa cuando opera un `SuperAdmin`.

El servidor asigna automáticamente:

```text
Estado = true
FechaAlta = DateTime.Now
```

Para `AdminEmpresa`, la empresa utilizada es la del usuario autenticado.

---

# 7. Normalización de datos

Antes de persistir:

- `RazonSocial` se guarda utilizando `Trim()`.
- Los textos opcionales vacíos se convierten en `null`.
- Los textos opcionales informados se guardan sin espacios externos.
- El CUIT se normaliza eliminando guiones, espacios y cualquier carácter no numérico.

Ejemplo:

```text
20-12345678-3
```

se almacena como:

```text
20123456783
```

---

# 8. Validación de CUIT

El CUIT es opcional.

Cuando se informa:

1. Se eliminan todos los caracteres no numéricos.
2. Debe quedar compuesto exactamente por 11 dígitos.
3. Se valida el dígito verificador mediante el algoritmo implementado en servidor.
4. Se controla duplicidad dentro de la empresa.

Actualmente la regla de duplicidad busca otro **proveedor activo** con el mismo CUIT dentro de la misma empresa.

Esto significa que un proveedor inactivo con ese CUIT no bloquea actualmente el alta o actualización de otro proveedor activo.

El mismo CUIT puede existir en empresas distintas.

---

# 9. Otras validaciones

## Razón social

- Obligatoria.
- Máximo 150 caracteres.

## Nombre de fantasía

- Opcional.
- Máximo 150 caracteres.

## Email

- Opcional.
- Formato válido cuando se informa.
- Máximo 150 caracteres.

## Teléfono

- Opcional.
- Máximo 50 caracteres.

## Dirección

- Opcional.
- Máximo 200 caracteres.

## Localidad y Provincia

- Opcionales.
- Máximo 100 caracteres cada una.

## Código postal

- Opcional.
- Máximo 20 caracteres.

## Observaciones

- Opcionales.
- Máximo 500 caracteres.

## Empresa

La empresa seleccionada durante creación debe existir y encontrarse activa.

---

# 10. Edición y reactivación

La edición utiliza `ProveedorEditVM`.

Pueden modificarse:

- Razón social.
- Nombre de fantasía.
- CUIT.
- Email.
- Teléfono.
- Dirección.
- Localidad.
- Provincia.
- Código postal.
- Observaciones.
- Estado.

La empresa propietaria no forma parte del flujo normal de edición actual del proveedor.

Por lo tanto, incluso para `SuperAdmin`, el proveedor mantiene actualmente su `EmpresaId` original durante edición.

Una entidad inactiva puede reactivarse estableciendo nuevamente:

```text
Estado = true
```

La fecha de alta original no se modifica.

---

# 11. Desactivación

La baja es lógica:

```text
Estado = false
```

El proveedor no se elimina físicamente.

Si el proveedor ya se encuentra inactivo, el sistema evita repetir la operación.

La baja lógica preserva las compras históricas relacionadas.

---

# 12. Relación con compras

Un proveedor puede asociarse a múltiples compras.

La desactivación del proveedor no debe eliminar ni modificar operaciones históricas.

La disponibilidad de proveedores para nuevas compras debe respetar:

- Empresa.
- Estado.

Las reglas específicas de selección y uso corresponden al módulo Compra.

---

# 13. Seguridad multiempresa

Para usuarios que no son `SuperAdmin`, las consultas se restringen mediante:

```text
Proveedor.EmpresaId == usuario.EmpresaId
```

Esto aplica a:

- Listado.
- Detalle.
- Edición.
- Desactivación.

El servidor no debe confiar en IDs enviados desde la vista para permitir acceso a proveedores de otras empresas.

---

# 14. Reglas de negocio

1. Cada proveedor pertenece a una única empresa.
2. Un proveedor puede asociarse a múltiples compras.
3. La razón social es obligatoria.
4. El CUIT es opcional.
5. Cuando se informa CUIT debe normalizarse y validar su dígito verificador.
6. No puede existir otro proveedor activo con el mismo CUIT dentro de la misma empresa.
7. El mismo CUIT puede existir en distintas empresas.
8. Los campos opcionales vacíos se normalizan a `null`.
9. Un proveedor nuevo comienza activo.
10. La fecha de alta se genera en servidor.
11. La baja es lógica.
12. Un proveedor inactivo puede reactivarse desde edición.
13. La baja no modifica compras históricas.
14. Un `AdminEmpresa` sólo puede operar proveedores de su empresa.
15. La empresa propietaria del proveedor no se modifica actualmente desde el flujo de edición.

---

# 15. Casos de error relevantes

- Razón social vacía.
- Razón social demasiado extensa.
- CUIT con longitud inválida.
- CUIT con dígito verificador inválido.
- CUIT duplicado entre proveedores activos de la misma empresa.
- Email inválido.
- Campos que superan sus longitudes máximas.
- Empresa inexistente o inactiva durante creación.
- Proveedor inexistente.
- Intento de acceder a un proveedor de otra empresa.
- ID inconsistente durante edición.
- Intento de desactivar nuevamente un proveedor inactivo.

---

# 16. Integraciones actuales

El módulo se integra principalmente con:

- Empresa.
- Compra.
- Pago a proveedor.
- Reintegros/devoluciones vinculadas al circuito de compras.
- Reportes comerciales y de compras cuando corresponda.

---

# 17. Capacidades no implementadas

Actualmente no existen funcionalidades específicas de proveedor para:

- Cuenta corriente completa.
- Condiciones comerciales persistidas.
- Plazos de pago configurables.
- Contactos múltiples.
- Sitio web estructurado.
- Condición IVA estructurada.
- Evaluación o scoring.
- Ranking específico persistido.
- Catálogo de productos por proveedor.
- Órdenes de compra.
- Importación masiva específica de proveedores.
- Integración automática con proveedores externos.

---

# 18. Evolución futura

La evolución se gestiona mediante Roadmap y GitHub Issues.

Entre las mejoras posibles se encuentran:

- Historial consolidado de compras por proveedor.
- Comparación de costos entre proveedores.
- Órdenes de compra.
- Recepciones parciales.
- Condiciones y plazos de pago.
- Resumen financiero del proveedor.
- Sugerencias de compra.
- Evaluación de desempeño del proveedor.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 19. Estado

✅ CRUD administrativo implementado.

✅ Seguridad multiempresa implementada.

✅ Baja lógica y reactivación implementadas.

✅ Validación y normalización de CUIT implementadas.

✅ Búsqueda, filtros y paginación implementados.

✅ Integración con compras implementada.

🚧 Gestión avanzada de proveedores y abastecimiento reservada para evolución post-MVP.