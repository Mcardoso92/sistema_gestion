# Infraestructura Veltika

> Documento de referencia del entorno productivo y la infraestructura actual de Veltika.

Última actualización: 01/09/2026

---

## 1. Información general

**Proyecto:** Veltika

**Stack principal:**

- ASP.NET Core MVC (.NET 9).
- Entity Framework Core.
- ASP.NET Identity.
- SQL Server.
- Arquitectura MVC.
- SaaS multiempresa.

La infraestructura actual está pensada para mantener costos y complejidad controlados durante la etapa inicial del producto, sin impedir una evolución posterior.

---

## 2. Objetivos de infraestructura

- Seguridad.
- Recuperación ante errores.
- Bajo costo durante las primeras etapas.
- Fácil mantenimiento.
- Despliegues controlados y reproducibles.
- Protección de datos productivos.
- Posibilidad de escalar sin rediseñar completamente la arquitectura.

La alta disponibilidad completa no es actualmente un requisito del MVP. Se evaluará cuando el volumen de usuarios y criticidad operativa lo justifiquen.

---

## 3. Proveedor cloud

- AWS (Amazon Web Services).
- Región principal: South America (São Paulo).

### Cuenta AWS

- [x] Cuenta creada.
- [x] Presupuesto y alertas de gasto configuradas.
- [x] MFA habilitado para Root.
- [x] Usuario Root sin Access Keys.
- [x] Usuario IAM administrativo creado.

No deben documentarse en este repositorio credenciales, Access Keys, contraseñas ni otros secretos.

---

## 4. Arquitectura productiva actual

Flujo principal:

`Internet → Dominio/DNS → AWS EC2 → IIS → ASP.NET Core MVC → SQL Server Express → Veltika_DB`

Componentes principales:

- AWS EC2.
- Windows Server.
- IIS.
- App Pool dedicado `VeltikaPool`.
- Aplicación publicada en `C:\inetpub\Veltika`.
- ASP.NET Core en ambiente `Production`.
- SQL Server Express.
- Base de datos `Veltika_DB`.
- Amazon S3 para backups externos de base de datos.

Estado:

✅ Infraestructura productiva operativa.

---

## 5. Servidor

Infraestructura inicial utilizada:

- Windows Server 2025.
- EC2 `t3.small`.
- 2 vCPU.
- 2 GB RAM.
- Disco SSD gp3 de 50 GB.
- Elastic IP.

Software principal:

- IIS.
- .NET Hosting Bundle.
- SQL Server Express.
- SQL Server Management Studio para administración cuando sea necesario.

### Checklist del servidor

- [x] Instancia EC2 creada.
- [x] Elastic IP configurada.
- [x] Security Groups configurados.
- [x] Windows Server operativo.
- [x] IIS instalado y configurado.
- [x] .NET Hosting Bundle instalado.
- [x] SQL Server instalado.
- [x] Aplicación ejecutándose mediante IIS.

---

## 6. Dominio y red

Dominio principal:

- `veltika.com.ar`.
- `www.veltika.com.ar`.

La aplicación productiva documentada utiliza actualmente:

- `www.veltika.com.ar`.

La infraestructura contempla tráfico web HTTP/HTTPS y acceso administrativo restringido al servidor.

Puertos relevantes a nivel de infraestructura:

- 80 — HTTP.
- 443 — HTTPS.
- 3389 — RDP, únicamente para administración y con las restricciones de seguridad correspondientes.

### Estado

- [x] Dominio registrado/configurado.
- [x] Aplicación accesible mediante dominio productivo.
- [ ] Mantener verificados los bindings y certificado HTTPS vigentes.

---

## 7. Configuración de producción y secretos

Producción utiliza variables de entorno de Windows con alcance `Machine` para valores sensibles.

Variables relevantes:

- `ASPNETCORE_ENVIRONMENT=Production`.
- `ConnectionStrings__SaasDbContext`.
- `EmailSettings__Host`.
- `EmailSettings__Port`.
- `EmailSettings__UserName`.
- `EmailSettings__Password`.
- `EmailSettings__FromEmail`.
- `EmailSettings__FromName`.
- `EmailSettings__UseSsl`.

Reglas:

- Los secretos productivos no se almacenan en Git.
- `appsettings.Production.json` no debe contener credenciales reales.
- `appsettings.Development.json` no debe formar parte del paquete productivo.
- Los uploads productivos no deben reemplazarse durante un deploy.

El detalle se mantiene en `Configuracion de produccion.md`.

---

## 8. Base de datos

Motor actual:

- SQL Server Express.
- Instancia: `.\SQLEXPRESS`.
- Base productiva: `Veltika_DB`.

Las actualizaciones de esquema se realizan mediante migraciones de Entity Framework convertidas a scripts SQL idempotentes para el proceso productivo.

### Estado

- [x] SQL Server productivo.
- [x] Base de datos productiva.
- [x] Migraciones de Entity Framework.
- [x] Generación de script idempotente para deploy.
- [x] Aplicación controlada de migraciones durante instalación.

---

## 9. Deploy

Veltika dispone de un proceso de deploy asistido y documentado.

Scripts principales:

- `Scripts\Deploy\Crear-PaqueteDeploy.ps1`.
- `Scripts\Deploy\Instalar-Veltika.ps1`.

El proceso incluye:

1. Build y tests en Release.
2. Publicación de la aplicación.
3. Exclusión de archivos de desarrollo y uploads locales.
4. Generación de migraciones idempotentes.
5. Creación del paquete ZIP.
6. Generación y validación SHA256.
7. Backup previo de la base.
8. Backup de aplicación e IIS.
9. Aplicación de migraciones.
10. Reemplazo controlado de la publicación.
11. Conservación de uploads productivos.
12. Reinicio de IIS.
13. Verificación HTTP posterior.
14. Backup SQL posterior al deploy.

La guía operativa completa está documentada en `Guia de deploy Veltika.md`.

### Estado

- [x] Publicación Release.
- [x] Deploy productivo funcional.
- [x] Scripts de empaquetado e instalación.
- [x] Validación SHA256.
- [x] Migraciones integradas al proceso.
- [x] Smoke test post-deploy definido.

---

## 10. Backups y recuperación

### Backups de base de datos

Ubicación local:

`C:\Backups\Veltika`

Ubicación externa:

`s3://veltika-prod-db-backups/database/`

Script productivo:

`C:\Scripts\Veltika\Backup-Veltika.ps1`

El deploy ejecuta backups antes y después de instalar una nueva versión.

### Recuperación de aplicación

El proceso conserva:

- Backup SQL previo.
- Copia externa en S3.
- Publicación anterior.
- Backup de configuración de IIS.
- Uploads productivos.

### Estado

- [x] Backup SQL local.
- [x] Backup SQL externo en S3.
- [x] Backup previo al deploy.
- [x] Backup posterior al deploy.
- [x] Respaldo de publicación anterior.
- [x] Procedimiento de recuperación documentado.
- [ ] Realizar periódicamente una restauración de prueba desde S3 para validar el proceso completo de recuperación.

---

## 11. Correo transaccional

Veltika dispone de configuración para correo mediante dominio propio.

Proveedor actual:

- Zoho Mail.

Configuración documentada:

- SMTP `smtp.zoho.com`.
- Puerto 587.
- Cuenta de autenticación `contacto@veltika.com.ar`.
- Remitente transaccional `no-reply@veltika.com.ar`.
- Credenciales mediante variables de entorno del servidor.

Los valores sensibles no se almacenan en el repositorio.

---

## 12. Validación posterior al deploy

Cada despliegue debe comprobar como mínimo:

- Landing pública.
- Inicio y cierre de sesión.
- Dashboard.
- Productos.
- Ventas.
- Compras.
- Caja.
- Persistencia de datos existentes.
- Configuración de empresa.
- Carga y acceso a imágenes.
- Recuperación de contraseña.
- Correo transaccional.
- Dominio productivo.
- Logs y estado del App Pool.

---

## 13. Evolución futura

La infraestructura actual es suficiente para la etapa de MVP y primeras pruebas con usuarios reales.

Posibles mejoras futuras, únicamente cuando aporten valor operativo:

- GitHub Actions para build/test de versiones etiquetadas.
- Bucket privado de artefactos de deploy.
- AWS Systems Manager Run Command para despliegue remoto.
- Mayor automatización manteniendo aprobación manual para producción.
- Monitoreo y observabilidad adicionales.
- Alertas de disponibilidad.
- Escalado vertical de EC2 cuando el consumo lo requiera.
- Evaluar migración de SQL Server a una alternativa administrada cuando operación, disponibilidad o escala lo justifiquen.
- Arquitectura de alta disponibilidad cuando la criticidad real del servicio lo requiera.

No se debe aumentar la complejidad de infraestructura antes de que exista una necesidad técnica o comercial comprobada.

---

## 14. Historial

### 07/08/2026

- Cuenta AWS creada.
- Presupuesto configurado.
- Usuario IAM creado.
- Primer inicio de sesión IAM exitoso.

### Agosto 2026

- Primera infraestructura EC2 configurada.
- IIS y SQL Server instalados.
- Veltika desplegada funcionalmente en AWS.
- Dominio productivo configurado.
- Configuración de producción separada del entorno local.

### 31/08/2026

- Configuración productiva revisada y documentada.
- Cobertura automatizada del MVP ampliada.
- Rendimiento, imágenes y paginación optimizados.

### 01/09/2026

- Proceso de deploy documentado y automatizado parcialmente mediante PowerShell.
- Backups SQL locales y externos en S3 incorporados al procedimiento de deploy.
- Procedimiento de recuperación documentado.
- Roadmap e infraestructura sincronizados con el estado real del proyecto.

---

## 15. Próximo objetivo de infraestructura

La prioridad ya no es crear la infraestructura inicial: **esa etapa está completada**.

El objetivo inmediato es validar la infraestructura durante pruebas reales del MVP:

1. Ejecutar deploys repetidos usando el procedimiento documentado.
2. Confirmar smoke tests después de cada versión.
3. Validar periódicamente una restauración real de backup.
4. Revisar HTTPS/bindings y correo transaccional en producción.
5. Observar consumo de recursos y logs durante los pilotos.
6. Escalar o automatizar únicamente cuando los datos reales indiquen que es necesario.