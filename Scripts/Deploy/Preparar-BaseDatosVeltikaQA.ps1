[CmdletBinding()]
param(
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_QA_DB"
)

$ErrorActionPreference = "Stop"

if ($BaseDatos -cne "Veltika_QA_DB") {
    throw "Este script solo puede preparar la base aislada Veltika_QA_DB."
}

if ($BaseDatos -ieq "Veltika_DB") {
    throw "La base productiva Veltika_DB no puede utilizarse para QA."
}

if ($null -eq (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
    throw "No se encontro sqlcmd. Instalalo o agregalo al PATH antes de continuar."
}

$consultaCrear = @"
SET NOCOUNT ON;

IF DB_ID(N'Veltika_QA_DB') IS NULL
BEGIN
    CREATE DATABASE [Veltika_QA_DB];
    PRINT N'Base Veltika_QA_DB creada.';
END
ELSE
BEGIN
    PRINT N'La base Veltika_QA_DB ya existe; no se realizaron cambios.';
END;
"@

& sqlcmd -S $InstanciaSql -d "master" -E -C -b -Q $consultaCrear
if ($LASTEXITCODE -ne 0) {
    throw "No se pudo crear o verificar Veltika_QA_DB. Codigo de sqlcmd: $LASTEXITCODE."
}

$consultaVerificar = @"
SET NOCOUNT ON;

IF NOT EXISTS (
    SELECT 1
    FROM sys.databases
    WHERE name = N'Veltika_QA_DB'
      AND state_desc = N'ONLINE'
)
BEGIN
    THROW 51000, N'Veltika_QA_DB no existe o no se encuentra ONLINE.', 1;
END;

SELECT name AS BaseDatos, state_desc AS Estado
FROM sys.databases
WHERE name = N'Veltika_QA_DB';
"@

& sqlcmd -S $InstanciaSql -d "master" -E -C -b -Q $consultaVerificar
if ($LASTEXITCODE -ne 0) {
    throw "Veltika_QA_DB fue localizada pero no supero la verificacion final."
}

Write-Host "Base de QA preparada correctamente."
Write-Host "Instancia: $InstanciaSql"
Write-Host "Base: Veltika_QA_DB"
Write-Host "No se copiaron ni modificaron datos de Veltika_DB."
