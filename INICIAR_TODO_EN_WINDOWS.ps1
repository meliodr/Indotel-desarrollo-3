[CmdletBinding()]
param([switch]$SinInterfaces)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$root = $PSScriptRoot
$runtime = Join-Path $root '.windows-runtime'
$logs = Join-Path $runtime 'logs'
$pidsPath = Join-Path $runtime 'procesos.json'
New-Item -ItemType Directory -Force -Path $runtime, $logs | Out-Null

function Find-LocalDb {
    $cmd = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $item = Get-ChildItem "$env:ProgramFiles\Microsoft SQL Server" -Recurse -Filter SqlLocalDB.exe -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
    if ($item) { return $item.FullName }
    throw 'SQL LocalDB no esta instalado. Ejecute 01_PREPARAR_TODO_EN_WINDOWS.bat.'
}

function Wait-Url([string]$Name,[string]$Url) {
    foreach ($i in 1..120) {
        try {
            $r = Invoke-WebRequest -UseBasicParsing -Uri $Url -TimeoutSec 4
            if ($r.StatusCode -eq 200) { Write-Host "OK: $Name" -ForegroundColor Green; return }
        } catch {}
        Start-Sleep -Seconds 2
    }
    throw "$Name no quedo disponible. Revise .windows-runtime\logs."
}

if (Test-Path $pidsPath) {
    try {
        $old = Get-Content $pidsPath -Raw | ConvertFrom-Json
        foreach ($item in @($old)) { Stop-Process -Id $item.Pid -Force -ErrorAction SilentlyContinue }
    } catch {}
}

$sql = Find-LocalDb
& $sql start MSSQLLocalDB | Out-Null
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'

$items = @(
    @{Name='core'; Port='5085'; Dll='core-indotel\Indotel.Core\bin\Release\net8.0\Indotel.Core.dll'; Dir='core-indotel\Indotel.Core'; Health='http://localhost:5085/health/ready'},
    @{Name='pagos'; Port='5285'; Dll='proveedor-pagos-mock\Indotel.ProveedorPagos.Mock\bin\Release\net8.0\Indotel.ProveedorPagos.Mock.dll'; Dir='proveedor-pagos-mock\Indotel.ProveedorPagos.Mock'; Health='http://localhost:5285/health'},
    @{Name='gateway'; Port='5185'; Dll='api-gateway\Indotel.ApiGateway\bin\Release\net8.0\Indotel.ApiGateway.dll'; Dir='api-gateway\Indotel.ApiGateway'; Health='http://localhost:5185/health/ready'},
    @{Name='web'; Port='5234'; Dll='INDOTEL.Web\bin\Release\net8.0\INDOTEL.WEB.dll'; Dir='INDOTEL.Web'; Health='http://localhost:5234/'}
)

$processes = @()
foreach ($item in $items) {
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$($item.Port)"
    $dll = Join-Path $root $item.Dll
    $dir = Join-Path $root $item.Dir
    if (-not (Test-Path $dll)) { throw "Falta $dll. Ejecute 01_PREPARAR_TODO_EN_WINDOWS.bat." }
    $out = Join-Path $logs "$($item.Name).out.log"
    $err = Join-Path $logs "$($item.Name).err.log"
    $p = Start-Process dotnet.exe -ArgumentList @("`"$dll`"") -WorkingDirectory $dir -WindowStyle Hidden -RedirectStandardOutput $out -RedirectStandardError $err -PassThru
    $processes += [pscustomobject]@{Name=$item.Name;Pid=$p.Id}
    Wait-Url $item.Name $item.Health
}
$processes | ConvertTo-Json | Set-Content $pidsPath -Encoding UTF8

$env:INDOTEL_GATEWAY_URL='http://localhost:5185'
$env:INDOTEL_CORE_URL='http://localhost:5085'
if (-not $SinInterfaces) {
    Start-Process (Join-Path $root 'Indotel.Caja\bin\Release\net8.0-windows\Indotel.Caja.exe')
    Start-Process (Join-Path $root 'core-indotel\INDOTEL.CORE.UI\bin\Release\net8.0-windows\INDOTEL.CORE.UI.exe')
    Start-Process 'http://localhost:5234/'
    Start-Process 'http://localhost:5085/swagger'
}
Write-Host 'TODO INDOTEL ESTA FUNCIONANDO EN WINDOWS.' -ForegroundColor Green
