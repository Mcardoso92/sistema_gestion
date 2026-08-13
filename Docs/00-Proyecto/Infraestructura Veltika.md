# Infraestructura Veltika

> Documento de referencia para la infraestructura del sistema Veltika.

## Información General

**Proyecto:** Veltika

**Tecnologías** - ASP.NET Core MVC (.NET 8) - Entity Framework Core -
ASP.NET Identity - SQL Server - Arquitectura MVC - SaaS Multiempresa

## Objetivos

-   Alta disponibilidad.
-   Seguridad.
-   Escalabilidad.
-   Bajo costo durante las primeras etapas.
-   Fácil mantenimiento.
-   Posibilidad de crecer sin rediseñar la arquitectura.

## Proveedor Cloud

-   AWS (Amazon Web Services)
-   Región: South America (São Paulo)

## Cuenta AWS

-   Cuenta creada
-   Presupuesto mensual USD 10
-   Alertas de gasto configuradas
-   MFA habilitado para Root
-   Usuario Root sin Access Keys
-   Usuario IAM: mcardoso
-   Primer inicio de sesión IAM exitoso

## Arquitectura

Internet → Dominio → AWS EC2 → IIS → ASP.NET Core MVC → SQL Server →
Base de Datos Veltika

## Infraestructura Inicial

-   Windows Server 2025
-   EC2 t3.small
-   2 vCPU
-   2 GB RAM
-   Disco SSD gp3 de 50 GB
-   Elastic IP

## Software

-   IIS
-   .NET Hosting Bundle
-   SQL Server
-   SQL Server Management Studio

## Red

Puertos: - 80 - 443 - 3389

## Dominio

-   veltika.com.ar
-   www.veltika.com.ar
-   app.veltika.com.ar

## Checklist

### AWS

-   [x] Cuenta creada
-   [x] Región São Paulo
-   [x] Presupuesto
-   [x] MFA
-   [x] Usuario IAM
-   [ ] EC2
-   [ ] Elastic IP
-   [ ] Security Groups

### Servidor

-   [ ] Windows actualizado
-   [ ] IIS
-   [ ] .NET Hosting Bundle
-   [ ] SQL Server

### Aplicación

-   [ ] Publicación Release
-   [ ] Migraciones
-   [ ] Seed inicial

## Historial

### 07/08/2026

-   Cuenta AWS creada.
-   Presupuesto configurado.
-   Usuario IAM creado.
-   Primer inicio de sesión IAM exitoso.

## Próximo paso

Crear la primera instancia EC2 para alojar Veltika.
