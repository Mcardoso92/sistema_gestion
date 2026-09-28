[CmdletBinding()]
param(
    [string]$ScriptMonitor = "C:\Scripts\Veltika\Verificar-SaludVeltika.ps1",
    [string]$NombreTarea = "Veltika - Monitoreo de salud",
    [int]$IntervaloMinutos = 5
)

$ErrorActionPreference = "Stop"

$identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
$principalActual = [Security.Principal.WindowsPrincipal]::new($identidad)
if (-not $principalActual.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Ejecuta PowerShell como administrador."
}

if (-not (Test-Path -LiteralPath $ScriptMonitor -PathType Leaf)) {
    throw "No se encontro el monitor: $ScriptMonitor"
}

if ($IntervaloMinutos -lt 1) {
    throw "El intervalo debe ser de al menos un minuto."
}

if (-not [System.Diagnostics.EventLog]::SourceExists("Veltika Monitor")) {
    New-EventLog -LogName Application -Source "Veltika Monitor"
}

$powershell = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
$argumentos = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$ScriptMonitor`""
$accion = New-ScheduledTaskAction -Execute $powershell -Argument $argumentos
$disparador = New-ScheduledTaskTrigger `
    -Once `
    -At (Get-Date).AddMinutes(1) `
    -RepetitionInterval (New-TimeSpan -Minutes $IntervaloMinutos)
$principalTarea = New-ScheduledTaskPrincipal `
    -UserId "SYSTEM" `
    -LogonType ServiceAccount `
    -RunLevel Highest
$configuracion = New-ScheduledTaskSettingsSet `
    -StartWhenAvailable `
    -MultipleInstances IgnoreNew `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 4)

Register-ScheduledTask `
    -TaskName $NombreTarea `
    -Action $accion `
    -Trigger $disparador `
    -Principal $principalTarea `
    -Settings $configuracion `
    -Description "Verifica IIS, HTTPS, SQL Server y vigencia de backups de Veltika." `
    -Force | Out-Null

Write-Host "Monitoreo de Veltika instalado correctamente."
Write-Host "Tarea: $NombreTarea"
Write-Host "Intervalo: $IntervaloMinutos minutos"
Write-Host "Los fallos se registran en el log Application con origen 'Veltika Monitor'."

