[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PaqueteZip,
    [Parameter(Mandatory)][string]$HashEsperado,
    [string]$Sitio = "Veltika",
    [string]$AppPool = "VeltikaPool",
    [string]$RutaAplicacion = "C:\inetpub\Veltika",
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_DB",
    [string]$HostPrueba = "www.veltika.com.ar",
    [ValidateSet("Machine", "AppPool")][string]$OrigenVariables = "Machine"
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw "Ejecuta PowerShell como administrador." }
}

function Verificar-Variables {
    param(
        [Parameter(Mandatory)][string]$NombreAppPool,
        [Parameter(Mandatory)][ValidateSet("Machine", "AppPool")][string]$Origen,
        [Parameter(Mandatory)][string]$BaseDatosEsperada
    )

    $requeridas = @("ConnectionStrings__SaasDbContext", "EmailSettings__Host", "EmailSettings__Port", "EmailSettings__UserName", "EmailSettings__Password", "EmailSettings__FromEmail", "EmailSettings__FromName", "EmailSettings__UseSsl")

    if ($Origen -eq "Machine") {
        # Produccion conserva las variables globales actuales. Si el ambiente no
        # esta definido, ASP.NET Core utiliza Production de forma predeterminada.
        $faltantes = foreach ($nombre in $requeridas) {
            if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($nombre, "Machine"))) { $nombre }
        }
    }
    else {
        if (-not (Test-Path "IIS:\AppPools\$NombreAppPool")) {
            throw "No existe el App Pool '$NombreAppPool'."
        }

        $requeridasAppPool = @("ASPNETCORE_ENVIRONMENT") + $requeridas
        $filtro = "system.applicationHost/applicationPools/add[@name='$NombreAppPool']/environmentVariables"
        $configuracion = Get-WebConfiguration -PSPath "MACHINE/WEBROOT/APPHOST" -Filter $filtro
        $variablesConfiguradas = @{}
        foreach ($elemento in $configuracion.Collection) {
            $variablesConfiguradas[[string]$elemento.GetAttributeValue("name")] = [string]$elemento.GetAttributeValue("value")
        }

        $faltantes = @($requeridasAppPool | Where-Object {
            -not $variablesConfiguradas.ContainsKey($_) -or [string]::IsNullOrWhiteSpace($variablesConfiguradas[$_])
        })

        if (-not $faltantes) {
            if ($variablesConfiguradas["ASPNETCORE_ENVIRONMENT"] -cne "Staging") {
                throw "El App Pool '$NombreAppPool' debe utilizar ASPNETCORE_ENVIRONMENT=Staging."
            }

            $baseEscapada = [Regex]::Escape($BaseDatosEsperada)
            $patronBase = "(?i)(Database|Initial Catalog)\s*=\s*$baseEscapada(?:\s*;|\s*$)"
            if ($variablesConfiguradas["ConnectionStrings__SaasDbContext"] -notmatch $patronBase) {
                throw "La connection string del App Pool '$NombreAppPool' no apunta a '$BaseDatosEsperada'."
            }
        }
    }

    if ($faltantes) { throw "Faltan variables de entorno: $($faltantes -join ', ')" }
}

function Detener-AplicacionIis {
    param(
        [Parameter(Mandatory)][string]$NombreSitio,
        [Parameter(Mandatory)][string]$NombreAppPool
    )

    if ((Get-WebsiteState -Name $NombreSitio).Value -ne "Stopped") {
        Stop-WebSite -Name $NombreSitio
    }

    if ((Get-WebAppPoolState -Name $NombreAppPool).Value -ne "Stopped") {
        Stop-WebAppPool -Name $NombreAppPool
    }

    $appcmd = "$env:windir\System32\inetsrv\appcmd.exe"
    $limite = (Get-Date).AddSeconds(30)

    do {
        $estadoAppPool = (Get-WebAppPoolState -Name $NombreAppPool).Value
        $procesosActivos = @(& $appcmd list wp "/apppool.name:$NombreAppPool")

        if ($estadoAppPool -eq "Stopped" -and $procesosActivos.Count -eq 0) {
            return
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $limite)

    throw "IIS no libero los archivos de la aplicacion dentro de 30 segundos."
}

Verificar-Administrador
Import-Module WebAdministration
Verificar-Variables -NombreAppPool $AppPool -Origen $OrigenVariables -BaseDatosEsperada $BaseDatos
if (-not (Test-Path -LiteralPath $PaqueteZip)) { throw "No se encontro el paquete: $PaqueteZip" }

$hashReal = (Get-FileHash -LiteralPath $PaqueteZip -Algorithm SHA256).Hash
if ($hashReal -ne $HashEsperado.Trim()) { throw "El SHA256 del paquete no coincide. No se realizara el deploy." }

$confirmacion = Read-Host "Escribi DESPLEGAR para continuar"
if ($confirmacion -cne "DESPLEGAR") { throw "Deploy cancelado." }

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
if (-not (Test-Path $scriptBackup)) { throw "No se encontro el script de backup: $scriptBackup" }

Write-Host "=== BACKUP PREVIO ==="
& $scriptBackup
if (-not $?) { throw "Fallo el backup previo." }

New-Item -ItemType Directory -Path $respaldoAplicacion -Force | Out-Null
Copy-Item -LiteralPath $RutaAplicacion -Destination (Join-Path $respaldoAplicacion "Aplicacion") -Recurse
Copy-Item -LiteralPath "$env:windir\System32\inetsrv\config\applicationHost.config" -Destination (Join-Path $respaldoAplicacion "applicationHost.config")
& "$env:windir\System32\inetsrv\appcmd.exe" add backup $backupIis

Write-Host "=== INICIO DE MANTENIMIENTO ==="
Detener-AplicacionIis -NombreSitio $Sitio -NombreAppPool $AppPool

Write-Host "=== MIGRACIONES ==="
& sqlcmd -S $InstanciaSql -d $BaseDatos -E -C -I -b -i $scriptMigraciones
if ($LASTEXITCODE -ne 0) { throw "Fallaron las migraciones. La aplicacion permanece detenida." }

Write-Host "=== REEMPLAZO DE APLICACION ==="
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
# El sitio y su App Pool se administran de forma individual. Reiniciar IIS completo
# interrumpiria otros ambientes alojados en el mismo servidor, como Veltika-QA.
Start-WebAppPool -Name $AppPool
Start-WebSite -Name $Sitio
$respuesta = Invoke-WebRequest "http://localhost" -Headers @{ Host = $HostPrueba } -UseBasicParsing
if ($respuesta.StatusCode -ne 200) { throw "La prueba local devolvio HTTP $($respuesta.StatusCode)." }

Write-Host "=== BACKUP POSTERIOR ==="
& $scriptBackup
if (-not $?) { throw "La aplicacion funciona, pero fallo el backup posterior." }

Write-Host ""
Write-Host "Deploy finalizado correctamente."
Write-Host "Publicacion anterior: $rutaAnterior"
Write-Host "Backup de archivos: $respaldoAplicacion"
Write-Host "Backup de IIS: $backupIis"
