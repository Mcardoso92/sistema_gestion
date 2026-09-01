# 📚 Documentación Oficial - Veltika

Última actualización: 01/09/2026

Esta carpeta reúne la documentación funcional, técnica, operativa y arquitectónica de Veltika.

Su objetivo es servir como referencia durante el desarrollo, mantenimiento, despliegue y evolución del producto.

La documentación debe evolucionar junto con el código. Cuando exista una contradicción entre documentación antigua y comportamiento real, deberá revisarse el código vigente, las reglas de negocio actuales y las decisiones arquitectónicas antes de corregir el documento.

---

# 1. Estructura general

```text
Docs/
│
├── 00-Proyecto/
│   ├── ArquitecturaGeneral.md
│   ├── Configuracion de produccion.md
│   ├── Convenciones.md
│   ├── Guia de deploy Veltika.md
│   ├── Infraestructura Veltika.md
│   └── Roadmap.md
│
├── 01-Reglas_Negocio/
│   ├── Caja/
│   ├── Comercial/
│   ├── Configuracion/
│   ├── Core/
│   ├── Inventario/
│   ├── Reportes/
│   └── Seguridad/
│
├── 02-Decisiones/
│   └── ADR-*.md
│
└── README.md
```

> Las reglas de negocio fueron revisadas contra el estado real del código el 01/09/2026. La siguiente etapa de la auditoría documental es revisar los ADR y los archivos auxiliares que permanecen directamente dentro de `Docs/`.

---

# 2. 00-Proyecto

Contiene la documentación transversal del proyecto y del producto.

## Arquitectura General

`ArquitecturaGeneral.md`

Describe:

- Arquitectura MVC actual.
- Stack tecnológico.
- Arquitectura SaaS multiempresa.
- Responsabilidades de Views, Controllers y Services.
- Persistencia con Entity Framework Core.
- Seguridad.
- Inventario.
- Caja.
- Infraestructura productiva.
- Principios de evolución arquitectónica.

## Convenciones

`Convenciones.md`

Define criterios comunes de desarrollo para mantener consistencia en el código.

## Roadmap

`Roadmap.md`

Resume:

- Estado actual de Veltika.
- Etapas completadas.
- Camino hacia validación con usuarios reales.
- Evolución post-MVP.
- Evolución comercial del SaaS.

El roadmap no reemplaza los GitHub Issues.

## Infraestructura

`Infraestructura Veltika.md`

Documenta el entorno productivo actual:

- AWS.
- EC2.
- Windows Server.
- IIS.
- SQL Server Express.
- Dominio.
- Backups.
- Recuperación.
- Evolución futura de infraestructura.

## Configuración de producción

`Configuracion de produccion.md`

Documenta:

- Configuración por ambiente.
- Variables de entorno.
- Manejo de secretos.
- Reglas para despliegues productivos.

## Guía de deploy

`Guia de deploy Veltika.md`

Describe el procedimiento operativo para publicar Veltika en producción, incluyendo:

- Build y tests.
- Publicación Release.
- Migraciones idempotentes.
- Empaquetado.
- SHA256.
- Backups.
- Instalación.
- Smoke tests.
- Recuperación.

---

# 3. 01-Reglas_Negocio

Contiene la especificación funcional de los módulos del sistema.

Los documentos de esta carpeta explican **cómo debe comportarse Veltika desde el punto de vista del negocio**.

La implementación debe respetar estas reglas, pero la documentación también debe mantenerse sincronizada cuando las reglas cambian.

## Core

- `01-Empresa.md`
- `02-Categoría.md`
- `03-Producto.md`
- `07-Sucursal.md`

> `Sucursal` está documentada como concepto futuro. No se encuentra implementada actualmente en el MVP.

## Comercial

- `08-Cliente.md`
- `09-Proveedor.md`
- `10-Venta.md`
- `11-DetalleVenta.md`
- `12-Compra.md`
- `13-DetalleCompra.md`
- `14-CobroVenta.md`
- `15-PagoProveedor.md`
- `16-DevolucionCompra.md`
- `17-ReintegroProveedor.md`
- `18-ReintegroVenta.md`

Esta sección cubre tanto los comprobantes comerciales principales como sus operaciones financieras y de reversión relacionadas.

## Inventario

- `14-MovimientoStock.md`
- `15-Stock.md`
- `16-AjusteStock.md`
- `17-ProductoImportacion.md`
- `Reglas_Negocio_Stock_Veltika.md`

Incluye reglas de stock, trazabilidad, ajustes e importación masiva inicial de Productos mediante Excel.

## Caja

- `17-Caja.md`
- `18-MovimientoCaja.md`
- `19-TurnoCaja.md`
- `20-MedioPago.md`
- `21-CategoriaGasto.md`
- `22-TransferenciaCaja.md`

Esta sección documenta la estructura de Cajas, movimientos financieros, turnos, medios de pago, clasificación de gastos y transferencias internas entre Cajas.

## Seguridad

- `04-Usuario.md`
- `05-Roles.md`
- `06-Permisos.md`

La autorización actual combina ASP.NET Core Identity, Roles y validaciones server-side de Empresa/recurso. El modelo de permisos granulares configurables todavía no está implementado.

## Configuración

- `20-Configuracion.md`

Documenta la configuración actual de Empresa basada en `ConfiguracionEmpresa`.

## Reportes

- `21-Reportes.md`
- `22-Dashboard.md`

Documenta los reportes operativos existentes, exportaciones Excel y el Dashboard descriptivo actual.

---

# 4. Estructura recomendada para reglas de negocio

No todos los documentos necesitan exactamente la misma cantidad de secciones, pero deberían cubrir cuando corresponda:

- Objetivo.
- Alcance.
- Actores.
- Permisos.
- Funcionalidades.
- Datos relevantes.
- Validaciones.
- Reglas de negocio.
- Casos de uso.
- Casos de error.
- Flujo funcional.
- Integraciones con otros módulos.
- Trazabilidad.
- Seguridad multiempresa.
- Mejoras futuras.
- Decisiones arquitectónicas relacionadas.

No es necesario mantener un roadmap independiente dentro de cada documento si la evolución futura ya se gestiona mediante el Roadmap general y GitHub Issues.

---

# 5. 02-Decisiones

Esta carpeta contiene los **Architecture Decision Records (ADR)** de Veltika.

Los ADR registran decisiones importantes y, principalmente, el motivo por el cual fueron tomadas.

Actualmente existen:

1. `ADR-001-Arquitectura-SaaS-Multiempresa.md`
2. `ADR-002-Soft-Delete.md`
3. `ADR-003-Precio-Historico.md`
4. `ADR-004-Venta-Inmutable.md`
5. `ADR-005-Cliente-Opcional.md`
6. `ADR-006-Seguridad-Multiempresa.md`
7. `ADR-007-Reglas-en-Servidor.md`
8. `ADR-008-Control-de-Stock.md`
9. `ADR-009-ViewModels.md`
10. `ADR-010-Transacciones-en-Ventas.md`
11. `ADR-011-AWS.md`
12. `ADR-012-SQLServer.md`
13. `ADR-013-CodeFirst.md`
14. `ADR-014-MultiEmpresa.md`
15. `ADR-015-SoftDelete.md`
16. `ADR-016-Identity.md`

Durante la revisión documental deberá evaluarse la duplicación conceptual entre algunos ADR, particularmente:

- ADR-001 y ADR-014 sobre multiempresa.
- ADR-002 y ADR-015 sobre Soft Delete.

No deben eliminarse automáticamente: primero debe revisarse si representan decisiones distintas, una evolución de la decisión anterior o simplemente documentación duplicada.

---

# 6. Cuándo crear un ADR

Debe considerarse un ADR cuando una decisión:

- Afecta a múltiples módulos.
- Cambia una regla arquitectónica importante.
- Define una tecnología estructural.
- Introduce una restricción que futuros desarrolladores deben conocer.
- Tiene alternativas razonables y se necesita conservar por qué se eligió una.

Ejemplos:

- Estrategia multiempresa.
- Soft Delete.
- Autenticación con Identity.
- Proveedor cloud.
- Estrategia de persistencia.
- Diseño de transacciones.
- Almacenamiento de archivos.
- Separación entre web pública y aplicación autenticada, si se considera una decisión arquitectónica estable.

Un ADR no debe utilizarse para registrar pequeños cambios de implementación.

---

# 7. Fuentes de verdad

La documentación se organiza según el tipo de información:

## Comportamiento actual

Fuente principal:

- Código en `main`.
- Tests.
- Base de reglas de negocio actualizada.

## Reglas de negocio

Fuente principal:

- `Docs/01-Reglas_Negocio/`.

## Decisiones arquitectónicas

Fuente principal:

- `Docs/02-Decisiones/`.

## Estado y dirección del producto

Fuente principal:

- `Roadmap.md`.
- GitHub Issues.

## Infraestructura y operación

Fuente principal:

- `Infraestructura Veltika.md`.
- `Configuracion de produccion.md`.
- `Guia de deploy Veltika.md`.

Cuando dos fuentes se contradigan, la discrepancia debe resolverse explícitamente; no se debe asumir automáticamente que el documento más antiguo sigue vigente.

---

# 8. Filosofía de desarrollo

Veltika se desarrolla siguiendo estos principios:

- Código limpio y mantenible.
- Seguridad primero.
- Reglas de negocio antes que implementación.
- Consistencia entre módulos.
- Evitar duplicación.
- Evitar deuda técnica innecesaria.
- Refactorizar cuando exista una mejora clara.
- Evitar sobrearquitectura.
- Seguridad multiempresa como requisito transversal.
- Evolución guiada por necesidades reales.
- Documentación viva junto al código.

---

# 9. Criterio de finalización de un Issue

Un Issue funcional se considera finalizado cuando, según corresponda:

- La funcionalidad está implementada.
- Se validaron sus reglas de negocio.
- Se revisó seguridad y aislamiento multiempresa.
- Se realizaron pruebas adecuadas.
- Se revisaron errores y casos límite relevantes.
- La experiencia de usuario es consistente con el resto del sistema.
- La documentación afectada fue actualizada.
- Las decisiones arquitectónicas relevantes fueron registradas.

No todos los cambios requieren un ADR, pero todo cambio que invalide documentación existente debe actualizarla.

---

# 10. Estado actual de la documentación

Al 01/09/2026:

✅ Arquitectura general actualizada.

✅ Roadmap actualizado al estado real del proyecto.

✅ Infraestructura productiva actualizada.

✅ Guía de deploy disponible.

✅ Configuración de producción documentada.

✅ Reglas de negocio antiguas revisadas contra el código actual.

✅ Documentación funcional agregada para TurnoCaja, MedioPago, CategoriaGasto, TransferenciaCaja, CobroVenta, PagoProveedor, DevolucionCompra, ReintegroProveedor, ReintegroVenta y ProductoImportacion.

✅ Índice funcional sincronizado con la documentación vigente.

🚧 ADR pendientes de revisión para detectar duplicados, decisiones obsoletas y decisiones faltantes.

🚧 Existen archivos `.cs` auxiliares dentro de `Docs/` que deben compararse con el código actual y eliminarse si ya no cumplen una función documental.

---

# 11. Criterio de mantenimiento

La carpeta `Docs/` no debe convertirse en un archivo histórico de documentación obsoleta presentada como vigente.

Cuando un documento deje de representar el comportamiento real deberá:

1. Actualizarse, si la regla sigue existiendo pero cambió.
2. Marcarse explícitamente como histórico, si existe una razón para conservarlo.
3. Eliminarse, si sólo representa una copia vieja sin valor documental.

La documentación debe ayudar a entender Veltika tal como funciona hoy y por qué fue construido de esa manera.