# Módulo Permisos

Última actualización: 01/09/2026

---

# 1. Objetivo

El concepto de Permisos representa la futura capacidad de controlar acciones específicas dentro de Veltika con mayor granularidad que los roles actuales.

Ejemplos conceptuales:

```text
Ventas.Ver
Ventas.Crear
Ventas.Anular
Caja.Ver
Caja.Operar
Productos.Editar
Reportes.Ver
```

Sin embargo, este sistema granular todavía no está implementado productivamente.

La seguridad actual se basa principalmente en:

- ASP.NET Core Identity.
- Roles.
- atributos `Authorize`.
- validaciones adicionales en servidor.
- aislamiento por `EmpresaId`.

---

# 2. Estado actual

Actualmente NO existe en Veltika:

- entidad `Permiso`;
- tabla propia de Permisos;
- `PermisoController`;
- CRUD de Permisos;
- relación persistida Rol-Permiso;
- relación persistida Usuario-Permiso;
- activación/desactivación de Permisos;
- catálogo configurable de Permisos;
- interfaz para asignar Permisos a empleados.

Por lo tanto, la documentación anterior que describía esas funcionalidades como actuales no correspondía al estado real del código.

---

# 3. Autorización vigente

La autorización actual utiliza principalmente expresiones como:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

También existen acciones con reglas más restrictivas, por ejemplo operaciones habilitadas únicamente para:

```text
AdminEmpresa
```

Además de los atributos de rol, distintos controllers verifican en servidor:

- Usuario autenticado.
- Rol actual.
- EmpresaId.
- pertenencia del recurso a la Empresa.
- Estado de entidades relacionadas.
- reglas específicas de negocio.

---

# 4. Rol actual de SuperAdmin

SuperAdmin posee actualmente acceso global según las reglas definidas por cada controller.

No depende de una lista individual de Permisos persistidos.

Cuando un módulo necesita comportamiento global suele distinguir explícitamente este rol mediante:

```text
UserManager.IsInRoleAsync(usuario, "SuperAdmin")
```

---

# 5. Rol actual de AdminEmpresa

AdminEmpresa posee actualmente acceso administrativo amplio dentro de su propia Empresa según los controllers habilitados.

El aislamiento empresarial no depende solamente del rol.

También se controla mediante:

```text
Usuario.EmpresaId
```

y verificaciones sobre las entidades consultadas o modificadas.

---

# 6. Rol Empleado

El rol `Empleado` existe actualmente en ASP.NET Core Identity.

Sin embargo, todavía no posee un sistema configurable de Permisos individuales.

La visión prevista es que en el futuro dos empleados de una misma Empresa puedan poseer capacidades diferentes sin necesidad de crear un rol rígido distinto para cada combinación.

Esta funcionalidad está definida como evolución post-MVP.

---

# 7. Issue de evolución

La evolución de Permisos granulares está registrada en GitHub mediante:

```text
Issue #37 - Sistema de permisos configurables para empleados
```

Su objetivo es permitir que cada AdminEmpresa determine qué módulos y acciones puede utilizar cada Usuario con rol Empleado dentro de su propia Empresa.

El issue se encuentra actualmente abierto y con prioridad Post-MVP.

---

# 8. Modelo de acceso objetivo

El modelo futuro planteado es:

```text
SuperAdmin
    acceso global

AdminEmpresa
    acceso completo dentro de su Empresa

Empleado
    acceso únicamente a acciones habilitadas
```

SuperAdmin y AdminEmpresa no deberían depender necesariamente de Permisos individuales para conservar sus alcances base.

---

# 9. Catálogo futuro de Permisos

Antes de implementar debe definirse un catálogo real de Permisos basado en reglas de negocio existentes.

Como referencia inicial se evalúan nombres como:

```text
Ventas.Ver
Ventas.Crear
Ventas.Anular
Caja.Ver
Caja.Operar
Productos.Ver
Productos.Editar
Clientes.Ver
Clientes.Editar
Compras.Ver
Compras.Registrar
Proveedores.Ver
Proveedores.Editar
Reportes.Ver
```

Esta lista es únicamente orientativa.

No debe implementarse automáticamente sin revisar cada módulo y separar correctamente acciones sensibles.

---

# 10. Granularidad

Un Permiso debería representar una capacidad concreta y comprensible.

Debe evitarse tanto:

```text
Permisos demasiado amplios
```

como:

```text
Permisos excesivamente pequeños y difíciles de administrar
```

Ejemplo:

`Ventas.Ver` y `Ventas.Anular` pueden tener sentido separados porque una anulación tiene mayor impacto operativo.

---

# 11. Backend como fuente de seguridad

Una futura implementación debe validar Permisos en servidor.

Ocultar botones o elementos del menú NO es suficiente.

Un Empleado sin autorización debe continuar bloqueado si intenta acceder directamente mediante:

- URL.
- formulario manual.
- solicitud HTTP.
- endpoint interno.

La interfaz debe reflejar los Permisos efectivos, pero nunca constituir la única barrera de seguridad.

---

# 12. Aislamiento multiempresa

Los Permisos futuros nunca deben permitir superar el aislamiento por Empresa.

Aunque un Empleado posea un Permiso como:

```text
Ventas.Ver
```

sólo podrá consultar Ventas correspondientes a:

```text
usuario.EmpresaId
```

El Permiso autoriza una acción; no cambia el tenant del Usuario.

---

# 13. Escalación de privilegios

Un Usuario Empleado nunca deberá poder mediante Permisos configurables:

- convertirse en AdminEmpresa;
- convertirse en SuperAdmin;
- asignarse Permisos a sí mismo;
- administrar usuarios con privilegios superiores;
- otorgarse acceso a otra Empresa;
- modificar configuraciones sensibles reservadas al administrador;
- acceder a acciones que no posee directamente por URL.

---

# 14. Administración futura

La visión registrada actualmente indica que AdminEmpresa podrá administrar únicamente Permisos de empleados pertenecientes a su propia Empresa.

Debe impedirse en backend:

```text
AdminEmpresa de Empresa A
    -> modificar Permisos de Usuario de Empresa B
```

La comparación de Empresa debe realizarse antes de cualquier modificación.

---

# 15. Autoasignación

Un Empleado no deberá poder modificar sus propios Permisos.

Asimismo, si la administración de Permisos se realiza desde el módulo Usuario, el backend deberá ignorar o rechazar cualquier intento de modificar datos fuera del alcance autorizado.

---

# 16. Navegación futura

Cuando se implemente el sistema granular, la interfaz deberá adaptar:

- menú lateral;
- accesos rápidos;
- botones de acción;
- enlaces;
- acciones sensibles.

según los Permisos efectivos.

Esta adaptación será una mejora de UX y claridad, no el mecanismo principal de seguridad.

---

# 17. Modelo de datos

El modelo definitivo todavía no está decidido.

Una posible arquitectura podría utilizar conceptos como:

```text
Permiso
UsuarioPermiso
```

o alternativamente:

```text
Permiso
Perfil/Rol personalizado
PerfilPermiso
UsuarioPerfil
```

La decisión debe tomarse cuando se implemente #37 considerando facilidad de administración, mantenimiento, performance y extensibilidad.

---

# 18. Permisos por Usuario vs por Rol

El objetivo actual de #37 prioriza que dos Usuarios con rol Empleado puedan tener Permisos diferentes.

Por lo tanto, un modelo exclusivamente Rol-Permiso con un único rol global `Empleado` no sería suficiente por sí solo.

Podría resolverse mediante:

- Permisos directos por Usuario;
- perfiles empresariales personalizados;
- combinación de perfil + excepciones;
- otra estrategia equivalente.

No está definido todavía cuál será la implementación final.

---

# 19. Roles personalizados

Los Roles actuales son globales de Identity.

En el futuro podría evaluarse un concepto distinto de perfil empresarial configurable, pero no debe mezclarse automáticamente con `IdentityRole` sin analizar sus implicancias multiempresa.

Un perfil como:

```text
Cajero
Vendedor
Encargado
```

podría ser simplemente una plantilla de Permisos dentro de una Empresa.

---

# 20. Permisos sensibles

Al diseñar el catálogo futuro deberían analizarse especialmente acciones con impacto económico o de seguridad, por ejemplo:

- Anular Ventas.
- Ajustar Stock.
- Ver costos.
- Editar precios.
- Registrar Compras.
- Anular Compras.
- Abrir/cerrar Caja.
- Registrar movimientos manuales.
- Gestionar Usuarios.
- Modificar Configuración.
- Ver reportes financieros.

No todas deberían quedar agrupadas bajo un único permiso genérico de módulo.

---

# 21. SuperAdmin

SuperAdmin debe conservar acceso global independientemente del futuro sistema de Permisos empresariales.

Los Permisos configurables para empleados no deben limitar accidentalmente funciones de administración global de Veltika.

---

# 22. AdminEmpresa

La visión actual establece que AdminEmpresa mantiene acceso completo dentro de su Empresa.

Por lo tanto, el sistema de Permisos individuales se orienta inicialmente a Usuarios Empleado.

Si en el futuro se requiere limitar también administradores, deberá tratarse como una decisión de producto y arquitectura separada.

---

# 23. Estado de Usuario

Los Permisos no reemplazan las reglas actuales de autenticación.

Un Usuario con:

```text
Estado == false
```

continúa sin poder autenticarse aunque tuviera Permisos almacenados.

Del mismo modo, las validaciones de Empresa activa y seguridad de Identity siguen teniendo prioridad.

---

# 24. Sucursales

No existen actualmente Permisos por Sucursal porque el dominio Sucursal no está implementado productivamente.

En una arquitectura futura podría existir alcance adicional, por ejemplo:

```text
Usuario puede operar Caja
pero sólo en Sucursal X
```

Esto requiere modelar primero Sucursal/Depósito y después definir cómo se combina el alcance de datos con los Permisos.

---

# 25. Permisos temporales

No existen actualmente:

- Permisos con fecha de inicio.
- Permisos con fecha de vencimiento.
- elevación temporal de privilegios.
- aprobaciones temporales.

No forman parte del alcance inicial de #37.

---

# 26. Auditoría futura

Cuando existan Permisos configurables será recomendable registrar cambios sensibles como:

```text
Usuario afectado
Permiso
Valor anterior
Valor nuevo
Administrador responsable
Fecha
```

La auditoría resulta especialmente importante para investigar escalaciones de privilegios o cambios accidentales.

Actualmente esta auditoría específica de Permisos no existe.

---

# 27. Pruebas requeridas

La futura implementación debe incluir pruebas de seguridad sobre al menos:

1. Empleado con Permiso permitido.
2. Empleado sin Permiso.
3. acceso directo a URL sin Permiso.
4. dos empleados de misma Empresa con configuraciones diferentes.
5. intento de acceso a datos de otra Empresa.
6. intento de autoasignación.
7. intento de escalación de rol.
8. AdminEmpresa intentando modificar otra Empresa.
9. comportamiento de SuperAdmin.
10. comportamiento de AdminEmpresa.

---

# 28. Criterios de aceptación futuros

El issue #37 establece como objetivos:

- Dos empleados de la misma Empresa pueden tener Permisos diferentes.
- AdminEmpresa administra únicamente empleados de su Empresa.
- El backend bloquea acciones no autorizadas.
- La navegación refleja los Permisos efectivos.
- El sistema mantiene aislamiento multiempresa.
- SuperAdmin conserva acceso global.
- AdminEmpresa conserva acceso completo en su Empresa.

Actualmente estos criterios continúan pendientes de implementación.

---

# 29. Lo que no debe hacerse

Al implementar Permisos debe evitarse:

- confiar sólo en Razor/JavaScript;
- crear un rol diferente por cada combinación posible;
- duplicar autorización en decenas de formas inconsistentes;
- permitir que el cliente envíe EmpresaId libremente;
- mezclar Permisos con aislamiento multiempresa;
- otorgar acceso por defecto ante ausencia de configuración;
- implementar una lista de Permisos sin revisar reglas de negocio.

---

# 30. Relación con módulos actuales

Los Permisos futuros podrán aplicarse progresivamente sobre:

- Ventas.
- Compras.
- Productos.
- Stock.
- Clientes.
- Proveedores.
- Caja.
- Reportes.
- Usuarios.
- Configuración.

La granularidad debe definirse módulo por módulo.

---

# 31. Regla de transición

Hasta que el sistema granular esté implementado, Veltika debe mantener el modelo de seguridad vigente basado en Roles y EmpresaId.

No debe introducirse lógica parcial de Permisos que deje controllers inconsistentes entre sí.

La migración futura debe realizarse de forma controlada, manteniendo compatibilidad con las restricciones actuales.

---

# 32. Reglas de negocio actuales

1. No existe una entidad Permiso implementada.
2. No existe CRUD de Permisos.
3. No existe relación Rol-Permiso persistida.
4. No existe relación Usuario-Permiso persistida.
5. La autorización actual se basa principalmente en roles de Identity.
6. El aislamiento multiempresa se controla además mediante EmpresaId.
7. SuperAdmin conserva acceso global según los módulos autorizados.
8. AdminEmpresa opera dentro de su Empresa.
9. Empleado existe como rol pero todavía no posee Permisos configurables.
10. Las validaciones críticas se realizan en backend.
11. Ocultar elementos de la UI no constituye autorización suficiente.
12. El sistema granular está planificado en issue #37.
13. #37 es una evolución Post-MVP.
14. El catálogo definitivo de Permisos todavía debe definirse.
15. No existen Permisos por Sucursal ni temporales.
16. No existe auditoría específica de modificaciones de Permisos.

---

# 33. Estado actual

✅ Autorización mediante ASP.NET Core Identity implementada.

✅ Roles SuperAdmin/AdminEmpresa/Empleado disponibles.

✅ Restricciones por rol en controllers implementadas.

✅ Aislamiento multiempresa mediante EmpresaId implementado en los módulos revisados.

✅ Validaciones de seguridad en backend presentes en los flujos actuales.

🚧 Catálogo de Permisos pendiente.

🚧 Modelo persistente de Permisos pendiente.

🚧 Administración de Permisos por AdminEmpresa pendiente.

🚧 Permisos individuales para Empleados pendientes.

🚧 Adaptación de navegación según Permisos pendiente.

🚧 Pruebas específicas del sistema granular pendientes.

🚧 Auditoría de cambios de Permisos pendiente.

❌ CRUD de Permisos no existe actualmente.

❌ Asociación Rol-Permiso configurable no existe actualmente.

❌ Permisos por Sucursal no existen actualmente.

---

# 34. Referencia funcional

La evolución de este módulo está centralizada actualmente en:

```text
GitHub Issue #37
Sistema de permisos configurables para empleados
```

El issue debe revisarse junto con las reglas reales de cada módulo antes de comenzar la implementación.