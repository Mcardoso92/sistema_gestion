[CmdletBinding()]
param(
    [string]$Sitio = "Veltika",
    [string]$AppPool = "VeltikaPool",
    [string]$Url = "https://www.veltika.com.ar",
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_DB",
    [string]$DirectorioBackups = "C:\Backups\Veltika",
    [int]$AntiguedadMaximaBackupHoras = 30,
    [string]$DirectorioLogs = "C:\VeltikaLogs\Monitoreo",
    [int]$DiasRetencionLogs = 30
)

$ErrorActionPreference = "Stop"
$resultados = [System.Collections.Generic.List[object]]::new()

function Agregar-Resultado {
    param(
        [Parameter(Mandatory)][string]$Componente,
        [Parameter(Mandatory)][bool]$Correcto,
        [Parameter(Mandatory)][string]$Detalle
    )

    $resultados.Add([pscustomobject]@{
        Componente = $Componente
        Estado = if ($Correcto) { "OK" } else { "ERROR" }
        Detalle = $Detalle
    })
}

try {
    Import-Module WebAdministration

    $estadoSitio = (Get-WebsiteState -Name $Sitio).Value
    Agregar-Resultado -Componente "Sitio IIS" -Correcto ($estadoSitio -eq "Started") -Detalle "${Sitio}: $estadoSitio"

    $estadoPool = (Get-WebAppPoolState -Name $AppPool).Value
    Agregar-Resultado -Componente "App Pool" -Correcto ($estadoPool -eq "Started") -Detalle "${AppPool}: $estadoPool"
}
catch {
    Agregar-Resultado -Componente "IIS" -Correcto $false -Detalle $_.Exception.Message
}

try {
    $respuesta = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 30
    Agregar-Resultado -Componente "HTTPS" -Correcto ($respuesta.StatusCode -eq 200) -Detalle "$Url -> HTTP $($respuesta.StatusCode)"
}
catch {
    Agregar-Resultado -Componente "HTTPS" -Correcto $false -Detalle $_.Exception.Message
}

try {
    if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
        throw "No se encontro sqlcmd."
    }

    $baseSql = $BaseDatos.Replace("'", "''")
    $salidaSql = & sqlcmd -S $InstanciaSql -d master -E -C -b -h -1 -W -Q "SET NOCOUNT ON; SELECT state_desc FROM sys.databases WHERE name = N'$baseSql';"
    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd finalizo con codigo $LASTEXITCODE."
    }

    $estadoBase = $salidaSql | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -First 1
    Agregar-Resultado -Componente "Base de datos" -Correcto ($estadoBase -eq "ONLINE") -Detalle "${BaseDatos}: $estadoBase"
}
catch {
    Agregar-Resultado -Componente "Base de datos" -Correcto $false -Detalle $_.Exception.Message
}

try {
    $ultimoBackup = Get-ChildItem -LiteralPath $DirectorioBackups -Filter "*.bak" -File |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $ultimoBackup) {
        throw "No se encontraron archivos .bak."
    }

    $antiguedadHoras = ((Get-Date) - $ultimoBackup.LastWriteTime).TotalHours
    $backupVigente = $ultimoBackup.Length -gt 0 -and $antiguedadHoras -le $AntiguedadMaximaBackupHoras
    $detalleBackup = "{0}; {1:N1} horas; {2} bytes" -f $ultimoBackup.Name, $antiguedadHoras, $ultimoBackup.Length
    Agregar-Resultado -Componente "Backup" -Correcto $backupVigente -Detalle $detalleBackup
}
catch {
    Agregar-Resultado -Componente "Backup" -Correcto $false -Detalle $_.Exception.Message
}

$fecha = Get-Date
$fallos = @($resultados | Where-Object Estado -eq "ERROR")
$estadoGeneral = if ($fallos.Count -eq 0) { "OK" } else { "ERROR" }
$registro = [pscustomobject]@{
    Fecha = $fecha.ToString("o")
    Estado = $estadoGeneral
    Resultados = $resultados
}

New-Item -ItemType Directory -Path $DirectorioLogs -Force | Out-Null
$archivoLog = Join-Path $DirectorioLogs ("salud-{0}.jsonl" -f $fecha.ToString("yyyy-MM-dd"))
$registro | ConvertTo-Json -Depth 4 -Compress | Add-Content -LiteralPath $archivoLog -Encoding UTF8

Get-ChildItem -LiteralPath $DirectorioLogs -Filter "salud-*.jsonl" -File |
    Where-Object LastWriteTime -lt $fecha.AddDays(-$DiasRetencionLogs) |
    Remove-Item -Force

$resultados | Format-Table -AutoSize
Write-Host "Estado general: $estadoGeneral"
Write-Host "Log: $archivoLog"

if ($fallos.Count -gt 0) {
    $detalleFallos = ($fallos | ForEach-Object { "$($_.Componente): $($_.Detalle)" }) -join " | "
    if ([System.Diagnostics.EventLog]::SourceExists("Veltika Monitor")) {
        Write-EventLog -LogName Application -Source "Veltika Monitor" -EntryType Error -EventId 1001 -Message $detalleFallos
    }

    throw "La verificacion de salud detecto fallos: $detalleFallos"
}
