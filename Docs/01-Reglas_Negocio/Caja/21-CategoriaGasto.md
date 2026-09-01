# Módulo CategoriaGasto

Última actualización: 01/09/2026

---

# 1. Objetivo

CategoriaGasto permite clasificar egresos manuales de Caja.

No representa un movimiento financiero por sí sola.

Conceptualmente:

```text
CategoriaGasto
    -> clasificación administrativa

MovimientoCaja EgresoManual
    -> operación financiera real
```

Ejemplos posibles:

- Servicios.
- Limpieza.
- Mantenimiento.
- Viáticos.
- Insumos administrativos.

---

# 2. Modelo actual

`CategoriaGasto` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| Nombre | Obligatorio, máximo 100 caracteres |
| Descripcion | Opcional, máximo 250 caracteres |
| Estado | Activa/Inactiva |
| FechaAlta | Fecha de creación |
| EmpresaId | Empresa propietaria |

Relaciones actuales:

- Empresa.
- MovimientoCaja.

---

# 3. Autorización

`CategoriaGastoController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Además se aplican controles multiempresa dentro de las acciones.

---

# 4. Multiempresa

AdminEmpresa sólo puede consultar y administrar categorías de:

```text
usuario.EmpresaId
```

El `EmpresaId` enviado por la vista no se considera fuente confiable para AdminEmpresa.

En Create el controller lo reemplaza por el EmpresaId del Usuario.

---

# 5. SuperAdmin

SuperAdmin puede seleccionar una Empresa al crear y filtrar categorías por Empresa en Index.

La Empresa seleccionada debe existir y estar activa.

---

# 6. Index

El listado soporta actualmente:

```text
estado = activos | inactivos | todos
empresaId = filtro para SuperAdmin
busqueda = Nombre / Descripcion
pagina
```

El estado por defecto es:

```text
activos
```

---

# 7. Paginación

Index utiliza:

```text
20 registros por página
```

Las categorías se ordenan por Nombre.

---

# 8. Creación

Al crear se valida:

- Usuario autenticado.
- Empresa válida.
- Empresa activa.
- alcance multiempresa.
- Nombre obligatorio.
- Nombre máximo 100 caracteres.
- Descripcion máximo 250 caracteres.
- unicidad del Nombre dentro de la Empresa.

---

# 9. Normalización de datos

Antes de persistir:

```text
Nombre = Nombre.Trim()
```

Para Descripcion:

```text
vacía / espacios -> null
con contenido -> Trim()
```

---

# 10. Unicidad por Empresa

No puede existir otra CategoriaGasto con el mismo Nombre dentro de la misma Empresa.

La comparación actual se realiza de forma equivalente a case-insensitive mediante `ToLower()`.

La validación incluye categorías activas e inactivas.

---

# 11. Categoría inactiva duplicada

Si al crear ya existe una categoría inactiva con el mismo Nombre, no se crea un nuevo registro.

El sistema indica que debe reactivarse desde Edit.

Esto evita duplicación histórica.

---

# 12. Valores iniciales

Al crear correctamente:

```text
Estado = true
FechaAlta = DateTime.Now
EmpresaId = empresa validada
```

---

# 13. Edición

Edit permite modificar:

- Nombre.
- Descripcion.
- Estado.

No permite mover una CategoriaGasto de una Empresa a otra.

El `EmpresaId` se conserva desde el registro existente.

---

# 14. Unicidad al editar

Al modificar el Nombre se valida que no exista otra CategoriaGasto distinta con el mismo Nombre dentro de la misma Empresa.

---

# 15. Reactivación

Una CategoriaGasto inactiva puede reactivarse mediante Edit estableciendo:

```text
Estado = true
```

No se crea un nuevo registro para reemplazarla.

---

# 16. Desactivación

El flujo Delete no elimina físicamente la CategoriaGasto.

Realiza:

```text
Estado = false
```

Esto implementa Soft Delete funcional.

---

# 17. Categoría ya inactiva

Si se intenta desactivar una categoría que ya está inactiva, la operación se rechaza/informa sin modificarla nuevamente.

---

# 18. Conservación histórica

Las categorías no deben eliminarse físicamente porque pueden estar referenciadas por MovimientoCaja históricos.

Una desactivación sólo impide su uso operativo futuro cuando los formularios filtran categorías activas.

Los movimientos históricos conservan la referencia.

---

# 19. Relación con MovimientoCaja

`MovimientoCaja` posee:

```text
CategoriaGastoId
```

Esta relación se utiliza para clasificar egresos manuales.

No todos los MovimientoCaja requieren CategoriaGasto.

---

# 20. Uso en EgresoManual

El flujo `MovimientoCaja/EgresoManual` utiliza CategoriaGasto para identificar la naturaleza del gasto.

Conceptualmente:

```text
MovimientoCaja
Tipo = EgresoManual
Direccion = Egreso
CategoriaGastoId = categoría elegida
```

---

# 21. Operaciones financieras automáticas

Operaciones como:

- CobroVenta.
- PagoProveedor.
- ReintegroVenta.
- ReintegroProveedor.
- reversiones.

no utilizan una CategoriaGasto administrativa para clasificarse.

En esos movimientos `CategoriaGastoId` normalmente queda null porque el propio TipoMovimientoCaja ya expresa el origen de la operación.

---

# 22. Diferencia entre TipoMovimientoCaja y CategoriaGasto

No son equivalentes.

`TipoMovimientoCaja` identifica la naturaleza técnica/operativa del movimiento.

Ejemplo:

```text
EgresoManual
PagoProveedor
CobroVenta
ReintegroVenta
```

`CategoriaGasto` permite clasificar específicamente un gasto manual.

Ejemplo:

```text
TipoMovimientoCaja = EgresoManual
CategoriaGasto = Servicios
```

---

# 23. Estado y uso operativo

Las categorías inactivas se conservan para historial pero no deberían ofrecerse para nuevos egresos manuales.

Las consultas operativas deben trabajar con:

```text
Estado == true
```

---

# 24. Seguridad

Las reglas críticas se validan en backend:

- Usuario autenticado.
- autorización por Rol.
- Empresa correcta.
- Empresa activa al crear.
- EmpresaId impuesto por servidor para AdminEmpresa.
- recurso restringido por Empresa.
- unicidad por Empresa.
- Empresa del registro inmutable durante Edit.

---

# 25. Reglas de negocio actuales

1. CategoriaGasto pertenece a una Empresa.
2. Nombre es obligatorio.
3. Nombre posee máximo 100 caracteres.
4. Descripcion es opcional y posee máximo 250 caracteres.
5. Nombre se normaliza con Trim.
6. Descripcion vacía se persiste como null.
7. Nombre debe ser único dentro de la Empresa.
8. La unicidad incluye registros activos e inactivos.
9. Una categoría inactiva duplicada debe reactivarse en lugar de recrearse.
10. FechaAlta se asigna al crear.
11. Estado inicial es activo.
12. CategoriaGasto no puede cambiar de Empresa mediante Edit.
13. Puede reactivarse desde Edit.
14. Delete implementa desactivación lógica.
15. Los registros históricos no se eliminan físicamente.
16. CategoriaGasto clasifica principalmente MovimientoCaja de EgresoManual.
17. No todos los movimientos financieros utilizan CategoriaGasto.
18. TipoMovimientoCaja y CategoriaGasto cumplen responsabilidades diferentes.
19. AdminEmpresa sólo administra categorías de su Empresa.
20. SuperAdmin puede operar globalmente y filtrar por Empresa.

---

# 26. Evolución futura

Posibles mejoras futuras:

- categorías y subcategorías.
- presupuesto mensual por categoría.
- límites de gasto.
- reportes comparativos por categoría.
- categorías predeterminadas al crear una Empresa.
- centros de costo.
- asociación con futura Sucursal.
- reglas de aprobación para determinados gastos.
- permisos granulares para administrar categorías y registrar egresos.

No se consideran implementadas actualmente salvo lo indicado expresamente antes.

---

# 27. Estado actual

✅ CRUD administrativo implementado.

✅ Multiempresa implementado.

✅ Filtro por Estado implementado.

✅ Búsqueda implementada.

✅ Paginación implementada.

✅ Unicidad por Empresa implementada.

✅ Soft Delete implementado.

✅ Reactivación desde Edit implementada.

✅ Asociación con MovimientoCaja implementada.

✅ Uso en EgresoManual implementado.

🚧 Jerarquía de categorías pendiente.

🚧 Presupuestos/centros de costo pendientes.

🚧 Permisos granulares pendientes.