# Convenciones de Desarrollo - Veltika

Versión: 2.0

Última actualización: 01/09/2026

## 1. Objetivo

Este documento establece criterios comunes para mantener Veltika consistente, legible, seguro y mantenible.

Las convenciones son una guía de diseño. No deben convertirse en reglas rígidas que obliguen a agregar capas o abstracciones sin una necesidad concreta.

## 2. Principios

Se prioriza:

- simplicidad;
- legibilidad;
- mantenibilidad;
- seguridad;
- consistencia;
- reutilización cuando aporta valor;
- reglas de negocio claras;
- evitar duplicación y deuda técnica;
- evitar sobrearquitectura.

Primero se entiende la regla de negocio y después se elige la implementación.

## 3. Arquitectura

Veltika utiliza ASP.NET Core MVC con Razor Views, Entity Framework Core, ASP.NET Core Identity y SQL Server.

### Controllers

Los Controllers coordinan la solicitud HTTP y pueden:

- validar entrada y `ModelState`;
- resolver usuario, rol y Empresa;
- aplicar autorización sobre el recurso;
- ejecutar consultas con Entity Framework Core;
- coordinar transacciones;
- invocar Services cuando una responsabilidad merece separación o reutilización;
- construir ViewModels y devolver respuestas.

Los Controllers no deben confiar en valores del navegador para decisiones críticas y deben evitar acumular lógica compleja que pueda aislarse claramente.

### Services

Los Services se incorporan cuando aportan valor concreto, por ejemplo:

- lógica reutilizada por varios Controllers;
- cálculos o reglas con responsabilidad propia;
- integraciones externas;
- procesos complejos que conviene aislar;
- operaciones que necesitan pruebas independientes.

No es obligatorio crear un Service para cada entidad ni envolver Entity Framework Core en una capa genérica sin necesidad.

### Entity Framework Core

EF Core es el mecanismo principal de acceso a datos. Se utiliza directamente cuando hacerlo mantiene el código claro y seguro.

Las operaciones críticas que modifican múltiples registros relacionados deben ser atómicas y utilizar transacciones cuando corresponda.

### Views

Las Views presentan información y comportamiento de interfaz. Pueden contener lógica de presentación simple, pero no deben decidir reglas de negocio ni autorización sobre recursos.

Las validaciones del navegador mejoran UX, pero nunca reemplazan la validación server-side.

## 4. Convenciones de nombres

- Clases y métodos: `PascalCase`.
- Variables locales y parámetros: `camelCase`.
- Claves primarias: normalmente `Id`.
- Claves foráneas: normalmente terminan en `Id`.
- Entidades: nombres de dominio en singular.

Los nombres deben describir intención de negocio y mantenerse consistentes con el vocabulario de Veltika.

## 5. Modelos y ViewModels

Los modelos de dominio representan información persistida y relaciones del negocio.

Se utilizan Data Annotations y/o Fluent API según corresponda.

Los ViewModels se utilizan cuando la entrada o salida de una pantalla no coincide limpiamente con una entidad, especialmente para:

- operaciones comerciales;
- formularios con múltiples entidades;
- búsquedas y resultados específicos;
- reducir riesgo de overposting;
- separar datos calculados o exclusivos de UI.

No es necesario crear un ViewModel trivial cuando no aporta una separación real y el binding utilizado es explícito y seguro.

## 6. Base de datos

- Motor: SQL Server.
- Persistencia: Entity Framework Core Code First.
- Los cambios de esquema se versionan mediante migraciones.
- Las migraciones ya aplicadas no deben reescribirse para alterar historia; los cambios nuevos deben realizarse con nuevas migraciones.
- Las migraciones de producción se aplican mediante el proceso de deploy documentado.

## 7. Validaciones y reglas críticas

Las validaciones simples pueden declararse con Data Annotations.

Las reglas de negocio se validan en servidor. Entre ellas:

- pertenencia a Empresa;
- existencia y estado del recurso;
- stock disponible;
- saldos de Caja;
- asociaciones entre Caja y MedioPago;
- turnos requeridos;
- importes máximos;
- precios, subtotales y totales;
- duplicados y restricciones del dominio.

Todo dato crítico recibido desde HTML, JavaScript, URL o formulario se considera entrada no confiable y debe verificarse nuevamente.

## 8. Seguridad multiempresa

La seguridad multiempresa es transversal.

- Los recursos deben validarse contra la Empresa permitida para el usuario.
- Un `EmpresaId` recibido desde el cliente no constituye autorización.
- `AdminEmpresa` opera dentro de su Empresa.
- `SuperAdmin` sólo obtiene alcance global donde la operación lo permite expresamente.
- Las consultas y escrituras deben evitar exposición cruzada entre Empresas.

## 9. Eliminación e historia

El Soft Delete se utiliza en módulos administrativos cuando corresponde; no es una regla universal para todas las entidades.

Los registros históricos/transaccionales deben preservar trazabilidad mediante estados, anulaciones, reversiones o movimientos compensatorios según el dominio.

No se debe eliminar físicamente información histórica crítica para simular una corrección.

## 10. Transacciones y concurrencia

Una operación que deba persistirse como una unidad debe ser atómica.

Las operaciones comerciales, de stock y de Caja que puedan competir por saldo, cantidad o estado deben revalidar dentro de la transacción. Cuando el caso lo requiere, Veltika utiliza `IsolationLevel.Serializable`.

No debe asumirse que una validación realizada antes de comenzar una operación crítica continúa siendo válida al momento de guardar.

## 11. Trazabilidad

Veltika conserva trazabilidad principalmente mediante los registros históricos del dominio: documentos comerciales, movimientos de stock, movimientos de Caja, turnos, devoluciones, reintegros y reversiones.

No existe actualmente una infraestructura genérica de auditoría para todas las acciones. No debe documentarse una funcionalidad como existente hasta que esté implementada.

## 12. Manejo de errores

- No mostrar excepciones técnicas al usuario final.
- Dar mensajes funcionales comprensibles.
- Registrar información técnica suficiente para diagnóstico cuando corresponda.
- No exponer secretos, cadenas de conexión ni credenciales.
- Ante errores en operaciones atómicas, realizar rollback y no dejar estados parciales.

## 13. Interfaz de usuario

La aplicación debe mantener:

- identidad visual consistente;
- navegación simple;
- formularios y controles reutilizables;
- mensajes claros;
- diseño responsive;
- separación entre la web pública y la aplicación autenticada mediante componentes/layouts apropiados.

CSS y JavaScript compartidos deben organizarse por responsabilidad y evitar duplicación innecesaria.

## 14. Git

Los cambios deben ser pequeños, comprensibles y con commits descriptivos.

Las funcionalidades pueden trabajarse por issue/branch/PR según el flujo utilizado en ese momento. Antes de integrar cambios se deben ejecutar las pruebas correspondientes y revisar que la documentación afectada continúe siendo válida.

## 15. Documentación

La documentación oficial se encuentra en `Docs/`:

- `00-Proyecto`: arquitectura, roadmap, infraestructura, deploy y convenciones.
- `01-Reglas_Negocio`: comportamiento funcional.
- `02-Decisiones`: ADR.

La documentación debe evolucionar con el código. Un documento obsoleto debe actualizarse, marcarse como histórico o eliminarse si no conserva valor documental.

No todo cambio necesita un ADR. Los ADR se reservan para decisiones estructurales que futuros desarrolladores necesiten comprender.

## 16. Filosofía de evolución

Veltika crece progresivamente. Una abstracción, integración o módulo nuevo debe incorporarse porque resuelve una necesidad concreta y no sólo porque podría resultar útil en el futuro.

Se prioriza una base sólida que pueda evolucionar sin introducir complejidad prematura.
