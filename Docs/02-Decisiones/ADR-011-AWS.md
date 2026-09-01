# ADR-011 - AWS como plataforma de infraestructura

## Estado
Aceptado

## Contexto

Veltika necesita una plataforma productiva que permita comenzar con una infraestructura simple y evolucionar gradualmente sin exigir una arquitectura distribuida prematura.

Se evaluaron alternativas de nube, incluyendo AWS y Azure.

## Decisión

AWS será la plataforma principal de infraestructura de Veltika.

La arquitectura productiva actual utiliza principalmente:

- Amazon EC2 para el servidor de aplicación;
- Windows Server;
- IIS para hospedar ASP.NET Core;
- SQL Server Express en la infraestructura productiva actual;
- Amazon S3 como almacenamiento externo de backups de base de datos.

No se adopta por esta decisión la obligación de utilizar servicios AWS administrados para cada componente. La infraestructura podrá evolucionar cuando exista una necesidad operativa o de escala demostrada.

## Motivos

- Ecosistema amplio y maduro.
- Capacidad de crecimiento progresivo.
- Buen soporte para cargas .NET y Windows.
- Disponibilidad de servicios complementarios para backups, automatización, monitoreo y evolución futura.

## Consecuencias

- La operación productiva depende de servicios AWS y requiere administración adecuada de acceso, red, backups y costos.
- La infraestructura actual prioriza simplicidad para la etapa MVP.
- Servicios como RDS, balanceadores, almacenamiento de archivos en S3 o despliegues completamente automatizados no se consideran obligatorios hasta que exista una razón concreta para incorporarlos.

## Documentación relacionada

El detalle operativo vigente se mantiene en `Docs/00-Proyecto/Infraestructura Veltika.md` y en la guía de deploy. Este ADR registra la decisión tecnológica, no reemplaza esa documentación.
