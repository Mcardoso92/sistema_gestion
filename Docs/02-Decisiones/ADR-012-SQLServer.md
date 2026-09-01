# ADR-012 - SQL Server como motor de base de datos

## Estado
Aceptado

## Contexto

Veltika necesita persistencia relacional y transaccional para mantener consistencia entre Empresas, usuarios, operaciones comerciales, Stock y Caja.

## Decisión

Se utilizará Microsoft SQL Server como motor relacional de Veltika.

La aplicación accede a la base mediante Entity Framework Core y la evolución del esquema se gestiona con migraciones Code First según ADR-013.

La infraestructura productiva actual utiliza SQL Server Express con la base `Veltika_DB`.

## Motivos

- Integración madura con .NET y Entity Framework Core.
- Soporte transaccional adecuado para las reglas actuales.
- Compatibilidad con ASP.NET Core Identity.
- Herramientas conocidas y suficientes para la etapa actual del producto.

## Consecuencias

- Las consultas, migraciones y scripts deben ser compatibles con SQL Server.
- SQL Server Express es una elección de infraestructura actual, no una restricción permanente del producto.
- Si volumen, disponibilidad o necesidades operativas superan las capacidades actuales, podrá migrarse a una edición o servicio administrado compatible sin cambiar la decisión de utilizar SQL Server como motor lógico.
