$ErrorActionPreference='Continue'
Set-Location $PSScriptRoot
$runtime = Join-Path $PSScriptRoot '.windows-runtime'
$pidsPath = Join-Path $runtime 'procesos.json'
Write-Host '=== PROCESOS ===' -ForegroundColor Cyan
if (Test-Path $pidsPath) {
    $items = Get-Content $pidsPath -Raw | ConvertFrom-Json
    foreach ($item in @($items)) {
        $p = Get-Process -Id $item.Pid -ErrorAction SilentlyContinue
        if ($p) { Write-Host "OK  $($item.Name) PID $($item.Pid)" -ForegroundColor Green }
        else { Write-Host "OFF $($item.Name)" -ForegroundColor Red }
    }
} else { Write-Host 'No existe registro de procesos.' -ForegroundColor Yellow }

Write-Host "`n=== SERVICIOS ===" -ForegroundColor Cyan
$targets = @(
    @('Core','http://localhost:5085/health/ready'),
    @('Gateway','http://localhost:5185/health/ready'),
    @('Web','http://localhost:5234/'),
    @('Pagos','http://localhost:5285/health')
)
foreach ($target in $targets) {
    try {
        $r=Invoke-WebRequest -UseBasicParsing $target[1] -TimeoutSec 4
        Write-Host "OK  $($target[0]) $($target[1])" -ForegroundColor Green
    } catch { Write-Host "OFF $($target[0]) $($target[1])" -ForegroundColor Red }
}
$sqlLocalDb = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
if ($sqlLocalDb) { Write-Host "`n=== SQL LOCALDB ===" -ForegroundColor Cyan; & $sqlLocalDb.Source info MSSQLLocalDB }
