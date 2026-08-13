# ADR-011 - Elección de AWS como proveedor Cloud

**Estado:** Aceptado

## Contexto

Se evaluaron AWS y Azure para alojar Veltika.

## Decisión

Se utilizará AWS como plataforma principal.

## Motivos

-   Amplia adopción en la industria.
-   Excelente escalabilidad.
-   Amplia documentación.
-   Buen ecosistema para .NET.
-   Posibilidad de crecer sin rediseñar la infraestructura.

## Consecuencias

-   Uso de EC2, IAM, Security Groups y CloudWatch.
-   Región principal: South America (São Paulo).
