$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if (-not (Test-Path '.\vm-config.json')) {
    Write-Host 'No existe vm-config.json; ejecutando detección automática...' -ForegroundColor Yellow
    & powershell -NoProfile -ExecutionPolicy Bypass -File '.\CONFIGURAR_VM_WINDOWS.ps1' -OmitirCompilacion
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

$config = Get-Content '.\vm-config.json' -Raw | ConvertFrom-Json
$targets = @(
    @{ Name='Core'; Url=([string]$config.CoreUrl).TrimEnd('/') + '/health/ready' },
    @{ Name='Gateway'; Url=([string]$config.GatewayUrl).TrimEnd('/') + '/health/ready' },
    @{ Name='Web'; Url=([string]$config.WebUrl).TrimEnd('/') + '/' }
)

foreach ($target in $targets) {
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $target.Url -TimeoutSec 12
        Write-Host ("OK {0}: HTTP {1}" -f $target.Name, $response.StatusCode) -ForegroundColor Green
    }
    catch {
        Write-Host ("ERROR {0}: {1}" -f $target.Name, $_.Exception.Message) -ForegroundColor Red
        Write-Host 'Ejecute nuevamente PREPARAR_VM_WINDOWS_COMPLETA.bat para redetectar Ubuntu.' -ForegroundColor Yellow
        exit 1
    }
}
