[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArchivoBackup,
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseTemporal = "Veltika_RestoreTest_$(Get-Date -Format 'yyyyMMdd_HHmmss')",
    [switch]$ConservarBaseTemporal
)

$ErrorActionPreference = "Stop"

function Ejecutar-SqlCmd {
    param(
        [Parameter(Mandatory)][string]$Consulta,
        [string]$BaseDatos = "master",
        [switch]$DevolverSalida
    )

    $argumentos = @(
        "-S", $InstanciaSql,
        "-d", $BaseDatos,
        "-E",
        "-C",
        "-b",
        "-Q", $Consulta
    )

    if ($DevolverSalida) {
        $argumentos += @("-h", "-1", "-W", "-s", "|")
        $salida = & sqlcmd @argumentos
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo sqlcmd con codigo $LASTEXITCODE."
        }

        return @($salida)
    }

    & sqlcmd @argumentos
    if ($LASTEXITCODE -ne 0) {
        throw "Fallo sqlcmd con codigo $LASTEXITCODE."
    }
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "No se encontro sqlcmd."
}

if (-not (Test-Path -LiteralPath $ArchivoBackup -PathType Leaf)) {
    throw "No se encontro el backup: $ArchivoBackup"
}

if ($BaseTemporal -notmatch '^[A-Za-z][A-Za-z0-9_]{0,127}$') {
    throw "El nombre de la base temporal no es valido."
}

$archivoResuelto = (Resolve-Path -LiteralPath $ArchivoBackup).Path
$archivoSql = $archivoResuelto.Replace("'", "''")
$baseSql = $BaseTemporal.Replace("]", "]]" )
$baseCreada = $false

try {
    $baseTemporalSql = $BaseTemporal.Replace("'", "''")
    $existe = Ejecutar-SqlCmd -Consulta "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = N'$baseTemporalSql';" -DevolverSalida
    if (($existe | Where-Object { $_.Trim() -eq "1" }).Count -gt 0) {
        throw "Ya existe la base temporal '$BaseTemporal'. No se modifico ninguna base."
    }

    Write-Host "=== VERIFICACION DEL ARCHIVO ==="
    Ejecutar-SqlCmd -Consulta "RESTORE VERIFYONLY FROM DISK = N'$archivoSql';"

    # FILELISTONLY permite descubrir los nombres logicos sin asumir los de produccion.
    $listaArchivos = Ejecutar-SqlCmd -Consulta "RESTORE FILELISTONLY FROM DISK = N'$archivoSql';" -DevolverSalida
    $archivoDatos = $null
    $archivoLog = $null

    foreach ($linea in $listaArchivos) {
        $columnas = @($linea -split '\|')
        if ($columnas.Count -lt 3) { continue }

        $nombreLogico = $columnas[0].Trim()
        $tipo = $columnas[2].Trim()
        if (-not $archivoDatos -and $tipo -eq "D") { $archivoDatos = $nombreLogico }
        if (-not $archivoLog -and $tipo -eq "L") { $archivoLog = $nombreLogico }
    }

    if ([string]::IsNullOrWhiteSpace($archivoDatos) -or [string]::IsNullOrWhiteSpace($archivoLog)) {
        throw "No se pudieron identificar los archivos logicos de datos y log del backup."
    }

    $rutasPredeterminadas = Ejecutar-SqlCmd -Consulta "SET NOCOUNT ON; SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultDataPath')) + '|' + CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultLogPath'));" -DevolverSalida
    $lineaRutas = $rutasPredeterminadas | Where-Object { $_ -match '\|' } | Select-Object -First 1
    if (-not $lineaRutas) {
        throw "No se pudieron obtener las rutas predeterminadas de SQL Server."
    }

    $rutas = @($lineaRutas -split '\|', 2)
    $rutaDatos = Join-Path $rutas[0].Trim() "$BaseTemporal.mdf"
    $rutaLog = Join-Path $rutas[1].Trim() "${BaseTemporal}_log.ldf"
    $rutaDatosSql = $rutaDatos.Replace("'", "''")
    $rutaLogSql = $rutaLog.Replace("'", "''")
    $archivoDatosSql = $archivoDatos.Replace("'", "''")
    $archivoLogSql = $archivoLog.Replace("'", "''")

    Write-Host "=== RESTAURACION AISLADA ==="
    Ejecutar-SqlCmd -Consulta "RESTORE DATABASE [$baseSql] FROM DISK = N'$archivoSql' WITH MOVE N'$archivoDatosSql' TO N'$rutaDatosSql', MOVE N'$archivoLogSql' TO N'$rutaLogSql', RECOVERY, STATS = 10;"
    $baseCreada = $true

    Write-Host "=== VERIFICACION DE INTEGRIDAD ==="
    Ejecutar-SqlCmd -Consulta "DBCC CHECKDB (N'$baseTemporalSql') WITH NO_INFOMSGS;"

    $estado = Ejecutar-SqlCmd -Consulta "SET NOCOUNT ON; SELECT state_desc FROM sys.databases WHERE name = N'$baseTemporalSql';" -DevolverSalida
    if (-not ($estado | Where-Object { $_.Trim() -eq "ONLINE" })) {
        throw "La base restaurada no quedo ONLINE."
    }

    Write-Host "Backup restaurado y verificado correctamente."
    Write-Host "Origen: $archivoResuelto"
    Write-Host "Base temporal: $BaseTemporal"
}
finally {
    if ($baseCreada -and -not $ConservarBaseTemporal) {
        Write-Host "=== LIMPIEZA DE LA PRUEBA ==="
        Ejecutar-SqlCmd -Consulta "ALTER DATABASE [$baseSql] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$baseSql];"
        Write-Host "Base temporal eliminada. Produccion no fue modificada."
    }
    elseif ($baseCreada) {
        Write-Host "La base temporal se conservo por solicitud: $BaseTemporal"
    }
}
