[CmdletBinding()]
param([string]$DirectorioSalida = "C:\Publish")

$ErrorActionPreference = "Stop"

function Ejecutar-Comando {
    param([string]$Comando, [string[]]$Argumentos)
    & $Comando @Argumentos
    if ($LASTEXITCODE -ne 0) { throw "El comando '$Comando' finalizó con código $LASTEXITCODE." }
}

# El script vive en Scripts/Deploy y obtiene desde allí la raíz del repositorio.
$raizRepositorio = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$proyectoWeb = Join-Path $raizRepositorio "saas\saas.csproj"
$proyectoTests = Join-Path $raizRepositorio "saas.Tests\saas.Tests.csproj"
$marca = Get-Date -Format "yyyyMMdd-HHmmss"
$directorioDeploy = Join-Path $DirectorioSalida "VeltikaDeploy-$marca"
$directorioAplicacion = Join-Path $directorioDeploy "Aplicacion"
$scriptMigraciones = Join-Path $directorioDeploy "Veltika-Migraciones.sql"
$archivoZip = "$directorioDeploy.zip"
$artefactosTemporales = Join-Path $env:TEMP "VeltikaDeployArtifacts-$marca"

if (-not (Test-Path $proyectoWeb) -or -not (Test-Path $proyectoTests)) { throw "No se encontraron los proyectos de Veltika." }

Push-Location $raizRepositorio
try {
    $rama = (git branch --show-current).Trim()
    $cambios = git status --porcelain
    if ($LASTEXITCODE -ne 0) { throw "No se pudo consultar el estado de Git." }
    if ($rama -ne "main") { throw "El paquete debe generarse desde main. Rama actual: $rama." }
    if ($cambios) { throw "El repositorio tiene cambios sin confirmar. Revisalos antes de generar el deploy." }

    New-Item -ItemType Directory -Path $directorioAplicacion -Force | Out-Null

    Write-Host "=== PRUEBAS AUTOMATIZADAS ==="
    Ejecutar-Comando -Comando "dotnet" -Argumentos @("test", $proyectoTests, "-c", "Release", "--verbosity:minimal", "--artifacts-path", $artefactosTemporales)

    Write-Host "=== PUBLICACIÓN RELEASE ==="
    Ejecutar-Comando -Comando "dotnet" -Argumentos @("publish", $proyectoWeb, "-c", "Release", "--verbosity:minimal", "--output", $directorioAplicacion)

    # La configuración de desarrollo y las imágenes locales nunca forman parte del paquete.
    $configuracionDesarrollo = Join-Path $directorioAplicacion "appsettings.Development.json"
    $uploadsLocales = Join-Path $directorioAplicacion "wwwroot\uploads"
    if (Test-Path $configuracionDesarrollo) { Remove-Item -LiteralPath $configuracionDesarrollo -Force }
    if (Test-Path $uploadsLocales) { Remove-Item -LiteralPath $uploadsLocales -Recurse -Force }

    Write-Host "=== SCRIPT DE MIGRACIONES ==="
    Ejecutar-Comando -Comando "dotnet" -Argumentos @("ef", "migrations", "script", "--idempotent", "--project", $proyectoWeb, "--startup-project", $proyectoWeb, "--output", $scriptMigraciones)

    $obligatorios = @("saas.dll", "web.config", "appsettings.json", "appsettings.Production.json")
    foreach ($nombre in $obligatorios) {
        $archivo = Join-Path $directorioAplicacion $nombre
        if (-not (Test-Path $archivo)) { throw "Falta un archivo obligatorio: $archivo" }
    }
    if (-not (Test-Path $scriptMigraciones)) { throw "No se generó el script de migraciones." }

    Write-Host "=== COMPRESIÓN ==="
    Compress-Archive -Path $directorioAplicacion, $scriptMigraciones -DestinationPath $archivoZip -Force
    $hash = Get-FileHash $archivoZip -Algorithm SHA256
    $hash.Hash | Set-Content -Path "$archivoZip.sha256.txt" -Encoding ascii

    Write-Host ""
    Write-Host "Paquete generado correctamente."
    Write-Host "ZIP: $archivoZip"
    Write-Host "SHA256: $($hash.Hash)"
}
finally {
    Pop-Location
}
