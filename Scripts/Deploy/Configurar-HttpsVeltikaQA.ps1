[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ThumbprintCertificado,
    [string]$Sitio = "Veltika-QA",
    [string]$HostQa = "qa.veltika.com.ar"
)

$ErrorActionPreference = "Stop"

function Verificar-Administrador {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Ejecuta PowerShell como administrador."
    }
}

Verificar-Administrador

if ($Sitio -ieq "Veltika" -or $HostQa -ieq "www.veltika.com.ar") {
    throw "Este script no puede modificar el sitio ni el host productivos."
}

Import-Module WebAdministration

if ($null -eq (Get-Website -Name $Sitio -ErrorAction SilentlyContinue)) {
    throw "No existe el sitio IIS de QA '$Sitio'."
}

$thumbprint = ($ThumbprintCertificado -replace '\s', '').ToUpperInvariant()
if ($thumbprint -notmatch '^[A-F0-9]{40,64}$') {
    throw "El thumbprint del certificado no tiene un formato valido."
}

$rutaCertificado = "Cert:\LocalMachine\My\$thumbprint"
if (-not (Test-Path -LiteralPath $rutaCertificado -PathType Leaf)) {
    throw "No se encontro el certificado en LocalMachine\My: $thumbprint"
}

$certificado = Get-Item -LiteralPath $rutaCertificado
$ahora = Get-Date
if ($certificado.NotBefore -gt $ahora -or $certificado.NotAfter -le $ahora) {
    throw "El certificado todavia no es valido o ya esta vencido."
}

$dnsCertificado = @($certificado.DnsNameList | ForEach-Object { $_.Unicode })
$hostCubierto = $dnsCertificado | Where-Object {
    $_ -ieq $HostQa -or
    ($_.StartsWith("*.") -and $HostQa.EndsWith($_.Substring(1), [StringComparison]::OrdinalIgnoreCase))
}
if ($null -eq $hostCubierto) {
    throw "El certificado no incluye el host '$HostQa'. Hosts encontrados: $($dnsCertificado -join ', ')"
}

$bindingEsperado = "*:443:$HostQa"
$bindingHttps = Get-WebBinding -Name $Sitio -Protocol "https" |
    Where-Object { $_.bindingInformation -eq $bindingEsperado } |
    Select-Object -First 1

if ($null -eq $bindingHttps) {
    New-WebBinding -Name $Sitio -Protocol "https" -Port 443 -HostHeader $HostQa -SslFlags 1 | Out-Null
    $bindingHttps = Get-WebBinding -Name $Sitio -Protocol "https" |
        Where-Object { $_.bindingInformation -eq $bindingEsperado } |
        Select-Object -First 1
}

if ($null -eq $bindingHttps) {
    throw "No se pudo crear o localizar el binding HTTPS '$bindingEsperado'."
}

$hashBinding = $bindingHttps.certificateHash
if ($hashBinding -is [byte[]]) {
    $thumbprintActual = [BitConverter]::ToString($hashBinding) -replace '-', ''
}
else {
    $thumbprintActual = [string]$hashBinding -replace '\s|-', ''
}
$thumbprintActual = $thumbprintActual.ToUpperInvariant()

if ([string]::IsNullOrWhiteSpace($thumbprintActual)) {
    # AddSslCertificate trabaja sobre el binding SNI real y evita depender de
    # la representación interna de IIS:\SslBindings, que varía por versión.
    $bindingHttps.AddSslCertificate($thumbprint, "My")
}
elseif ($thumbprintActual -ne $thumbprint) {
    throw "El binding HTTPS ya existe pero utiliza otro certificado. No se reemplazo."
}

Write-Host "HTTPS configurado correctamente para QA."
Write-Host "Sitio: $Sitio"
Write-Host "Host: $HostQa"
Write-Host "Certificado valido hasta: $($certificado.NotAfter)"
Write-Host "El binding productivo no fue modificado."
