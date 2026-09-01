# Módulo Cliente

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Cliente administra las personas o entidades que pueden asociarse a ventas dentro de Veltika.

Cada cliente pertenece a una única empresa y su información debe mantenerse aislada de las demás empresas del SaaS.

La asociación de un cliente a una venta permite conservar información comercial e histórica, aunque el cliente sea desactivado posteriormente.

---

# 2. Alcance actual

Actualmente permite:

- Listar clientes.
- Buscar clientes.
- Filtrar por estado.
- Filtrar por empresa para `SuperAdmin`.
- Consultar detalle.
- Crear clientes.
- Editar clientes.
- Desactivar clientes mediante baja lógica.
- Reactivar clientes desde edición.
- Asociar clientes a ventas.

---

# 3. Actores y permisos

El módulo administrativo está protegido mediante:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto, el CRUD administrativo actual no está habilitado directamente para roles genéricos de vendedor o cajero.

## SuperAdmin

Puede:

- Consultar clientes de todas las empresas.
- Filtrar por empresa.
- Crear clientes para una empresa activa.
- Editar clientes de cualquier empresa.
- Cambiar la empresa asociada durante edición, sujeto a validaciones.
- Desactivar y reactivar clientes.

## AdminEmpresa

Puede:

- Consultar únicamente clientes de su empresa.
- Crear clientes para su empresa.
- Editar clientes de su empresa.
- Desactivar y reactivar clientes de su empresa.

Para `AdminEmpresa`, `EmpresaId` se obtiene del usuario autenticado y no se confía en el valor enviado desde el cliente.

---

# 4. Modelo actual

La entidad `Cliente` contiene:

| Campo | Tipo | Regla |
|---|---|---|
| Id | int | Identificador único |
| Nombre | string | Obligatorio, máximo 50 caracteres |
| Apellido | string? | Opcional, máximo 50 caracteres |
| Documento | string? | Opcional, máximo 20 caracteres |
| Email | string? | Opcional, formato email, máximo 100 caracteres |
| Telefono | string? | Opcional, formato teléfono, máximo 30 caracteres |
| Direccion | string? | Opcional, máximo 150 caracteres |
| Estado | bool | Activo o inactivo |
| FechaAlta | DateTime | Fecha de creación |
| EmpresaId | int | Empresa propietaria |

Relaciones:

- Empresa.
- Ventas.

---

# 5. Listado y búsqueda

El listado muestra clientes activos por defecto.

Permite filtrar por:

- Activos.
- Inactivos.
- Todos.
- Empresa para `SuperAdmin`.

La búsqueda permite coincidencias en:

- Nombre.
- Apellido.
- Documento.
- Email.

Los resultados se ordenan por nombre y luego apellido.

La paginación actual utiliza 20 registros por página.

Las consultas de listado utilizan `AsNoTracking()`.

---

# 6. Creación

La creación utiliza `ClienteCreateVM` para limitar y controlar los datos recibidos desde la vista.

Puede ingresarse:

- Nombre.
- Apellido.
- Documento.
- Email.
- Teléfono.
- Dirección.
- Empresa únicamente cuando opera un `SuperAdmin`.

El servidor asigna automáticamente:

```text
Estado = true
FechaAlta = DateTime.Now
```

Para usuarios que no son `SuperAdmin`:

```text
EmpresaId = usuario.EmpresaId
```

---

# 7. Normalización de datos

Antes de persistir, el controller normaliza los campos de texto.

- `Nombre` se guarda sin espacios externos.
- Apellido vacío se convierte en `null`.
- Documento vacío se convierte en `null`.
- Email vacío se convierte en `null`.
- Teléfono vacío se convierte en `null`.
- Dirección vacía se convierte en `null`.

Los valores opcionales informados se guardan utilizando `Trim()`.

---

# 8. Validaciones

## Nombre

- Obligatorio.
- Máximo 50 caracteres.

El apellido **no es obligatorio** en el modelo actual.

## Documento

- Opcional.
- Máximo 20 caracteres.
- Si se informa, no puede repetirse dentro de la misma empresa.

El mismo documento puede existir en empresas diferentes.

## Email

- Opcional.
- Debe tener formato válido cuando se informa.
- Máximo 100 caracteres.

Actualmente no existe una regla de unicidad de email por empresa.

## Teléfono

- Opcional.
- Debe cumplir la validación de teléfono cuando se informa.
- Máximo 30 caracteres.

## Dirección

- Opcional.
- Máximo 150 caracteres.

## Empresa

La empresa debe:

- Existir.
- Estar activa.

---

# 9. Edición y reactivación

La edición utiliza `ClienteEditVM`.

Pueden modificarse:

- Nombre.
- Apellido.
- Documento.
- Email.
- Teléfono.
- Dirección.
- Estado.

Para `SuperAdmin` también puede modificarse la empresa.

Para `AdminEmpresa`, la empresa permanece restringida a la del usuario autenticado.

La fecha de alta original no se modifica.

Una persona inactiva puede reactivarse desde edición estableciendo nuevamente:

```text
Estado = true
```

Antes de guardar se vuelven a validar empresa y documento duplicado.

---

# 10. Desactivación

La baja es lógica:

```text
Estado = false
```

El cliente no se elimina físicamente.

Si ya se encuentra inactivo, el controller rechaza una nueva solicitud de desactivación e informa la situación.

La baja lógica preserva las ventas históricas asociadas.

---

# 11. Relación con ventas

Un cliente puede estar asociado a múltiples ventas.

La desactivación del cliente no elimina ni modifica ventas históricas.

La disponibilidad de clientes para nuevas ventas debe respetar el estado y la empresa dentro del flujo de Venta/POS.

La existencia del módulo Cliente no implica que todas las ventas deban obligatoriamente poseer un cliente; esa regla corresponde al módulo Venta.

---

# 12. Seguridad multiempresa

Para usuarios que no son `SuperAdmin`, las consultas de listado, detalle, edición y desactivación se restringen por:

```text
Cliente.EmpresaId == usuario.EmpresaId
```

El `EmpresaId` enviado por formularios no es fuente confiable para `AdminEmpresa`.

Las validaciones de empresa se realizan en servidor.

Nunca debe permitirse acceder, modificar o asociar un cliente perteneciente a otra empresa.

---

# 13. Reglas de negocio

1. Cada cliente pertenece a una única empresa.
2. Un cliente puede estar asociado a múltiples ventas.
3. El nombre es obligatorio.
4. El apellido es opcional.
5. El documento es opcional.
6. Si se informa documento, debe ser único dentro de la empresa.
7. Email, teléfono y dirección son opcionales.
8. Una empresa nueva asociada al cliente debe existir y estar activa.
9. Un cliente nuevo comienza activo.
10. La fecha de alta se genera en servidor.
11. La baja es lógica.
12. La reactivación se realiza desde edición.
13. La baja del cliente no altera operaciones históricas.
14. Un `AdminEmpresa` sólo puede operar clientes de su empresa.
15. Los campos opcionales vacíos se normalizan a `null`.

---

# 14. Casos de error relevantes

- Nombre vacío.
- Nombre con más de 50 caracteres.
- Apellido con más de 50 caracteres.
- Documento con más de 20 caracteres.
- Documento duplicado dentro de la empresa.
- Email inválido.
- Email con más de 100 caracteres.
- Teléfono inválido.
- Teléfono con más de 30 caracteres.
- Dirección con más de 150 caracteres.
- Empresa inexistente o inactiva.
- Cliente inexistente.
- Intento de acceder a un cliente de otra empresa.
- ID inconsistente durante edición.
- Intento de desactivar nuevamente un cliente inactivo.
- Error de persistencia.

---

# 15. Integraciones actuales

El módulo se integra principalmente con:

- Empresa.
- Venta.
- POS.
- Dashboard.
- Reportes comerciales.

El historial comercial puede derivarse de las ventas asociadas, aunque actualmente no existe un módulo avanzado independiente de CRM o cuenta corriente.

---

# 16. Capacidades no implementadas

Actualmente Cliente no posee campos o módulos específicos para:

- CUIT separado del documento general.
- Razón social.
- Fecha de nacimiento.
- Localidad estructurada.
- Provincia.
- País.
- Código postal.
- Observaciones.
- Límite de crédito.
- Saldo de cuenta corriente.
- Programa de puntos.
- Descuentos personalizados por cliente.
- Etiquetas.
- Segmentación persistida.
- CRM externo.
- Importación masiva específica de clientes.

Tampoco existe actualmente un CRUD administrativo de clientes habilitado directamente para roles `Vendedor` o `Cajero`.

---

# 17. Evolución futura

La evolución se gestiona mediante Roadmap y GitHub Issues.

Entre las mejoras previstas o posibles se encuentran:

- Historial consolidado de compras.
- Total gastado.
- Ticket promedio.
- Última compra.
- Frecuencia de compra.
- Segmentación comercial.
- Cuenta corriente.
- Crédito y condiciones de pago.
- Alertas de vencimiento.
- Descuentos o condiciones comerciales.
- Fidelización.

No se mantiene un roadmap por versiones independiente dentro de este documento.

---

# 18. Estado

✅ CRUD administrativo implementado.

✅ Seguridad multiempresa implementada.

✅ Baja lógica y reactivación implementadas.

✅ Búsqueda, filtros y paginación implementados.

✅ Documento único por empresa cuando se informa.

✅ Integración con ventas implementada.

🚧 Cuenta corriente y funcionalidades CRM/comerciales avanzadas reservadas para evolución post-MVP.