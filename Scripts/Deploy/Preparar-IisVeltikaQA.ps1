[CmdletBinding()]
param(
    [string]$Sitio = "Veltika-QA",
    [string]$AppPool = "VeltikaQAPool",
    [string]$RutaAplicacion = "C:\inetpub\Veltika-QA",
    [string]$HostQa = "qa.veltika.com.ar"
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Ejecuta PowerShell como administrador."
    }
}

function Verificar-AislamientoProduccion {
    param(
        [Parameter(Mandatory)][string]$NombreSitio,
        [Parameter(Mandatory)][string]$NombreAppPool,
        [Parameter(Mandatory)][string]$Ruta,
        [Parameter(Mandatory)][string]$Host
    )

    if ($NombreSitio -ieq "Veltika" -or
        $NombreAppPool -ieq "VeltikaPool" -or
        $Ruta.TrimEnd("\") -ieq "C:\inetpub\Veltika" -or
        $Host -ieq "www.veltika.com.ar") {
        throw "La configuracion de QA no puede reutilizar nombres, rutas ni host de produccion."
    }
}

Verificar-Administrador
Verificar-AislamientoProduccion -NombreSitio $Sitio -NombreAppPool $AppPool -Ruta $RutaAplicacion -Host $HostQa
Import-Module WebAdministration

if (-not (Test-Path -LiteralPath $RutaAplicacion -PathType Container)) {
    New-Item -ItemType Directory -Path $RutaAplicacion -Force | Out-Null
}

if (-not (Test-Path "IIS:\AppPools\$AppPool")) {
    New-WebAppPool -Name $AppPool | Out-Null
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name managedPipelineMode -Value "Integrated"
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name processModel.identityType -Value "ApplicationPoolIdentity"
}

$sitioExistente = Get-Website -Name $Sitio -ErrorAction SilentlyContinue
if ($null -eq $sitioExistente) {
    New-Website -Name $Sitio -PhysicalPath $RutaAplicacion -Port 80 -HostHeader $HostQa -ApplicationPool $AppPool | Out-Null
}
else {
    if ($sitioExistente.PhysicalPath.TrimEnd("\") -ine $RutaAplicacion.TrimEnd("\")) {
        throw "El sitio '$Sitio' ya existe pero utiliza otra ruta: $($sitioExistente.PhysicalPath)"
    }

    if ($sitioExistente.ApplicationPool -ine $AppPool) {
        throw "El sitio '$Sitio' ya existe pero utiliza otro App Pool: $($sitioExistente.ApplicationPool)"
    }

    $bindingEsperado = "*:80:$HostQa"
    $bindingCorrecto = Get-WebBinding -Name $Sitio -Protocol "http" | Where-Object { $_.bindingInformation -eq $bindingEsperado }
    if ($null -eq $bindingCorrecto) {
        throw "El sitio '$Sitio' ya existe pero no posee el binding HTTP esperado '$bindingEsperado'."
    }
}

# El sitio permanece detenido hasta tener aplicacion, variables y base de datos propias.
if ((Get-WebsiteState -Name $Sitio).Value -ne "Stopped") {
    Stop-WebSite -Name $Sitio
}
if ((Get-WebAppPoolState -Name $AppPool).Value -ne "Stopped") {
    Stop-WebAppPool -Name $AppPool
}

Write-Host "Infraestructura IIS de QA preparada correctamente."
Write-Host "Sitio: $Sitio (detenido)"
Write-Host "App Pool: $AppPool (detenido)"
Write-Host "Ruta: $RutaAplicacion"
Write-Host "Binding HTTP: $HostQa"
Write-Host "Produccion no fue modificada."
