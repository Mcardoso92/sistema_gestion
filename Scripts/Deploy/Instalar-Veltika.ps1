[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PaqueteZip,
    [Parameter(Mandatory)][string]$HashEsperado,
    [string]$Sitio = "Veltika",
    [string]$AppPool = "VeltikaPool",
    [string]$RutaAplicacion = "C:\inetpub\Veltika",
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_DB",
    [string]$HostPrueba = "www.veltika.com.ar"
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw "Ejecutá PowerShell como administrador." }
}

function Verificar-Variables {
    $requeridas = @("ASPNETCORE_ENVIRONMENT", "ConnectionStrings__SaasDbContext", "EmailSettings__Host", "EmailSettings__Port", "EmailSettings__UserName", "EmailSettings__Password", "EmailSettings__FromEmail", "EmailSettings__FromName", "EmailSettings__UseSsl")
    $faltantes = foreach ($nombre in $requeridas) {
        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($nombre, "Machine"))) { $nombre }
    }
    if ($faltantes) { throw "Faltan variables de entorno: $($faltantes -join ', ')" }
}

Verificar-Administrador
Verificar-Variables
if (-not (Test-Path -LiteralPath $PaqueteZip)) { throw "No se encontró el paquete: $PaqueteZip" }

$hashReal = (Get-FileHash -LiteralPath $PaqueteZip -Algorithm SHA256).Hash
if ($hashReal -ne $HashEsperado.Trim()) { throw "El SHA256 del paquete no coincide. No se realizará el deploy." }

$confirmacion = Read-Host "Escribí DESPLEGAR para continuar"
if ($confirmacion -cne "DESPLEGAR") { throw "Deploy cancelado." }

Import-Module WebAdministration
$marca = Get-Date -Format "yyyyMMdd-HHmmss"
$directorioTrabajo = "C:\Deploy\Trabajo-$marca"
$respaldoAplicacion = "C:\VeltikaBackups\$marca-predeploy"
$rutaAnterior = "C:\inetpub\Veltika-anterior-$marca"
$scriptBackup = "C:\Scripts\Veltika\Backup-Veltika.ps1"
$backupIis = "Veltika-$marca"

New-Item -ItemType Directory -Path $directorioTrabajo -Force | Out-Null
Expand-Archive -LiteralPath $PaqueteZip -DestinationPath $directorioTrabajo -Force
$nuevaAplicacion = Join-Path $directorioTrabajo "Aplicacion"
$scriptMigraciones = Join-Path $directorioTrabajo "Veltika-Migraciones.sql"

if (-not (Test-Path (Join-Path $nuevaAplicacion "saas.dll"))) { throw "El paquete no contiene Aplicacion\saas.dll." }
if (-not (Test-Path $scriptMigraciones)) { throw "El paquete no contiene Veltika-Migraciones.sql." }
if (-not (Test-Path $scriptBackup)) { throw "No se encontró el script de backup: $scriptBackup" }

Write-Host "=== BACKUP PREVIO ==="
& $scriptBackup
if (-not $?) { throw "Falló el backup previo." }

New-Item -ItemType Directory -Path $respaldoAplicacion -Force | Out-Null
Copy-Item -LiteralPath $RutaAplicacion -Destination (Join-Path $respaldoAplicacion "Aplicacion") -Recurse
Copy-Item -LiteralPath "$env:windir\System32\inetsrv\config\applicationHost.config" -Destination (Join-Path $respaldoAplicacion "applicationHost.config")
& "$env:windir\System32\inetsrv\appcmd.exe" add backup $backupIis

Write-Host "=== INICIO DE MANTENIMIENTO ==="
Stop-WebSite -Name $Sitio
Stop-WebAppPool -Name $AppPool

Write-Host "=== MIGRACIONES ==="
& sqlcmd -S $InstanciaSql -d $BaseDatos -E -C -I -b -i $scriptMigraciones
if ($LASTEXITCODE -ne 0) { throw "Fallaron las migraciones. La aplicación permanece detenida." }

Write-Host "=== REEMPLAZO DE APLICACIÓN ==="
Move-Item -LiteralPath $RutaAplicacion -Destination $rutaAnterior
Copy-Item -LiteralPath $nuevaAplicacion -Destination $RutaAplicacion -Recurse

# Los uploads pertenecen al servidor y se conservan entre publicaciones.
$uploadsAnteriores = Join-Path $rutaAnterior "wwwroot\uploads"
$uploadsNuevos = Join-Path $RutaAplicacion "wwwroot\uploads"
New-Item -ItemType Directory -Path $uploadsNuevos -Force | Out-Null
if (Test-Path $uploadsAnteriores) {
    Get-ChildItem -LiteralPath $uploadsAnteriores -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $uploadsNuevos -Recurse -Force
    }
}
& icacls $uploadsNuevos /grant "IIS AppPool\$AppPool`:(OI)(CI)(M)" /T

Write-Host "=== INICIO Y PRUEBA LOCAL ==="
iisreset | Out-Host
Start-WebAppPool -Name $AppPool
Start-WebSite -Name $Sitio
$respuesta = Invoke-WebRequest "http://localhost" -Headers @{ Host = $HostPrueba } -UseBasicParsing
if ($respuesta.StatusCode -ne 200) { throw "La prueba local devolvió HTTP $($respuesta.StatusCode)." }

Write-Host "=== BACKUP POSTERIOR ==="
& $scriptBackup
if (-not $?) { throw "La aplicación funciona, pero falló el backup posterior." }

Write-Host ""
Write-Host "Deploy finalizado correctamente."
Write-Host "Publicación anterior: $rutaAnterior"
Write-Host "Backup de archivos: $respaldoAplicacion"
Write-Host "Backup de IIS: $backupIis"
