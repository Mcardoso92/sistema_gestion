# Auditoría y trazabilidad - estado actual

Última revisión: 01/09/2026

## Estado

**No existe actualmente un módulo genérico de Auditoría implementado en Veltika.**

Este documento se conserva para distinguir la trazabilidad que ya existe en el sistema de una futura auditoría transversal de acciones de usuario.

## Trazabilidad existente

Veltika ya conserva trazabilidad funcional mediante los propios registros históricos del dominio. Entre otros ejemplos:

- Ventas y Compras conservan sus documentos y detalles históricos.
- Cobros y pagos se preservan y sus correcciones se realizan mediante anulaciones/reversiones.
- Reintegros y devoluciones mantienen el registro original y los movimientos compensatorios correspondientes.
- `MovimientoStock` registra los cambios relevantes de inventario.
- `MovimientoCaja` registra ingresos, egresos y reversiones financieras.
- `TurnoCaja` conserva apertura, cierre, usuario, importes esperados/contados y diferencias.
- Las operaciones históricas críticas no se corrigen mediante eliminación física del registro original.

Esta trazabilidad de dominio es parte del comportamiento actual del sistema y no debe confundirse con una tabla central de auditoría.

## Lo que NO está implementado

Actualmente no existe una entidad/tabla genérica `Auditoria` que registre automáticamente para toda la aplicación datos como:

- Empresa.
- Usuario.
- Módulo.
- Acción.
- Registro afectado.
- Fecha/hora.
- Dirección IP.
- Valor anterior/nuevo.

Tampoco existe actualmente:

- `AuditoriaController`.
- Pantalla central de consulta de auditorías.
- Rol `Auditor`.
- Exportación de auditorías.
- Integración SIEM.
- Firma digital de eventos.

Por lo tanto, estas capacidades no deben presentarse como funcionalidades actuales de Veltika.

## Decisión actual

Para el MVP se prioriza la trazabilidad específica de cada dominio y la conservación de los registros históricos críticos.

No se incorporará una infraestructura genérica de auditoría únicamente por anticipación. Si durante la validación real aparece una necesidad concreta de registrar acciones administrativas transversales, deberá diseñarse una solución que:

- respete el aislamiento multiempresa;
- diferencie eventos técnicos de eventos funcionales;
- no duplique innecesariamente la información ya registrada en movimientos y documentos históricos;
- permita determinar usuario, recurso y acción cuando resulte necesario;
- mantenga los eventos de auditoría inmutables;
- contemple crecimiento de volumen y políticas de retención.

## Casos que podrían justificar una auditoría transversal futura

- Cambios administrativos sensibles.
- Modificación de configuraciones de Empresa.
- Gestión de usuarios y roles.
- Activaciones y desactivaciones relevantes.
- Accesos o acciones de SuperAdmin sobre Empresas.
- Operaciones cuya trazabilidad no quede suficientemente representada por los registros del dominio.

## Relación con otras decisiones

La auditoría futura deberá respetar:

- seguridad multiempresa;
- reglas críticas ejecutadas en servidor;
- inmutabilidad de operaciones históricas;
- Identity para identificar al usuario autenticado;
- principio de evitar sobrearquitectura antes de validar una necesidad real.
