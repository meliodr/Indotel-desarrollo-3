$ErrorActionPreference='Stop'
Set-Location $PSScriptRoot
function Check([string]$Name,[string]$Url) {
    $r=Invoke-WebRequest -UseBasicParsing -Uri $Url -TimeoutSec 10
    if($r.StatusCode -ne 200){throw "$Name devolvio $($r.StatusCode)"}
    Write-Host "OK: $Name" -ForegroundColor Green
}
Check 'Core' 'http://localhost:5085/health/ready'
Check 'Gateway' 'http://localhost:5185/health/ready'
Check 'Web' 'http://localhost:5234/'
Check 'Pagos' 'http://localhost:5285/health'
Write-Host 'PRUEBA INTEGRAL WINDOWS APROBADA.' -ForegroundColor Green
