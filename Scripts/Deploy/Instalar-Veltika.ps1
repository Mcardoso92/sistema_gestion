[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PaqueteZip,
    [Parameter(Mandatory)][string]$HashEsperado,
    [string]$Sitio = "Veltika",
    [string]$AppPool = "VeltikaPool",
    [string]$RutaAplicacion = "C:\inetpub\Veltika",
    [string]$InstanciaSql = ".\SQLEXPRESS",
    [string]$BaseDatos = "Veltika_DB",
    [string]$HostPrueba = "www.veltika.com.ar",
    [ValidateSet("Machine", "AppPool")][string]$OrigenVariables = "Machine",
    [string]$ScriptBackup = "C:\Scripts\Veltika\Backup-Veltika.ps1",
    [string]$DirectorioBackups = "C:\VeltikaBackups",
    [string]$PrefijoRespaldo = "Veltika",
    [ValidateRange(1, 10)][int]$CantidadRespaldosConservar = 2,
    [ValidateRange(128, 4096)][int]$MargenEspacioMB = 512
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw "Ejecuta PowerShell como administrador." }
}

function Verificar-Variables {
    param(
        [Parameter(Mandatory)][string]$NombreAppPool,
        [Parameter(Mandatory)][ValidateSet("Machine", "AppPool")][string]$Origen,
        [Parameter(Mandatory)][string]$BaseDatosEsperada
    )

    $requeridas = @("ConnectionStrings__SaasDbContext", "EmailSettings__Host", "EmailSettings__Port", "EmailSettings__UserName", "EmailSettings__Password", "EmailSettings__FromEmail", "EmailSettings__FromName", "EmailSettings__UseSsl")

    if ($Origen -eq "Machine") {
        # Produccion conserva las variables globales actuales. Si el ambiente no
        # esta definido, ASP.NET Core utiliza Production de forma predeterminada.
        $faltantes = foreach ($nombre in $requeridas) {
            if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($nombre, "Machine"))) { $nombre }
        }
    }
    else {
        if (-not (Test-Path "IIS:\AppPools\$NombreAppPool")) {
            throw "No existe el App Pool '$NombreAppPool'."
        }

        $requeridasAppPool = @("ASPNETCORE_ENVIRONMENT") + $requeridas
        $filtro = "system.applicationHost/applicationPools/add[@name='$NombreAppPool']/environmentVariables"
        $configuracion = Get-WebConfiguration -PSPath "MACHINE/WEBROOT/APPHOST" -Filter $filtro
        $variablesConfiguradas = @{}
        foreach ($elemento in $configuracion.Collection) {
            $variablesConfiguradas[[string]$elemento.GetAttributeValue("name")] = [string]$elemento.GetAttributeValue("value")
        }

        $faltantes = @($requeridasAppPool | Where-Object {
            -not $variablesConfiguradas.ContainsKey($_) -or [string]::IsNullOrWhiteSpace($variablesConfiguradas[$_])
        })

        if (-not $faltantes) {
            if ($variablesConfiguradas["ASPNETCORE_ENVIRONMENT"] -cne "Staging") {
                throw "El App Pool '$NombreAppPool' debe utilizar ASPNETCORE_ENVIRONMENT=Staging."
            }

            $baseEscapada = [Regex]::Escape($BaseDatosEsperada)
            $patronBase = "(?i)(Database|Initial Catalog)\s*=\s*$baseEscapada(?:\s*;|\s*$)"
            if ($variablesConfiguradas["ConnectionStrings__SaasDbContext"] -notmatch $patronBase) {
                throw "La connection string del App Pool '$NombreAppPool' no apunta a '$BaseDatosEsperada'."
            }
        }
    }

    if ($faltantes) { throw "Faltan variables de entorno: $($faltantes -join ', ')" }
}

function Detener-AplicacionIis {
    param(
        [Parameter(Mandatory)][string]$NombreSitio,
        [Parameter(Mandatory)][string]$NombreAppPool
    )

    if ((Get-WebsiteState -Name $NombreSitio).Value -ne "Stopped") {
        Stop-WebSite -Name $NombreSitio
    }

    if ((Get-WebAppPoolState -Name $NombreAppPool).Value -ne "Stopped") {
        Stop-WebAppPool -Name $NombreAppPool
    }

    $appcmd = "$env:windir\System32\inetsrv\appcmd.exe"
    $limite = (Get-Date).AddSeconds(30)

    do {
        $estadoAppPool = (Get-WebAppPoolState -Name $NombreAppPool).Value
        $procesosActivos = @(& $appcmd list wp "/apppool.name:$NombreAppPool")

        if ($estadoAppPool -eq "Stopped" -and $procesosActivos.Count -eq 0) {
            return
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $limite)

    throw "IIS no libero los archivos de la aplicacion dentro de 30 segundos."
}

function Verificar-BackupsAislados {
    param(
        [Parameter(Mandatory)][ValidateSet("Machine", "AppPool")][string]$Origen,
        [Parameter(Mandatory)][string]$RutaScriptBackup,
        [Parameter(Mandatory)][string]$RutaDirectorioBackups,
        [Parameter(Mandatory)][string]$Prefijo
    )

    if ($Origen -ne "AppPool") { return }

    $scriptProductivo = [IO.Path]::GetFullPath("C:\Scripts\Veltika\Backup-Veltika.ps1")
    $directorioProductivo = [IO.Path]::GetFullPath("C:\VeltikaBackups").TrimEnd("\")
    $scriptRecibido = [IO.Path]::GetFullPath($RutaScriptBackup)
    $directorioRecibido = [IO.Path]::GetFullPath($RutaDirectorioBackups).TrimEnd("\")

    if ($scriptRecibido -ieq $scriptProductivo -or
        $directorioRecibido -ieq $directorioProductivo -or
        $Prefijo -ieq "Veltika") {
        throw "QA debe utilizar script, carpeta y prefijo de backup exclusivos."
    }
}

function Obtener-TamanioDirectorio {
    param([Parameter(Mandatory)][string]$Ruta)

    if (-not (Test-Path -LiteralPath $Ruta -PathType Container)) { return [int64]0 }

    $medicion = Get-ChildItem -LiteralPath $Ruta -Recurse -File -Force -ErrorAction Stop |
        Measure-Object -Property Length -Sum

    if ($null -eq $medicion.Sum) { return [int64]0 }
    return [int64]$medicion.Sum
}

function Obtener-TamanioZipDescomprimido {
    param([Parameter(Mandatory)][string]$RutaZip)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archivo = [IO.Compression.ZipFile]::OpenRead($RutaZip)
    try {
        $medicion = $archivo.Entries | Measure-Object -Property Length -Sum
        if ($null -eq $medicion.Sum) { return [int64]0 }
        return [int64]$medicion.Sum
    }
    finally {
        $archivo.Dispose()
    }
}

function Obtener-TamanioAplicacionZip {
    param([Parameter(Mandatory)][string]$RutaZip)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archivo = [IO.Compression.ZipFile]::OpenRead($RutaZip)
    try {
        $medicion = $archivo.Entries |
            Where-Object {
                $rutaEntrada = $_.FullName.Replace('\', '/')
                $rutaEntrada -like "Aplicacion/*" -and -not $rutaEntrada.EndsWith("/")
            } |
            Measure-Object -Property Length -Sum

        if ($null -eq $medicion.Sum) { return [int64]0 }
        return [int64]$medicion.Sum
    }
    finally {
        $archivo.Dispose()
    }
}

function Formatear-GB {
    param([Parameter(Mandatory)][int64]$Bytes)
    return "{0:N2} GB" -f ($Bytes / 1GB)
}

function Verificar-EspacioDisponible {
    param(
        [Parameter(Mandatory)][string]$RutaZip,
        [Parameter(Mandatory)][string]$RutaActual,
        [Parameter(Mandatory)][int]$MargenMB
    )

    $unidad = [IO.Path]::GetPathRoot([IO.Path]::GetFullPath($RutaActual))
    $disco = [IO.DriveInfo]::new($unidad)
    $tamanioActual = Obtener-TamanioDirectorio -Ruta $RutaActual
    $tamanioUploads = Obtener-TamanioDirectorio -Ruta (Join-Path $RutaActual "wwwroot\uploads")
    $tamanioZipExpandido = Obtener-TamanioZipDescomprimido -RutaZip $RutaZip
    $tamanioAplicacionNueva = Obtener-TamanioAplicacionZip -RutaZip $RutaZip
    $margen = [int64]$MargenMB * 1MB

    # Durante el pico conviven la extraccion, el backup de archivos, la
    # publicacion anterior y la copia nueva. La publicacion anterior es un
    # movimiento dentro del mismo disco, por eso no suma espacio adicional.
    $necesario = $tamanioZipExpandido + $tamanioActual + $tamanioAplicacionNueva + $tamanioUploads + $margen
    $disponible = [int64]$disco.AvailableFreeSpace

    Write-Host "Espacio disponible: $(Formatear-GB $disponible)"
    Write-Host "Espacio requerido estimado: $(Formatear-GB $necesario)"

    if ($disponible -lt $necesario) {
        throw "Espacio insuficiente. Se requieren aproximadamente $(Formatear-GB $necesario) y hay $(Formatear-GB $disponible). No se detuvo IIS ni se modifico la aplicacion."
    }
}

function Eliminar-DirectorioControlado {
    param(
        [Parameter(Mandatory)][IO.DirectoryInfo]$Directorio,
        [Parameter(Mandatory)][string]$RaizPermitida
    )

    $raiz = [IO.Path]::GetFullPath($RaizPermitida).TrimEnd('\') + '\'
    $destino = [IO.Path]::GetFullPath($Directorio.FullName)
    if (-not $destino.StartsWith($raiz, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Se rechazo la eliminacion fuera de la raiz permitida: $destino"
    }

    Remove-Item -LiteralPath $destino -Recurse -Force
    Write-Host "Eliminado: $destino"
}

function Aplicar-RetencionDirectorios {
    param(
        [Parameter(Mandatory)][string]$Raiz,
        [Parameter(Mandatory)][string]$Filtro,
        [Parameter(Mandatory)][int]$Conservar
    )

    if (-not (Test-Path -LiteralPath $Raiz -PathType Container)) { return }

    Get-ChildItem -LiteralPath $Raiz -Directory -Filter $Filtro -Force |
        Sort-Object -Property Name -Descending |
        Select-Object -Skip $Conservar |
        ForEach-Object {
            Eliminar-DirectorioControlado -Directorio $_ -RaizPermitida $Raiz
        }
}

function Aplicar-RetencionIis {
    param(
        [Parameter(Mandatory)][string]$Prefijo,
        [Parameter(Mandatory)][int]$Conservar
    )

    $appcmd = "$env:windir\System32\inetsrv\appcmd.exe"
    $patron = '^' + [Regex]::Escape($Prefijo) + '-\d{8}-\d{6}$'
    $respaldos = @(& $appcmd list backup | ForEach-Object {
        if ($_ -match '^BACKUP "([^"]+)"') {
            # Guardamos el nombre antes de aplicar la segunda expresion regular,
            # porque PowerShell reemplaza automaticamente el contenido de $matches.
            $nombreBackup = $matches[1]
            if ($nombreBackup -match $patron) {
                $nombreBackup
            }
        }
    })

    $respaldos |
        Sort-Object -Descending |
        Select-Object -Skip $Conservar |
        ForEach-Object {
            & $appcmd delete backup $_ | Out-Host
            if ($LASTEXITCODE -ne 0) { throw "No se pudo eliminar el backup IIS '$_'." }
        }
}

function Aplicar-LimpiezaDeploy {
    param(
        [Parameter(Mandatory)][string]$RutaApp,
        [Parameter(Mandatory)][string]$RutaBackups,
        [Parameter(Mandatory)][string]$PrefijoIis,
        [Parameter(Mandatory)][int]$Conservar,
        [string]$TrabajoExcluir
    )

    $raizDeploy = "C:\Deploy"
    if (Test-Path -LiteralPath $raizDeploy -PathType Container) {
        Get-ChildItem -LiteralPath $raizDeploy -Directory -Filter "Trabajo-*" -Force |
            Where-Object { -not $TrabajoExcluir -or $_.FullName -ine $TrabajoExcluir } |
            ForEach-Object {
                Eliminar-DirectorioControlado -Directorio $_ -RaizPermitida $raizDeploy
            }
    }

    $padreAplicacion = Split-Path -Parent $RutaApp
    $nombreAplicacion = Split-Path -Leaf $RutaApp
    Aplicar-RetencionDirectorios -Raiz $padreAplicacion -Filtro "$nombreAplicacion-anterior-*" -Conservar $Conservar
    Aplicar-RetencionDirectorios -Raiz $RutaBackups -Filtro "*-predeploy" -Conservar $Conservar
    Aplicar-RetencionIis -Prefijo $PrefijoIis -Conservar $Conservar
}

Verificar-Administrador
Import-Module WebAdministration
Verificar-Variables -NombreAppPool $AppPool -Origen $OrigenVariables -BaseDatosEsperada $BaseDatos
Verificar-BackupsAislados -Origen $OrigenVariables -RutaScriptBackup $ScriptBackup -RutaDirectorioBackups $DirectorioBackups -Prefijo $PrefijoRespaldo
if (-not (Test-Path -LiteralPath $PaqueteZip)) { throw "No se encontro el paquete: $PaqueteZip" }

$hashReal = (Get-FileHash -LiteralPath $PaqueteZip -Algorithm SHA256).Hash
if ($hashReal -ne $HashEsperado.Trim()) { throw "El SHA256 del paquete no coincide. No se realizara el deploy." }

Write-Host "=== DESTINO DEL DEPLOY ==="
Write-Host "Sitio: $Sitio"
Write-Host "App Pool: $AppPool"
Write-Host "Base: $BaseDatos"
Write-Host "Ruta: $RutaAplicacion"
Write-Host "Retencion: $CantidadRespaldosConservar respaldos por tipo"
Write-Host ""

$confirmacion = Read-Host "Escribi DESPLEGAR para continuar"
if ($confirmacion -cne "DESPLEGAR") { throw "Deploy cancelado." }

$marca = Get-Date -Format "yyyyMMdd-HHmmss"
$directorioTrabajo = "C:\Deploy\Trabajo-$marca"
$respaldoAplicacion = Join-Path $DirectorioBackups "$marca-predeploy"
$directorioPadreAplicacion = Split-Path -Parent $RutaAplicacion
$nombreAplicacion = Split-Path -Leaf $RutaAplicacion
$rutaAnterior = Join-Path $directorioPadreAplicacion "$nombreAplicacion-anterior-$marca"
$backupIis = "$PrefijoRespaldo-$marca"

try {
Write-Host "=== LIMPIEZA Y ESPACIO ==="
Aplicar-LimpiezaDeploy `
    -RutaApp $RutaAplicacion `
    -RutaBackups $DirectorioBackups `
    -PrefijoIis $PrefijoRespaldo `
    -Conservar $CantidadRespaldosConservar

Verificar-EspacioDisponible `
    -RutaZip $PaqueteZip `
    -RutaActual $RutaAplicacion `
    -MargenMB $MargenEspacioMB

New-Item -ItemType Directory -Path $directorioTrabajo -Force | Out-Null
Expand-Archive -LiteralPath $PaqueteZip -DestinationPath $directorioTrabajo -Force
$nuevaAplicacion = Join-Path $directorioTrabajo "Aplicacion"
$scriptMigraciones = Join-Path $directorioTrabajo "Veltika-Migraciones.sql"

if (-not (Test-Path (Join-Path $nuevaAplicacion "saas.dll"))) { throw "El paquete no contiene Aplicacion\saas.dll." }
if (-not (Test-Path $scriptMigraciones)) { throw "El paquete no contiene Veltika-Migraciones.sql." }
if (-not (Test-Path $ScriptBackup)) { throw "No se encontro el script de backup: $ScriptBackup" }

Write-Host "=== BACKUP PREVIO ==="
& $ScriptBackup
if (-not $?) { throw "Fallo el backup previo." }

New-Item -ItemType Directory -Path $respaldoAplicacion -Force | Out-Null
Copy-Item -LiteralPath $RutaAplicacion -Destination (Join-Path $respaldoAplicacion "Aplicacion") -Recurse
Copy-Item -LiteralPath "$env:windir\System32\inetsrv\config\applicationHost.config" -Destination (Join-Path $respaldoAplicacion "applicationHost.config")
& "$env:windir\System32\inetsrv\appcmd.exe" add backup $backupIis

Write-Host "=== INICIO DE MANTENIMIENTO ==="
Detener-AplicacionIis -NombreSitio $Sitio -NombreAppPool $AppPool

Write-Host "=== MIGRACIONES ==="
& sqlcmd -S $InstanciaSql -d $BaseDatos -E -C -I -b -i $scriptMigraciones
if ($LASTEXITCODE -ne 0) { throw "Fallaron las migraciones. La aplicacion permanece detenida." }

Write-Host "=== REEMPLAZO DE APLICACION ==="
Move-Item -LiteralPath $RutaAplicacion -Destination $rutaAnterior
Copy-Item -LiteralPath $nuevaAplicacion -Destination $RutaAplicacion -Recurse

# Los uploads pertenecen al servidor y se conservan entre publicaciones.
$uploadsAnteriores = Join-Path $rutaAnterior "wwwroot\uploads"
$uploadsNuevos = Join-Path $RutaAplicacion "wwwroot\uploads"
New-Item -ItemType Directory -Path $uploadsNuevos -Force | Out-Null
if (Test-Path $uploadsAnteriores) {
    Get-ChildItem -LiteralPath $uploadsAnteriores -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $uploadsNuevos -Recurse -Force
    }
}
& icacls $uploadsNuevos /grant "IIS AppPool\$AppPool`:(OI)(CI)(M)" /T

Write-Host "=== INICIO Y PRUEBA LOCAL ==="
# El sitio y su App Pool se administran de forma individual. Reiniciar IIS completo
# interrumpiria otros ambientes alojados en el mismo servidor, como Veltika-QA.
Start-WebAppPool -Name $AppPool
Start-WebSite -Name $Sitio
$respuesta = Invoke-WebRequest "http://localhost" -Headers @{ Host = $HostPrueba } -UseBasicParsing
if ($respuesta.StatusCode -ne 200) { throw "La prueba local devolvio HTTP $($respuesta.StatusCode)." }

Write-Host "=== BACKUP POSTERIOR ==="
& $ScriptBackup
if (-not $?) { throw "La aplicacion funciona, pero fallo el backup posterior." }

Write-Host "=== RETENCION FINAL ==="
Aplicar-LimpiezaDeploy `
    -RutaApp $RutaAplicacion `
    -RutaBackups $DirectorioBackups `
    -PrefijoIis $PrefijoRespaldo `
    -Conservar $CantidadRespaldosConservar `
    -TrabajoExcluir $directorioTrabajo

Write-Host ""
Write-Host "Deploy finalizado correctamente."
Write-Host "Publicacion anterior: $rutaAnterior"
Write-Host "Backup de archivos: $respaldoAplicacion"
Write-Host "Backup de IIS: $backupIis"
}
finally {
    # El directorio de trabajo nunca debe quedar acumulado, incluso cuando el
    # deploy falla antes o despues de detener la aplicacion.
    if (Test-Path -LiteralPath $directorioTrabajo -PathType Container) {
        $trabajo = Get-Item -LiteralPath $directorioTrabajo -Force
        Eliminar-DirectorioControlado -Directorio $trabajo -RaizPermitida "C:\Deploy"
    }
}
