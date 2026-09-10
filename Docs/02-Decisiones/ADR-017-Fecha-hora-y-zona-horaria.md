# ADR-017: Fecha, hora y zona horaria

## Estado

Aceptada.

## Contexto

La aplicación y SQL Server se ejecutan en producción con zona horaria UTC. Las
columnas operativas existentes son `datetime2` y no incluyen un desplazamiento.
Antes de esta decisión, las fechas se generaban con `DateTime.Now` y se mostraban
sin conversión, por lo que la interfaz exhibía la hora UTC como si fuera la hora
local de la empresa.

El diagnóstico realizado el 9 de septiembre de 2026 confirmó que `GETDATE()`,
`GETUTCDATE()` y `SYSDATETIMEOFFSET()` utilizan UTC en producción. Por ese motivo,
los valores históricos existentes se interpretan como UTC y no se modifican.

## Decisión

- Las marcas de tiempo persistidas por Veltika se generan en UTC.
- La obtención de la hora actual y las conversiones se centralizan mediante
  `IFechaHoraService`.
- La zona horaria inicial se configura como
  `America/Argentina/Buenos_Aires` en `appsettings.json`.
- Las fechas se convierten a la zona configurada al presentarlas al usuario.
- Los días seleccionados en filtros se interpretan como días locales de la
  empresa y sus límites se convierten a UTC antes de consultar la base de datos.
- Los nombres de archivos que incluyen fecha y hora utilizan la hora local.
- No se permiten ajustes dispersos como `AddHours(-3)`.

## Consecuencias

La hora visible deja de depender de la configuración de Windows, IIS o SQL
Server. Los registros históricos se conservan sin una migración de datos. La
abstracción permite reemplazar en el futuro la zona global por una configuración
por empresa sin reintroducir relojes directos en cada módulo.
