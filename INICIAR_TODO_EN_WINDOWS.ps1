[CmdletBinding()]
param([switch]$SinInterfaces)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$root = $PSScriptRoot
$runtime = Join-Path $root '.windows-runtime'
$logs = Join-Path $runtime 'logs'
$pidsPath = Join-Path $runtime 'procesos.json'
New-Item -ItemType Directory -Force -Path $runtime, $logs, (Join-Path $runtime 'gateway'), (Join-Path $runtime 'web-keys') | Out-Null

function Get-SqlLocalDb {
    $command = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    $candidate = Get-ChildItem "$env:ProgramFiles\Microsoft SQL Server" -Recurse -Filter SqlLocalDB.exe -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
    if ($candidate) { return $candidate.FullName }
    throw 'SQL LocalDB no esta instalado. Ejecute 01_PREPARAR_TODO_EN_WINDOWS.bat.'
}

function Stop-RecordedProcesses {
    if (-not (Test-Path $pidsPath)) { return }
    try {
        $items = Get-Content $pidsPath -Raw | ConvertFrom-Json
        foreach ($item in @($items)) {
            $process = Get-Process -Id $item.Pid -ErrorAction SilentlyContinue
            if ($process) { Stop-Process -Id $item.Pid -Force -ErrorAction SilentlyContinue }
        }
    } catch {}
    Remove-Item $pidsPath -Force -ErrorAction SilentlyContinue
}

function Set-ProcessEnvironment([hashtable]$Values) {
    foreach ($key in $Values.Keys) {
        [Environment]::SetEnvironmentVariable($key, [string]$Values[$key], 'Process')
    }
}

function Start-DotnetService([string]$Name, [string]$Dll, [string]$WorkingDirectory, [hashtable]$Environment) {
    if (-not (Test-Path $Dll)) { throw "No existe $Dll. Ejecute 01_PREPARAR_TODO_EN_WINDOWS.bat." }
    Set-ProcessEnvironment $Environment
    $out = Join-Path $logs "$Name.out.log"
    $err = Join-Path $logs "$Name.err.log"
    Remove-Item $out, $err -Force -ErrorAction SilentlyContinue
    $process = Start-Process -FilePath 'dotnet.exe' -ArgumentList @("`"$Dll`"") -WorkingDirectory $WorkingDirectory -WindowStyle Hidden -RedirectStandardOutput $out -RedirectStandardError $err -PassThru
    return [pscustomobject]@{ Name=$Name; Pid=$process.Id; Path=$Dll }
}

function Wait-Url([string]$Name, [string]$Url, [int]$Attempts=90) {
    Write-Host ("Esperando {0}: {1}" -f $Name, $Url)
    foreach ($i in 1..$Attempts) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -TimeoutSec 4
            if ($response.StatusCode -eq 200) { Write-Host "OK: $Name" -ForegroundColor Green; return }
        } catch {}
        Start-Sleep -Seconds 2
    }
    Write-Host "ERROR: $Name no respondio. Revise $logs" -ForegroundColor Red
    throw "$Name no quedo disponible."
}

Stop-RecordedProcesses
$sqlLocalDb = Get-SqlLocalDb
& $sqlLocalDb start MSSQLLocalDB | Out-Null

$common = @{
    'ASPNETCORE_ENVIRONMENT'='Development'
    'DOTNET_ENVIRONMENT'='Development'
}

$processes = @()
$coreEnv = $common.Clone()
$coreEnv['ASPNETCORE_URLS']='http://127.0.0.1:5085'
$coreEnv['ConnectionStrings__DefaultConnection']='Server=(localdb)\MSSQLLocalDB;Database=IndotelCoreDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;'
$coreEnv['SeedData__Enabled']='true'
$coreEnv['Database__ApplyMigrationsOnStartup']='true'
$coreEnv['Jwt__Key']='INDOTEL_WINDOWS_LOCAL_2026_CLAVE_DEMO_SEGURA_64_CARACTERES_ABC123'
$coreEnv['Integration__SharedKey']='INDOTEL_INTEGRATION_DEMO_2026_CAMBIAR'
$processes += Start-DotnetService 'core' (Join-Path $root 'core-indotel\Indotel.Core\bin\Release\net8.0\Indotel.Core.dll') (Join-Path $root 'core-indotel\Indotel.Core') $coreEnv
Wait-Url 'Core API' 'http://localhost:5085/health/ready' 120

$paymentsEnv = $common.Clone()
$paymentsEnv['ASPNETCORE_URLS']='http://127.0.0.1:5285'
$processes += Start-DotnetService 'pagos' (Join-Path $root 'proveedor-pagos-mock\Indotel.ProveedorPagos.Mock\bin\Release\net8.0\Indotel.ProveedorPagos.Mock.dll') (Join-Path $root 'proveedor-pagos-mock\Indotel.ProveedorPagos.Mock') $paymentsEnv
Wait-Url 'Proveedor de pagos' 'http://localhost:5285/health'

$gatewayEnv = $common.Clone()
$gatewayEnv['ASPNETCORE_URLS']='http://127.0.0.1:5185'
$gatewayEnv['Gateway__CoreBaseUrl']='http://localhost:5085/'
$gatewayEnv['Gateway__ExternalPaymentsBaseUrl']='http://localhost:5285/'
$gatewayEnv['GatewayStore__DatabasePath']=(Join-Path $runtime 'gateway\indotel-gateway.db')
$gatewayEnv['GatewayStore__CoreIntegrationKey']='INDOTEL_INTEGRATION_DEMO_2026_CAMBIAR'
$processes += Start-DotnetService 'gateway' (Join-Path $root 'api-gateway\Indotel.ApiGateway\bin\Release\net8.0\Indotel.ApiGateway.dll') (Join-Path $root 'api-gateway\Indotel.ApiGateway') $gatewayEnv
Wait-Url 'API Gateway' 'http://localhost:5185/health/ready' 120

$webEnv = $common.Clone()
$webEnv['ASPNETCORE_URLS']='http://127.0.0.1:5234'
$webEnv['ApiSettings__GatewayBaseUrl']='http://localhost:5185'
$webEnv['Security__DataProtectionKeysPath']=(Join-Path $runtime 'web-keys')
$processes += Start-DotnetService 'web' (Join-Path $root 'INDOTEL.Web\bin\Release\net8.0\INDOTEL.WEB.dll') (Join-Path $root 'INDOTEL.Web') $webEnv
Wait-Url 'Portal Web' 'http://localhost:5234/' 120

$processes | ConvertTo-Json | Set-Content $pidsPath -Encoding UTF8
[Environment]::SetEnvironmentVariable('INDOTEL_GATEWAY_URL','http://localhost:5185','Process')
[Environment]::SetEnvironmentVariable('INDOTEL_CORE_URL','http://localhost:5085','Process')

if (-not $SinInterfaces) {
    $caja = Join-Path $root 'Indotel.Caja\bin\Release\net8.0-windows\Indotel.Caja.exe'
    $coreUi = Join-Path $root 'core-indotel\INDOTEL.CORE.UI\bin\Release\net8.0-windows\INDOTEL.CORE.UI.exe'
    if (-not (Test-Path $caja)) { throw 'No se encontro Caja compilada.' }
    if (-not (Test-Path $coreUi)) { throw 'No se encontro Core UI compilado.' }
    Start-Process -FilePath $caja -WorkingDirectory (Split-Path $caja)
    Start-Process -FilePath $coreUi -WorkingDirectory (Split-Path $coreUi)
    Start-Process 'http://localhost:5234/'
    Start-Process 'http://localhost:5085/swagger'
}

Write-Host "`nTODO INDOTEL ESTA FUNCIONANDO EN WINDOWS." -ForegroundColor Green
Write-Host 'Core:    http://localhost:5085/swagger'
Write-Host 'Gateway: http://localhost:5185/health/ready'
Write-Host 'Web:     http://localhost:5234/'
Write-Host 'Pagos:   http://localhost:5285/health'
Write-Host 'SQL:     (localdb)\MSSQLLocalDB'
Write-Host 'Admin:   admin@indotel.test / Admin123*'
Write-Host 'Cajero:  cajero@indotel.test / Caja123*'
