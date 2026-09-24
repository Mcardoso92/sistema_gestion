[CmdletBinding()]
param(
    [switch]$ExigirCompleto,
    [string]$Sitio = "Veltika-QA",
    [string]$AppPool = "VeltikaQAPool",
    [string]$RutaAplicacion = "C:\inetpub\Veltika-QA",
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_QA_DB",
    [string]$HostQa = "qa.veltika.com.ar"
)

$ErrorActionPreference = "Stop"
$resultados = [Collections.Generic.List[object]]::new()

function Registrar-Resultado {
    param(
        [Parameter(Mandatory)][string]$Componente,
        [Parameter(Mandatory)][bool]$Correcto,
        [Parameter(Mandatory)][string]$Detalle
    )

    $estado = if ($Correcto) { "OK" } else { "PENDIENTE" }
    $resultados.Add([pscustomobject]@{
        Componente = $Componente
        Estado      = $estado
        Detalle     = $Detalle
    })
}

if ($Sitio -ieq "Veltika" -or
    $AppPool -ieq "VeltikaPool" -or
    $RutaAplicacion.TrimEnd("\") -ieq "C:\inetpub\Veltika" -or
    $BaseDatos -ieq "Veltika_DB" -or
    $HostQa -ieq "www.veltika.com.ar") {
    throw "La verificacion de QA no puede utilizar recursos productivos."
}

$webAdministrationDisponible = $null -ne (Get-Module -ListAvailable -Name WebAdministration)
Registrar-Resultado -Componente "Modulo IIS" -Correcto $webAdministrationDisponible -Detalle "WebAdministration disponible"

if ($webAdministrationDisponible) {
    Import-Module WebAdministration

    $sitioQa = Get-Website -Name $Sitio -ErrorAction SilentlyContinue
    Registrar-Resultado -Componente "Sitio QA" -Correcto ($null -ne $sitioQa) -Detalle $Sitio

    $appPoolExiste = Test-Path "IIS:\AppPools\$AppPool"
    Registrar-Resultado -Componente "App Pool QA" -Correcto $appPoolExiste -Detalle $AppPool

    if ($appPoolExiste) {
        $requeridas = @(
            "ASPNETCORE_ENVIRONMENT",
            "ConnectionStrings__SaasDbContext",
            "EmailSettings__Host",
            "EmailSettings__Port",
            "EmailSettings__UserName",
            "EmailSettings__Password",
            "EmailSettings__FromEmail",
            "EmailSettings__FromName",
            "EmailSettings__UseSsl"
        )
        $filtro = "system.applicationHost/applicationPools/add[@name='$AppPool']/environmentVariables"
        $configuracion = Get-WebConfiguration -PSPath "MACHINE/WEBROOT/APPHOST" -Filter $filtro
        $variables = @{}
        foreach ($elemento in $configuracion.Collection) {
            $variables[[string]$elemento.GetAttributeValue("name")] = [string]$elemento.GetAttributeValue("value")
        }

        $faltantes = @($requeridas | Where-Object {
            -not $variables.ContainsKey($_) -or [string]::IsNullOrWhiteSpace($variables[$_])
        })
        Registrar-Resultado -Componente "Variables QA" -Correcto ($faltantes.Count -eq 0) -Detalle $(
            if ($faltantes.Count -eq 0) { "Completas; valores sensibles ocultos" }
            else { "Faltan: $($faltantes -join ', ')" }
        )

        if ($faltantes.Count -eq 0) {
            $ambienteCorrecto = $variables["ASPNETCORE_ENVIRONMENT"] -ceq "Staging"
            Registrar-Resultado -Componente "Ambiente ASP.NET" -Correcto $ambienteCorrecto -Detalle "Debe ser Staging"

            $baseEscapada = [Regex]::Escape($BaseDatos)
            $conexionCorrecta = $variables["ConnectionStrings__SaasDbContext"] -match "(?i)(Database|Initial Catalog)\s*=\s*$baseEscapada(?:\s*;|\s*$)"
            Registrar-Resultado -Componente "Conexion QA" -Correcto $conexionCorrecta -Detalle "Debe apuntar a $BaseDatos; valor oculto"
        }
    }

    if ($null -ne $sitioQa) {
        $bindingHttp = Get-WebBinding -Name $Sitio -Protocol "http" | Where-Object { $_.bindingInformation -eq "*:80:$HostQa" }
        $bindingHttps = Get-WebBinding -Name $Sitio -Protocol "https" | Where-Object { $_.bindingInformation -eq "*:443:$HostQa" }
        Registrar-Resultado -Componente "Binding HTTP" -Correcto ($null -ne $bindingHttp) -Detalle "*:80:$HostQa"
        Registrar-Resultado -Componente "Binding HTTPS" -Correcto ($null -ne $bindingHttps) -Detalle "*:443:$HostQa"
    }
}

$rutaExiste = Test-Path -LiteralPath $RutaAplicacion -PathType Container
Registrar-Resultado -Componente "Carpeta QA" -Correcto $rutaExiste -Detalle $RutaAplicacion

$sqlcmdDisponible = $null -ne (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)
Registrar-Resultado -Componente "sqlcmd" -Correcto $sqlcmdDisponible -Detalle "Cliente SQL disponible"

if ($sqlcmdDisponible) {
    # Los valores se escapan antes de incorporarlos a las consultas para que la
    # verificacion siga siendo segura si se personalizan los nombres de QA.
    $baseDatosSql = $BaseDatos.Replace("'", "''")
    $principalQa = "IIS APPPOOL\$AppPool"
    $principalQaSql = $principalQa.Replace("'", "''")

    $consultaDb = "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = N'$baseDatosSql' AND state_desc = N'ONLINE';"
    $salidaDb = & sqlcmd -S $InstanciaSql -d "master" -E -C -b -h -1 -W -Q $consultaDb 2>$null
    $baseOnline = $LASTEXITCODE -eq 0 -and (($salidaDb -join "").Trim() -eq "1")
    Registrar-Resultado -Componente "Base QA" -Correcto $baseOnline -Detalle "$BaseDatos ONLINE"

    if ($baseOnline) {
        $consultaPermisos = @"
SET NOCOUNT ON;
SELECT CASE WHEN COUNT(DISTINCT rol.name) = 2 THEN 1 ELSE 0 END
FROM sys.database_role_members membresia
INNER JOIN sys.database_principals rol
    ON rol.principal_id = membresia.role_principal_id
INNER JOIN sys.database_principals miembro
    ON miembro.principal_id = membresia.member_principal_id
WHERE miembro.name = N'$principalQaSql'
  AND rol.name IN (N'db_datareader', N'db_datawriter');
"@
        $salidaPermisos = & sqlcmd -S $InstanciaSql -d $BaseDatos -E -C -b -h -1 -W -Q $consultaPermisos 2>$null
        $permisosCorrectos = $LASTEXITCODE -eq 0 -and (($salidaPermisos -join "").Trim() -eq "1")
        Registrar-Resultado -Componente "Permisos SQL QA" -Correcto $permisosCorrectos -Detalle "Lectura y escritura para $AppPool"
    }
}

try {
    $ipQa = @(Resolve-DnsName $HostQa -Type A -ErrorAction Stop | Where-Object IPAddress | Select-Object -ExpandProperty IPAddress)
    $ipProduccion = @(Resolve-DnsName "www.veltika.com.ar" -Type A -ErrorAction Stop | Where-Object IPAddress | Select-Object -ExpandProperty IPAddress)
    $dnsCorrecto = $ipQa.Count -gt 0 -and @($ipQa | Where-Object { $_ -in $ipProduccion }).Count -gt 0
    Registrar-Resultado -Componente "DNS QA" -Correcto $dnsCorrecto -Detalle $(
        if ($ipQa.Count -gt 0) { "$HostQa -> $($ipQa -join ', ')" } else { "Sin registro A" }
    )
}
catch {
    Registrar-Resultado -Componente "DNS QA" -Correcto $false -Detalle "No se pudo resolver $HostQa"
}

$scriptsRequeridos = @(
    "Instalar-Veltika.ps1",
    "Instalar-VeltikaQA.ps1",
    "Backup-VeltikaQA.ps1"
)
$directorioScripts = "C:\Scripts\Veltika"
$scriptsFaltantes = @($scriptsRequeridos | Where-Object { -not (Test-Path (Join-Path $directorioScripts $_) -PathType Leaf) })
Registrar-Resultado -Componente "Scripts QA" -Correcto ($scriptsFaltantes.Count -eq 0) -Detalle $(
    if ($scriptsFaltantes.Count -eq 0) { "Instaladores y backup disponibles" }
    else { "Faltan: $($scriptsFaltantes -join ', ')" }
)

$resultados | Format-Table -AutoSize -Wrap
$pendientes = @($resultados | Where-Object Estado -ne "OK")

Write-Host ""
Write-Host "Correctos: $($resultados.Count - $pendientes.Count)"
Write-Host "Pendientes: $($pendientes.Count)"

if ($ExigirCompleto -and $pendientes.Count -gt 0) {
    throw "La infraestructura QA todavia tiene verificaciones pendientes."
}
