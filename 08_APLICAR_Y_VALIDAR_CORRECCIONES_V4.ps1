[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$root = $PSScriptRoot
$log = Join-Path $root 'VALIDACION_CORRECCIONES_V4.txt'

function Step([string]$text) {
    Write-Host "`n==> $text" -ForegroundColor Cyan
    Add-Content -Path $log -Value "`n==> $text"
}

function DotNet([string[]]$arguments) {
    $line = 'dotnet ' + ($arguments -join ' ')
    Write-Host $line -ForegroundColor DarkGray
    Add-Content -Path $log -Value $line
    & dotnet @arguments 2>&1 | Tee-Object -FilePath $log -Append
    if ($LASTEXITCODE -ne 0) { throw "Fallo: $line" }
}

"Validacion V4 iniciada: $(Get-Date -Format s)" | Set-Content $log -Encoding UTF8

Step 'Deteniendo procesos para evitar ventanas o ejecutables duplicados'
& (Join-Path $root 'DETENER_TODO_EN_WINDOWS.ps1') | Tee-Object -FilePath $log -Append
Get-Process -Name 'Indotel.Caja','INDOTEL.CORE.UI' -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Step 'Validando archivo externo de datos de demostracion'
$seedPath = Join-Path $root 'core-indotel\Indotel.Core\Data\Seed\seed-development.json'
$seed = Get-Content $seedPath -Raw | ConvertFrom-Json
$currencyCodes = @($seed.monedas | ForEach-Object { $_.codigo })
if ('DOP' -notin $currencyCodes -or 'USD' -notin $currencyCodes) {
    throw 'El seed externo debe incluir DOP y USD.'
}
foreach ($section in 'productos','serviciosCobrables','tasasCambio','exenciones','reclamaciones','facturas') {
    if (@($seed.$section).Count -eq 0) { throw "La seccion $section no contiene datos de prueba." }
}
Write-Host 'Seed externo: OK (DOP, USD y datos funcionales).' -ForegroundColor Green

Step 'Validando eliminacion de flujos y vistas duplicadas'
$obsolete = @(
    (Join-Path $root 'INDOTEL.Web\Views\Comercial\Login.cshtml'),
    (Join-Path $root 'INDOTEL.Web\Views\Reclamacion\Crear.cshtml')
)
foreach ($path in $obsolete) {
    if (Test-Path $path) { throw "Todavia existe la vista duplicada $path" }
}
$webController = Get-Content (Join-Path $root 'INDOTEL.Web\Controllers\ComercialController.cs') -Raw
if ($webController -notmatch 'View\("DetalleOrden", model\)') {
    throw 'EstadoOrden no esta enlazado con la vista DetalleOrden.'
}
$cajaApp = Get-Content (Join-Path $root 'Indotel.Caja\App.xaml.cs') -Raw
$coreProgram = Get-Content (Join-Path $root 'core-indotel\INDOTEL.CORE.UI\Program.cs') -Raw
if ($cajaApp -notmatch 'INDOTEL\.Caja\.SingleInstance' -or $coreProgram -notmatch 'INDOTEL\.CoreUI\.SingleInstance') {
    throw 'Falta la proteccion de instancia unica en una interfaz de escritorio.'
}
Write-Host 'Flujos canonicos e instancia unica: OK.' -ForegroundColor Green

Step 'Restaurando y compilando solucion completa'
DotNet @('restore', (Join-Path $root 'INDOTEL.sln'))
DotNet @('build', (Join-Path $root 'INDOTEL.sln'), '--configuration', 'Release', '--no-restore')

Step 'Ejecutando pruebas Core, Gateway y Web'
DotNet @('test', (Join-Path $root 'core-indotel\Indotel.Core.Tests\Indotel.Core.Tests.csproj'), '--configuration', 'Release', '--no-build')
DotNet @('test', (Join-Path $root 'api-gateway\Indotel.ApiGateway.Tests\Indotel.ApiGateway.Tests.csproj'), '--configuration', 'Release', '--no-build')
DotNet @('test', (Join-Path $root 'INDOTEL.Web.Tests\INDOTEL.Web.Tests.csproj'), '--configuration', 'Release', '--no-build')

Step 'Iniciando servicios e interfaces una sola vez'
& (Join-Path $root 'INICIAR_TODO_EN_WINDOWS.ps1') | Tee-Object -FilePath $log -Append
Start-Sleep -Seconds 2
foreach ($processName in 'Indotel.Caja','INDOTEL.CORE.UI') {
    $count = @(Get-Process -Name $processName -ErrorAction SilentlyContinue).Count
    if ($count -gt 1) { throw "Se detectaron $count instancias de $processName." }
    Write-Host ("Instancias {0}: {1}" -f $processName, $count) -ForegroundColor Green
}

Step 'Comprobando endpoints'
$urls = @(
    'http://localhost:5085/health/ready',
    'http://localhost:5185/health/ready',
    'http://localhost:5234/',
    'http://localhost:5285/health'
)
foreach ($url in $urls) {
    $response = Invoke-WebRequest -UseBasicParsing -Uri $url -TimeoutSec 10
    if ($response.StatusCode -ne 200) { throw "Fallo de salud en $url" }
    Write-Host "OK $url" -ForegroundColor Green
}

"`nVALIDACION V4 COMPLETADA: $(Get-Date -Format s)" | Add-Content $log
Write-Host "`nCORRECCIONES V4 COMPILADAS, PROBADAS E INICIADAS." -ForegroundColor Green
Write-Host "Evidencia: $log" -ForegroundColor Green
