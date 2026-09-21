[CmdletBinding()]
param(
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_QA_DB",
    [string]$IdentidadAppPool = "IIS APPPOOL\VeltikaQAPool"
)

$ErrorActionPreference = "Stop"

if ($BaseDatos -cne "Veltika_QA_DB") {
    throw "Este script solo puede configurar permisos sobre Veltika_QA_DB."
}

if ($IdentidadAppPool -cne "IIS APPPOOL\VeltikaQAPool") {
    throw "La identidad debe ser exclusivamente IIS APPPOOL\VeltikaQAPool."
}

if ($null -eq (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
    throw "No se encontro sqlcmd. Instalalo o agregalo al PATH antes de continuar."
}

$consulta = @"
SET NOCOUNT ON;

IF DB_ID(N'Veltika_QA_DB') IS NULL
BEGIN
    THROW 51000, N'No existe Veltika_QA_DB.', 1;
END;

IF DB_ID(N'Veltika_DB') IS NOT NULL
   AND EXISTS (
       SELECT 1
       FROM [Veltika_DB].sys.database_principals
       WHERE name = N'IIS APPPOOL\VeltikaQAPool'
   )
BEGIN
    THROW 51001, N'La identidad de QA ya posee acceso a Veltika_DB. Revisar manualmente antes de continuar.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = N'IIS APPPOOL\VeltikaQAPool'
)
BEGIN
    CREATE LOGIN [IIS APPPOOL\VeltikaQAPool] FROM WINDOWS;
END;

USE [Veltika_QA_DB];

IF USER_ID(N'IIS APPPOOL\VeltikaQAPool') IS NULL
BEGIN
    CREATE USER [IIS APPPOOL\VeltikaQAPool]
    FOR LOGIN [IIS APPPOOL\VeltikaQAPool];
END;

IF IS_ROLEMEMBER(N'db_datareader', N'IIS APPPOOL\VeltikaQAPool') <> 1
BEGIN
    ALTER ROLE [db_datareader] ADD MEMBER [IIS APPPOOL\VeltikaQAPool];
END;

IF IS_ROLEMEMBER(N'db_datawriter', N'IIS APPPOOL\VeltikaQAPool') <> 1
BEGIN
    ALTER ROLE [db_datawriter] ADD MEMBER [IIS APPPOOL\VeltikaQAPool];
END;
"@

& sqlcmd -S $InstanciaSql -d "master" -E -C -b -Q $consulta
if ($LASTEXITCODE -ne 0) {
    throw "No se pudo configurar el acceso SQL de QA. Codigo de sqlcmd: $LASTEXITCODE."
}

$consultaVerificar = @"
SET NOCOUNT ON;
USE [Veltika_QA_DB];

IF USER_ID(N'IIS APPPOOL\VeltikaQAPool') IS NULL
   OR IS_ROLEMEMBER(N'db_datareader', N'IIS APPPOOL\VeltikaQAPool') <> 1
   OR IS_ROLEMEMBER(N'db_datawriter', N'IIS APPPOOL\VeltikaQAPool') <> 1
BEGIN
    THROW 51002, N'La identidad de QA no posee los permisos esperados.', 1;
END;
"@

& sqlcmd -S $InstanciaSql -d "master" -E -C -b -Q $consultaVerificar
if ($LASTEXITCODE -ne 0) {
    throw "La configuracion SQL se ejecuto pero no supero la verificacion final."
}

Write-Host "Acceso SQL de QA configurado correctamente."
Write-Host "Identidad: IIS APPPOOL\VeltikaQAPool"
Write-Host "Base: Veltika_QA_DB"
Write-Host "Roles: db_datareader, db_datawriter"
Write-Host "No se otorgaron permisos sobre Veltika_DB."
