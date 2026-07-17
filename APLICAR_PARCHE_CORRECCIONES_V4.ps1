[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$root = $PSScriptRoot

if (-not (Test-Path (Join-Path $root 'INDOTEL.sln'))) {
    throw 'Extraiga este parche directamente dentro de la carpeta INDOTEL-Proyecto-Completo.'
}

Write-Host '==> Deteniendo procesos de INDOTEL' -ForegroundColor Cyan
if (Test-Path (Join-Path $root 'DETENER_TODO_EN_WINDOWS.ps1')) {
    & (Join-Path $root 'DETENER_TODO_EN_WINDOWS.ps1') | Out-Host
}
Get-Process -Name 'Indotel.Caja','INDOTEL.CORE.UI' -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host '==> Eliminando pantallas duplicadas reemplazadas por los flujos canonicos' -ForegroundColor Cyan
$obsolete = @(
    'INDOTEL.Web\Views\Comercial\Login.cshtml',
    'INDOTEL.Web\Views\Reclamacion\Crear.cshtml'
)
foreach ($relative in $obsolete) {
    $path = Join-Path $root $relative
    if (Test-Path $path) { Remove-Item $path -Force }
}

Write-Host '==> Compilando, probando e iniciando la version corregida' -ForegroundColor Cyan
& (Join-Path $root '08_APLICAR_Y_VALIDAR_CORRECCIONES_V4.ps1')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
