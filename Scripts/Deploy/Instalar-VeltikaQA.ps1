[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PaqueteZip,
    [Parameter(Mandatory)][string]$HashEsperado
)

$ErrorActionPreference = "Stop"

$instaladorCompartido = Join-Path $PSScriptRoot "Instalar-Veltika.ps1"
if (-not (Test-Path -LiteralPath $instaladorCompartido -PathType Leaf)) {
    throw "No se encontro el instalador compartido: $instaladorCompartido"
}

$parametrosQa = @{
    PaqueteZip        = $PaqueteZip
    HashEsperado      = $HashEsperado
    Sitio             = "Veltika-QA"
    AppPool           = "VeltikaQAPool"
    RutaAplicacion    = "C:\inetpub\Veltika-QA"
    InstanciaSql      = ".\SQLEXPRESS"
    BaseDatos         = "Veltika_QA_DB"
    HostPrueba        = "qa.veltika.com.ar"
    OrigenVariables   = "AppPool"
    ScriptBackup      = "C:\Scripts\Veltika\Backup-VeltikaQA.ps1"
    DirectorioBackups = "C:\VeltikaBackups\QA"
    PrefijoRespaldo   = "Veltika-QA"
}

Write-Host "=== DEPLOY VELTIKA QA ==="
Write-Host "Sitio: Veltika-QA"
Write-Host "Base: Veltika_QA_DB"
Write-Host "Ruta: C:\inetpub\Veltika-QA"
Write-Host "Produccion no sera utilizada por este comando."
Write-Host ""

& $instaladorCompartido @parametrosQa
