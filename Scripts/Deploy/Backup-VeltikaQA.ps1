[CmdletBinding()]
param(
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_QA_DB",
    [string]$DirectorioBackups = "C:\Backups\Veltika-QA"
)

$ErrorActionPreference = "Stop"

if ($BaseDatos -cne "Veltika_QA_DB") {
    throw "Este script solo puede respaldar Veltika_QA_DB."
}

$directorioProductivo = [IO.Path]::GetFullPath("C:\Backups\Veltika").TrimEnd("\")
$directorioQa = [IO.Path]::GetFullPath($DirectorioBackups).TrimEnd("\")
if ($directorioQa -ieq $directorioProductivo) {
    throw "QA no puede guardar backups en la carpeta productiva."
}

if ($null -eq (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
    throw "No se encontro sqlcmd. Instalalo o agregalo al PATH antes de continuar."
}

New-Item -ItemType Directory -Path $directorioQa -Force | Out-Null

$marca = Get-Date -Format "yyyyMMdd-HHmmss"
$archivoBackup = Join-Path $directorioQa "Veltika_QA_DB_$marca.bak"
$rutaSql = $archivoBackup.Replace("'", "''")

$consultaBackup = @"
SET NOCOUNT ON;

IF DB_ID(N'Veltika_QA_DB') IS NULL
BEGIN
    THROW 51000, N'No existe Veltika_QA_DB.', 1;
END;

BACKUP DATABASE [Veltika_QA_DB]
TO DISK = N'$rutaSql'
WITH COPY_ONLY, CHECKSUM, STATS = 10;
"@

& sqlcmd -S $InstanciaSql -d "master" -E -C -b -Q $consultaBackup
if ($LASTEXITCODE -ne 0) {
    throw "Fallo el backup de QA. Codigo de sqlcmd: $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $archivoBackup -PathType Leaf)) {
    throw "SQL Server informo exito pero no se encontro el archivo de backup."
}

# La verificacion se ejecuta en una segunda llamada. De esta forma nunca se
# intenta validar un archivo inexistente cuando BACKUP DATABASE falla.
$consultaVerificar = @"
SET NOCOUNT ON;

RESTORE VERIFYONLY
FROM DISK = N'$rutaSql'
WITH CHECKSUM;
"@

& sqlcmd -S $InstanciaSql -d "master" -E -C -b -Q $consultaVerificar
if ($LASTEXITCODE -ne 0) {
    throw "Fallo la verificacion del backup de QA. Codigo de sqlcmd: $LASTEXITCODE."
}

$backup = Get-Item -LiteralPath $archivoBackup
if ($backup.Length -le 0) {
    throw "El archivo de backup fue creado sin contenido."
}

Write-Host "Backup de QA creado y verificado correctamente."
Write-Host "Base: Veltika_QA_DB"
Write-Host "Archivo: $archivoBackup"
Write-Host "Tamanio: $($backup.Length) bytes"
Write-Host "No se utilizo el almacenamiento productivo ni se realizo una subida a S3."
