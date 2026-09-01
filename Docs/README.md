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

> La documentación fue auditada globalmente contra el estado del repositorio el 01/09/2026. Los documentos deben seguir revisándose cada vez que una modificación funcional o arquitectónica cambie el comportamiento que describen.

---

# 2. 00-Proyecto

Contiene la documentación transversal del proyecto y del producto.

## Arquitectura General

`ArquitecturaGeneral.md`

Describe la arquitectura MVC, stack tecnológico, SaaS multiempresa, responsabilidades, persistencia, seguridad, inventario, Caja, infraestructura productiva y principios de evolución arquitectónica.

## Convenciones

`Convenciones.md`

Define criterios comunes de desarrollo alineados con la arquitectura actual. Services y ViewModels se utilizan cuando aportan separación, seguridad o reutilización; no se fuerzan abstracciones sin necesidad.

## Roadmap

`Roadmap.md`

Resume el estado actual, camino hacia validación con usuarios reales y evolución post-MVP/comercial. El Roadmap no reemplaza los GitHub Issues.

## Infraestructura

`Infraestructura Veltika.md`

Documenta AWS, EC2, Windows Server, IIS, SQL Server Express, dominio, backups, recuperación y evolución futura.

## Configuración de producción

`Configuracion de produccion.md`

Documenta configuración por ambiente, variables, secretos y reglas productivas.

## Guía de deploy

`Guia de deploy Veltika.md`

Documenta build/tests, Release, migraciones idempotentes, empaquetado, SHA256, backups, instalación, smoke tests y recuperación.

---

# 3. 01-Reglas_Negocio

Contiene la especificación funcional de los módulos. Explica cómo debe comportarse Veltika desde el punto de vista del negocio y debe mantenerse sincronizada con la implementación.

## Core

- `01-Empresa.md`
- `02-Categoría.md`
- `03-Producto.md`
- `07-Sucursal.md`

> `Sucursal` está documentada como concepto futuro y no está implementada actualmente en el MVP.

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

## Inventario

- `14-MovimientoStock.md`
- `15-Stock.md`
- `16-AjusteStock.md`
- `17-ProductoImportacion.md`
- `Reglas_Negocio_Stock_Veltika.md`

## Caja

- `17-Caja.md`
- `18-MovimientoCaja.md`
- `19-TurnoCaja.md`
- `20-MedioPago.md`
- `21-CategoriaGasto.md`
- `22-TransferenciaCaja.md`

## Seguridad

- `04-Usuario.md`
- `05-Roles.md`
- `06-Permisos.md`
- `19-Auditoría.md`

La autorización actual combina ASP.NET Core Identity, Roles y validaciones server-side de Empresa/recurso. Los permisos granulares configurables todavía no están implementados. `19-Auditoría.md` documenta el estado real de trazabilidad y deja explícito que actualmente no existe un módulo genérico de Auditoría.

## Configuración

- `20-Configuracion.md`

Documenta la configuración actual de Empresa basada en `ConfiguracionEmpresa`.

## Reportes

- `21-Reportes.md`
- `22-Dashboard.md`

Documenta los reportes operativos existentes, exportaciones Excel y Dashboard descriptivo actual.

---

# 4. Criterios para reglas de negocio

Según corresponda, los documentos deberían cubrir objetivo, alcance, actores, permisos, funcionalidades, datos, validaciones, reglas, casos de uso/error, flujo, integraciones, trazabilidad, seguridad multiempresa, mejoras futuras y ADR relacionados.

No es necesario duplicar un roadmap dentro de cada módulo si la evolución futura ya se gestiona mediante Roadmap e Issues.

---

# 5. 02-Decisiones

Los Architecture Decision Records registran decisiones estructurales y el motivo por el cual fueron tomadas.

ADR vigentes:

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
16. `ADR-016-Identity.md`

ADR históricos/supersedidos conservados intencionalmente:

- `ADR-014-MultiEmpresa.md` → supersedido por ADR-001.
- `ADR-015-SoftDelete.md` → supersedido por ADR-002.

---

# 6. Cuándo crear un ADR

Debe considerarse cuando una decisión afecta múltiples módulos, define tecnología estructural, introduce una restricción importante o tiene alternativas razonables cuyo motivo de elección conviene preservar.

Ejemplos: estrategia multiempresa, Soft Delete, Identity, proveedor cloud, persistencia, transacciones, almacenamiento de archivos o separación pública/autenticada si se consolida como decisión arquitectónica estable.

Un ADR no debe registrar pequeños cambios de implementación.

---

# 7. Fuentes de verdad

## Comportamiento actual

- Código en `main`.
- Tests.
- Reglas de negocio vigentes.

## Reglas de negocio

- `Docs/01-Reglas_Negocio/`.

## Decisiones arquitectónicas

- `Docs/02-Decisiones/`.

## Estado y dirección del producto

- `Roadmap.md`.
- GitHub Issues.

## Infraestructura y operación

- `Infraestructura Veltika.md`.
- `Configuracion de produccion.md`.
- `Guia de deploy Veltika.md`.

Cuando dos fuentes se contradigan, la discrepancia debe resolverse explícitamente.

---

# 8. Filosofía de desarrollo

Veltika se desarrolla priorizando código limpio, seguridad, reglas de negocio, consistencia, baja duplicación, deuda técnica controlada, refactorización con propósito, seguridad multiempresa, evolución guiada por necesidades reales y ausencia de sobrearquitectura.

---

# 9. Criterio de finalización de un Issue

Según corresponda, un Issue funcional se considera finalizado cuando la funcionalidad está implementada, sus reglas y seguridad fueron validadas, existen pruebas adecuadas, se revisaron casos límite, la UX es consistente y la documentación afectada fue actualizada.

No todos los cambios requieren un ADR, pero todo cambio que invalide documentación existente debe actualizarla.

---

# 10. Estado actual de la documentación

Al 01/09/2026:

✅ Arquitectura general revisada.

✅ Roadmap alineado con el estado real del proyecto.

✅ Infraestructura productiva y proceso de deploy documentados.

✅ Reglas de negocio existentes revisadas contra el código.

✅ Documentación funcional incorporada para los módulos recientes.

✅ Índice funcional sincronizado.

✅ ADR revisados y actualizados.

✅ ADR duplicados consolidados como decisiones supersedidas.

✅ Copias antiguas de código eliminadas de `Docs/`.

✅ Convenciones alineadas con la arquitectura actual.

✅ Documento de Auditoría corregido para no presentar funcionalidades inexistentes como implementadas.

**La auditoría documental global queda completada al 01/09/2026.**

Esto no significa que la documentación quede congelada: debe actualizarse junto con cada cambio funcional, arquitectónico u operativo relevante.

---

# 11. Criterio de mantenimiento

`Docs/` no debe convertirse en un archivo de documentación obsoleta presentada como vigente.

Cuando un documento deje de representar el comportamiento real deberá actualizarse, marcarse explícitamente como histórico o eliminarse si sólo representa una copia vieja sin valor documental.

La documentación debe ayudar a entender Veltika tal como funciona hoy y por qué fue construido de esa manera.
