$ErrorActionPreference='Continue'
Set-Location $PSScriptRoot
$runtime = Join-Path $PSScriptRoot '.windows-runtime'
$pidsPath = Join-Path $runtime 'procesos.json'
if (Test-Path $pidsPath) {
    try {
        $items = Get-Content $pidsPath -Raw | ConvertFrom-Json
        foreach ($item in @($items)) {
            $process = Get-Process -Id $item.Pid -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Deteniendo $($item.Name) PID $($item.Pid)"
                Stop-Process -Id $item.Pid -Force -ErrorAction SilentlyContinue
            }
        }
    } catch { Write-Warning $_ }
    Remove-Item $pidsPath -Force -ErrorAction SilentlyContinue
}
Get-Process 'Indotel.Caja','INDOTEL.CORE.UI' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
$sqlLocalDb = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
if ($sqlLocalDb) { & $sqlLocalDb.Source stop MSSQLLocalDB | Out-Null }
Write-Host 'Todos los componentes de INDOTEL fueron detenidos.' -ForegroundColor Green
