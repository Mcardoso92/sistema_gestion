[CmdletBinding()]
param(
    [string]$Sitio = "Veltika",
    [string]$AppPool = "VeltikaPool",
    [string]$RutaAplicacion = "C:\inetpub\Veltika",
    [string]$HostPrueba = "www.veltika.com.ar",
    [string]$RutaAnterior
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Ejecuta PowerShell como administrador."
    }
}

function Detener-Veltika {
    $sitioActual = Get-Website -Name $Sitio -ErrorAction Stop
    if ($sitioActual.State -ne "Stopped") {
        Stop-WebSite -Name $Sitio
    }

    $poolActual = Get-WebAppPoolState -Name $AppPool -ErrorAction Stop
    if ($poolActual.Value -ne "Stopped") {
        Stop-WebAppPool -Name $AppPool
    }
}

function Iniciar-Veltika {
    $poolActual = Get-WebAppPoolState -Name $AppPool -ErrorAction Stop
    if ($poolActual.Value -ne "Started") {
        Start-WebAppPool -Name $AppPool
    }

    $sitioActual = Get-Website -Name $Sitio -ErrorAction Stop
    if ($sitioActual.State -ne "Started") {
        Start-WebSite -Name $Sitio
    }
}

function Probar-Veltika {
    Start-Sleep -Seconds 2
    $respuesta = Invoke-WebRequest "http://localhost" -Headers @{ Host = $HostPrueba } -UseBasicParsing -TimeoutSec 30
    if ($respuesta.StatusCode -ne 200) {
        throw "La prueba local devolvio HTTP $($respuesta.StatusCode)."
    }
}

Verificar-Administrador
Import-Module WebAdministration

if (-not (Test-Path -LiteralPath $RutaAplicacion -PathType Container)) {
    throw "No se encontro la aplicacion actual: $RutaAplicacion"
}

# El rollback es deliberadamente SOLO de archivos de aplicacion.
# No ejecuta migraciones, no restaura backups SQL y no modifica la base de datos.
if ([string]::IsNullOrWhiteSpace($RutaAnterior)) {
    $directorioPadre = Split-Path -Parent $RutaAplicacion
    $nombreAplicacion = Split-Path -Leaf $RutaAplicacion
    $patron = "$nombreAplicacion-anterior-*"

    $candidatos = Get-ChildItem -LiteralPath $directorioPadre -Directory -Filter $patron |
        Sort-Object LastWriteTime -Descending

    if (-not $candidatos -or $candidatos.Count -eq 0) {
        throw "No se encontro ninguna publicacion anterior con patron '$patron'."
    }

    $RutaAnterior = $candidatos[0].FullName
}

if (-not (Test-Path -LiteralPath $RutaAnterior -PathType Container)) {
    throw "No se encontro la publicacion anterior: $RutaAnterior"
}

if (-not (Test-Path -LiteralPath (Join-Path $RutaAnterior "saas.dll") -PathType Leaf)) {
    throw "La publicacion anterior no contiene saas.dll: $RutaAnterior"
}

if (-not (Test-Path -LiteralPath (Join-Path $RutaAplicacion "saas.dll") -PathType Leaf)) {
    throw "La publicacion actual no contiene saas.dll: $RutaAplicacion"
}

$marca = Get-Date -Format "yyyyMMdd-HHmmss"
$rutaActualResguardada = "C:\inetpub\Veltika-rollback-resguardo-$marca"
$rutaUploadsActuales = Join-Path $RutaAplicacion "wwwroot\uploads"

Write-Host ""
Write-Host "=== ROLLBACK DE VELTIKA ==="
Write-Host "Version actual:   $RutaAplicacion"
Write-Host "Version a restaurar: $RutaAnterior"
Write-Host "Resguardo actual: $rutaActualResguardada"
Write-Host ""
Write-Host "IMPORTANTE: este script NO modifica ni restaura la base de datos."
Write-Host "Los uploads productivos actuales se conservaran."
Write-Host "Si la version anterior no responde HTTP 200, se restaurara automaticamente la version actual."
Write-Host ""

$confirmacion = Read-Host "Escribi ROLLBACK para continuar"
if ($confirmacion -cne "ROLLBACK") {
    throw "Rollback cancelado."
}

$rollbackAplicado = $false

try {
    Write-Host "=== INICIO DE MANTENIMIENTO ==="
    Detener-Veltika

    Write-Host "=== RESGUARDO DE VERSION ACTUAL ==="
    Move-Item -LiteralPath $RutaAplicacion -Destination $rutaActualResguardada

    Write-Host "=== RESTAURACION DE VERSION ANTERIOR ==="
    Copy-Item -LiteralPath $RutaAnterior -Destination $RutaAplicacion -Recurse
    $rollbackAplicado = $true

    Write-Host "=== CONSERVACION DE UPLOADS PRODUCTIVOS ==="
    $uploadsResguardados = Join-Path $rutaActualResguardada "wwwroot\uploads"
    $uploadsRestaurados = Join-Path $RutaAplicacion "wwwroot\uploads"

    if (Test-Path -LiteralPath $uploadsRestaurados) {
        Remove-Item -LiteralPath $uploadsRestaurados -Recurse -Force
    }

    New-Item -ItemType Directory -Path $uploadsRestaurados -Force | Out-Null

    if (Test-Path -LiteralPath $uploadsResguardados) {
        Get-ChildItem -LiteralPath $uploadsResguardados -Force | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $uploadsRestaurados -Recurse -Force
        }
    }

    & icacls $uploadsRestaurados /grant "IIS AppPool\$AppPool`:(OI)(CI)(M)" /T | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudieron restaurar los permisos de uploads."
    }

    Write-Host "=== INICIO Y PRUEBA LOCAL ==="
    Iniciar-Veltika
    Probar-Veltika

    Write-Host ""
    Write-Host "Rollback finalizado correctamente."
    Write-Host "Version restaurada: $RutaAnterior"
    Write-Host "Version reemplazada conservada en: $rutaActualResguardada"
    Write-Host "Base de datos: SIN CAMBIOS"
}
catch {
    $errorRollback = $_
    Write-Host ""
    Write-Host "ERROR DURANTE EL ROLLBACK: $($errorRollback.Exception.Message)"

    if ($rollbackAplicado -and (Test-Path -LiteralPath $rutaActualResguardada -PathType Container)) {
        Write-Host "=== RECUPERACION AUTOMATICA DE VERSION ACTUAL ==="

        try {
            Detener-Veltika

            if (Test-Path -LiteralPath $RutaAplicacion) {
                $rutaRollbackFallido = "C:\inetpub\Veltika-rollback-fallido-$marca"
                Move-Item -LiteralPath $RutaAplicacion -Destination $rutaRollbackFallido
            }

            Move-Item -LiteralPath $rutaActualResguardada -Destination $RutaAplicacion
            Iniciar-Veltika
            Probar-Veltika

            Write-Host "Se restauro automaticamente la version que estaba activa antes del rollback."
        }
        catch {
            Write-Host "FALLO TAMBIEN LA RECUPERACION AUTOMATICA: $($_.Exception.Message)"
            Write-Host "La version previa al rollback permanece resguardada, si fue posible, en: $rutaActualResguardada"
        }
    }
    else {
        try {
            Iniciar-Veltika
        }
        catch {
            Write-Host "No se pudo asegurar el inicio de IIS: $($_.Exception.Message)"
        }
    }

    throw $errorRollback
}
