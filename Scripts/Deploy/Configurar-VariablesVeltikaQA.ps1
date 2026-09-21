[CmdletBinding()]
param(
    [string]$AppPool = "VeltikaQAPool",
    [Parameter(Mandatory)][string]$EmailHost,
    [int]$EmailPort = 587,
    [Parameter(Mandatory)][string]$EmailUsuario,
    [Parameter(Mandatory)][string]$EmailRemitente,
    [string]$EmailNombre = "Veltika QA",
    [bool]$EmailUsaSsl = $true
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Ejecuta PowerShell como administrador."
    }
}

function Convertir-SecureStringATexto {
    param([Parameter(Mandatory)][Security.SecureString]$Valor)

    $puntero = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Valor)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($puntero)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($puntero)
    }
}

function Establecer-VariableAppPool {
    param(
        [Parameter(Mandatory)][string]$NombreAppPool,
        [Parameter(Mandatory)][string]$Nombre,
        [Parameter(Mandatory)][string]$Valor
    )

    $psPath = "MACHINE/WEBROOT/APPHOST"
    $filtroColeccion = "system.applicationHost/applicationPools/add[@name='$NombreAppPool']/environmentVariables"
    $filtroVariable = "$filtroColeccion/add[@name='$Nombre']"
    $existente = Get-WebConfigurationProperty -PSPath $psPath -Filter $filtroVariable -Name "name" -ErrorAction SilentlyContinue

    if ($null -eq $existente) {
        Add-WebConfigurationProperty -PSPath $psPath -Filter $filtroColeccion -Name "." -Value @{ name = $Nombre; value = $Valor }
        return
    }

    Set-WebConfigurationProperty -PSPath $psPath -Filter $filtroVariable -Name "value" -Value $Valor
}

Verificar-Administrador
Import-Module WebAdministration

if (-not (Test-Path "IIS:\AppPools\$AppPool")) {
    throw "No existe el App Pool de QA '$AppPool'. Crealo antes de configurar sus variables."
}

$conexionSegura = Read-Host "Connection string de Veltika_QA_DB" -AsSecureString
$passwordEmailSegura = Read-Host "Password del correo exclusivo de QA" -AsSecureString
$conexion = Convertir-SecureStringATexto -Valor $conexionSegura
$passwordEmail = Convertir-SecureStringATexto -Valor $passwordEmailSegura

try {
    # Esta validacion evita que un error de tipeo conecte QA con la base productiva.
    if ($conexion -notmatch '(?i)(Database|Initial Catalog)\s*=\s*Veltika_QA_DB(?:\s*;|\s*$)') {
        throw "La connection string debe apuntar explicitamente a Veltika_QA_DB."
    }

    $variables = [ordered]@{
        "ASPNETCORE_ENVIRONMENT"              = "Staging"
        "ConnectionStrings__SaasDbContext"    = $conexion
        "EmailSettings__Host"                 = $EmailHost
        "EmailSettings__Port"                 = $EmailPort.ToString([Globalization.CultureInfo]::InvariantCulture)
        "EmailSettings__UserName"             = $EmailUsuario
        "EmailSettings__Password"             = $passwordEmail
        "EmailSettings__FromEmail"            = $EmailRemitente
        "EmailSettings__FromName"             = $EmailNombre
        "EmailSettings__UseSsl"               = $EmailUsaSsl.ToString().ToLowerInvariant()
    }

    foreach ($variable in $variables.GetEnumerator()) {
        Establecer-VariableAppPool -NombreAppPool $AppPool -Nombre $variable.Key -Valor $variable.Value
    }
}
finally {
    # Reducimos el tiempo durante el cual los secretos permanecen referenciados en memoria.
    $conexion = $null
    $passwordEmail = $null
    $variables = $null
}

Write-Host "Variables aisladas configuradas correctamente para '$AppPool'."
Write-Host "Ambiente: Staging"
Write-Host "Base requerida: Veltika_QA_DB"
Write-Host "Los valores sensibles no se mostraron ni se guardaron en el repositorio."
