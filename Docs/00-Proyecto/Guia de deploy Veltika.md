# Guía de deploy de Veltika

Esta guía documenta el procedimiento usado para publicar Veltika en AWS EC2 con Windows Server, IIS, SQL Server Express y ASP.NET Core 9.

## Arquitectura actual

- Sitio IIS: `Veltika`.
- App Pool: `VeltikaPool`.
- Aplicación: `C:\inetpub\Veltika`.
- SQL Server: `.\SQLEXPRESS`.
- Base: `Veltika_DB`.
- Dominio: `www.veltika.com.ar`.
- Backups locales: `C:\Backups\Veltika`.
- Backups externos: `s3://veltika-prod-db-backups/database/`.
- Script de backup: `C:\Scripts\Veltika\Backup-Veltika.ps1`.

## Variables y secretos

Los valores reales nunca se almacenan en Git ni en el ZIP. El servidor necesita variables de Windows con alcance `Machine`:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__SaasDbContext`
- `EmailSettings__Host`
- `EmailSettings__Port`
- `EmailSettings__UserName`
- `EmailSettings__Password`
- `EmailSettings__FromEmail`
- `EmailSettings__FromName`
- `EmailSettings__UseSsl`

Zoho utiliza `smtp.zoho.com`, puerto `587`, usuario `contacto@veltika.com.ar`, remitente `no-reply@veltika.com.ar` y `UseSsl=false` para STARTTLS. La contraseña debe ser una contraseña específica de aplicación.

## Deploy asistido recomendado

La automatización mantiene un control manual entre la PC y el servidor:

1. `Scripts\Deploy\Crear-PaqueteDeploy.ps1` genera y valida el paquete local.
2. El ZIP se copia al servidor mediante RDP.
3. `Scripts\Deploy\Instalar-Veltika.ps1` respalda, migra y publica en la EC2.

### Generar el paquete

Primero actualizar `main` y confirmar que esté limpia:

```powershell
git switch main
git pull origin main
git status
```

Desde la raíz del repositorio:

```powershell
.\Scripts\Deploy\Crear-PaqueteDeploy.ps1
```

El script ejecuta tests en Release, publica, excluye `appsettings.Development.json` y los uploads locales, genera migraciones idempotentes, crea el ZIP y calcula SHA256. El resultado se guarda en `C:\Publish` con fecha y hora.

### Transferir y validar

Copiar a `C:\Deploy` en el servidor:

- El `.zip`.
- El `.sha256.txt`.
- El instalador cuando el servidor todavía no lo tenga.

Comprobar el hash:

```powershell
Get-FileHash "C:\Deploy\NOMBRE-DEL-PAQUETE.zip" -Algorithm SHA256
```

### Instalar en el servidor

Guardar el instalador en `C:\Scripts\Veltika\Instalar-Veltika.ps1` y abrir PowerShell como administrador:

```powershell
C:\Scripts\Veltika\Instalar-Veltika.ps1 `
    -PaqueteZip "C:\Deploy\NOMBRE-DEL-PAQUETE.zip" `
    -HashEsperado "HASH-SHA256"
```

El script exige escribir `DESPLEGAR` y después:

1. Verifica administrador, hash y variables.
2. Extrae el paquete fuera de IIS.
3. Ejecuta el backup SQL previo y la subida a S3.
4. Respalda la publicación y la configuración de IIS.
5. Detiene sitio y App Pool.
6. Ejecuta las migraciones idempotentes con `sqlcmd -E -C -I -b`.
7. Conserva la publicación anterior.
8. Instala la nueva versión.
9. Conserva `wwwroot\uploads` productivo.
10. Restaura permisos de uploads.
11. Reinicia IIS y exige HTTP 200.
12. Ejecuta un backup SQL posterior.

`-I` es obligatorio porque activa `QUOTED_IDENTIFIER`, requerido por SQL Server para índices filtrados.

## Procedimiento manual

### En la PC

1. Confirmar `main` limpia y sincronizada.
2. Ejecutar build y tests.
3. Publicar en Release en una carpeta nueva.
4. Eliminar del paquete `appsettings.Development.json`.
5. Eliminar del paquete `wwwroot\uploads`.
6. Conservar `appsettings.json` y `appsettings.Production.json`.
7. Generar migraciones idempotentes.
8. Comprimir aplicación y SQL.
9. Calcular SHA256.

```powershell
dotnet ef migrations script --idempotent `
    --project .\saas\saas.csproj `
    --startup-project .\saas\saas.csproj `
    --output "C:\Publish\Veltika-Migraciones.sql"
```

### En el servidor

1. Confirmar espacio libre.
2. Ejecutar `Backup-Veltika.ps1`.
3. Confirmar `.bak` local y copia en S3.
4. Respaldar `C:\inetpub\Veltika`.
5. Respaldar `applicationHost.config` e IIS con `appcmd`.
6. Confirmar los nombres de variables sin mostrar valores.
7. Extraer el ZIP fuera de `C:\inetpub`.
8. Comparar SHA256.
9. Detener sitio y App Pool.
10. Aplicar SQL con `-E -C -I -b`.
11. Ante un error SQL, no reemplazar archivos.
12. Confirmar `__EFMigrationsHistory`.
13. Renombrar la publicación anterior; no eliminarla.
14. Copiar la nueva publicación.
15. Recuperar uploads y otorgar `Modify` a `IIS AppPool\VeltikaPool` solo sobre esa carpeta.
16. Reiniciar IIS y verificar HTTP 200.
17. Ejecutar un backup SQL posterior.

## Smoke test posterior

Verificar como mínimo:

- Landing pública.
- Inicio y cierre de sesión.
- Dashboard.
- Productos, ventas, compras y caja.
- Datos existentes.
- Configuración de empresa.
- Carga de imágenes.
- Recuperación de contraseña.
- Remitente `no-reply@veltika.com.ar`.
- Enlace `https://www.veltika.com.ar`.
- Logs y estado del App Pool.

## Prueba de restauración del backup

La validación real se realiza en una base temporal y no reemplaza ni detiene `Veltika_DB`.
Copiar `Verificar-RestauracionBackupVeltika.ps1` a `C:\Scripts\Veltika` y ejecutar, como administrador, con un backup productivo reciente:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File "C:\Scripts\Veltika\Verificar-RestauracionBackupVeltika.ps1" `
    -ArchivoBackup "C:\Backups\Veltika\Veltika_DB_AAAAMMDD-HHMM.bak"
```

El script ejecuta `RESTORE VERIFYONLY`, restaura el archivo con un nombre temporal, comprueba su integridad mediante `DBCC CHECKDB`, verifica que quede `ONLINE` y elimina la base temporal. La prueba es satisfactoria únicamente cuando informa que el backup fue restaurado y verificado y que producción no fue modificada.

La opción `-ConservarBaseTemporal` se reserva para un diagnóstico manual. No debe utilizarse en la verificación periódica normal.

## Monitoreo y logs básicos

El servidor ejecuta `Verificar-SaludVeltika.ps1` cada cinco minutos mediante la tarea programada `Veltika - Monitoreo de salud`. La verificación cubre:

- sitio y App Pool de IIS iniciados;
- respuesta HTTP 200 mediante HTTPS;
- base `Veltika_DB` en estado `ONLINE`;
- existencia, tamaño y antigüedad del último backup productivo.

Cada ejecución se conserva durante 30 días en `C:\VeltikaLogs\Monitoreo\salud-AAAA-MM-DD.jsonl`. Los fallos también se registran como errores en el log `Application` del Visor de eventos, con origen `Veltika Monitor`.

La instalación inicial se realiza una sola vez, como administrador:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File "C:\Scripts\Veltika\Instalar-MonitoreoVeltika.ps1"
```

La comprobación manual puede ejecutarse en cualquier momento:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File "C:\Scripts\Veltika\Verificar-SaludVeltika.ps1"
```

## Recuperación

No continuar automáticamente después de un error. Para recuperar existen:

- `.bak` previo local y en S3.
- Copia de `C:\inetpub\Veltika`.
- Carpeta `Veltika-anterior-*`.
- Backup de IIS creado con `appcmd`.

Si las migraciones fallan antes de reemplazar archivos, mantener IIS detenido, corregir la causa y volver a ejecutar el script idempotente. Restaurar la base solo cuando sea necesario volver explícitamente al estado anterior.

Si la versión nueva falla después del reemplazo, conservar logs y evaluar la compatibilidad entre la base migrada y la publicación anterior antes del rollback.

## Automatización futura

El siguiente nivel puede usar GitHub Actions, un bucket privado de artefactos y AWS Systems Manager Run Command. Debe compilar y probar una versión etiquetada, subir ZIP y SHA256 a S3 y ordenar el despliegue por SSM sin abrir puertos nuevos. Producción debe conservar una aprobación manual.

Conviene validar varias veces los dos scripts actuales antes de habilitar ese flujo completamente remoto.
