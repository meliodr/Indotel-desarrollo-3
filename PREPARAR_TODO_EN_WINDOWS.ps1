[CmdletBinding()]
param([switch]$OmitirPruebas)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$root = $PSScriptRoot

function Write-Step([string]$Text) {
    Write-Host "`n==> $Text" -ForegroundColor Cyan
}

function Require-Administrator {
    $principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Ejecute 01_PREPARAR_TODO_EN_WINDOWS.bat y acepte el permiso de Administrador.'
    }
}

function Get-VsWhere {
    $path = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $path)) { throw 'No se encontro Visual Studio Installer.' }
    return $path
}

function Get-VisualStudioPath {
    $vswhere = Get-VsWhere
    $path = & $vswhere -latest -prerelease -products * -property installationPath
    if (-not $path) { throw 'No se encontro Visual Studio 2022/2026 compatible.' }
    return ($path | Select-Object -First 1)
}

function Ensure-DotNet8 {
    $sdks = @(& dotnet --list-sdks 2>$null)
    if ($sdks -match '^8\.0\.') {
        Write-Host 'SDK .NET 8 detectado.' -ForegroundColor Green
        return
    }
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) { throw 'No se encontro SDK .NET 8 ni WinGet para instalarlo.' }
    Write-Step 'Instalando SDK .NET 8'
    & winget install --id Microsoft.DotNet.SDK.8 --exact --source winget --accept-package-agreements --accept-source-agreements
    if ($LASTEXITCODE -ne 0) { throw "No se pudo instalar .NET 8. Codigo: $LASTEXITCODE" }
    $env:PATH = "${env:ProgramFiles}\dotnet;$env:PATH"
    $sdks = @(& dotnet --list-sdks)
    if (-not ($sdks -match '^8\.0\.')) { throw 'El SDK .NET 8 no aparece despues de instalarlo.' }
}

function Ensure-VisualStudioComponents {
    $vswhere = Get-VsWhere
    $vsPath = Get-VisualStudioPath
    $setup = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\setup.exe'
    if (-not (Test-Path $setup)) { throw 'No se encontro setup.exe de Visual Studio Installer.' }

    $components = @(
        'Microsoft.VisualStudio.Workload.ManagedDesktop',
        'Microsoft.VisualStudio.Workload.NetWeb',
        'Microsoft.VisualStudio.Component.SQL.LocalDB.Runtime'
    )

    $missing = @()
    foreach ($component in $components) {
        $found = & $vswhere -latest -prerelease -products * -requires $component -property installationPath
        if (-not $found) { $missing += $component }
    }

    if ($missing.Count -eq 0) {
        Write-Host 'Visual Studio, WPF, WinForms, ASP.NET y SQL LocalDB estan instalados.' -ForegroundColor Green
        return
    }

    Write-Step ('Agregando componentes a Visual Studio: ' + ($missing -join ', '))
    $argumentText = 'modify --installPath "' + $vsPath + '"'
    foreach ($component in $missing) { $argumentText += ' --add ' + $component }
    $argumentText += ' --includeRecommended --passive --norestart'
    $process = Start-Process -FilePath $setup -ArgumentList $argumentText -Wait -PassThru
    if ($process.ExitCode -notin @(0, 3010)) {
        throw "Visual Studio Installer termino con codigo $($process.ExitCode)."
    }
    if ($process.ExitCode -eq 3010) {
        Write-Host 'Windows requiere reinicio. Reinicie y ejecute otra vez 01_PREPARAR_TODO_EN_WINDOWS.bat.' -ForegroundColor Yellow
        exit 3010
    }
}

function Get-SqlLocalDb {
    $command = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    $candidates = Get-ChildItem "$env:ProgramFiles\Microsoft SQL Server" -Recurse -Filter SqlLocalDB.exe -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending
    if ($candidates) { return $candidates[0].FullName }
    throw 'SQL Server Express LocalDB no se encontro despues de preparar Visual Studio.'
}

function Ensure-LocalDb {
    $sqlLocalDb = Get-SqlLocalDb
    $info = & $sqlLocalDb info MSSQLLocalDB 2>&1
    if ($LASTEXITCODE -ne 0) {
        & $sqlLocalDb create MSSQLLocalDB | Out-Host
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear MSSQLLocalDB.' }
    }
    & $sqlLocalDb start MSSQLLocalDB | Out-Host
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) { throw 'No se pudo iniciar MSSQLLocalDB.' }
    Write-Host 'SQL Server LocalDB listo.' -ForegroundColor Green
}

function Invoke-DotNet([string[]]$Arguments) {
    Write-Host ('dotnet ' + ($Arguments -join ' ')) -ForegroundColor DarkGray
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Fallo: dotnet $($Arguments -join ' ')" }
}

Require-Administrator
Write-Step 'Comprobando Visual Studio 2022/2026 y componentes'
Ensure-VisualStudioComponents
Write-Step 'Comprobando SDK .NET 8'
Ensure-DotNet8
Write-Step 'Preparando SQL Server Express LocalDB'
Ensure-LocalDb

$runtime = Join-Path $root '.windows-runtime'
New-Item -ItemType Directory -Force -Path $runtime, (Join-Path $runtime 'logs'), (Join-Path $runtime 'gateway'), (Join-Path $runtime 'web-keys') | Out-Null

Write-Step 'Restaurando y compilando toda la solucion'
Invoke-DotNet @('restore', (Join-Path $root 'INDOTEL.sln'))
Invoke-DotNet @('build', (Join-Path $root 'INDOTEL.sln'), '--configuration', 'Release', '--no-restore')

if (-not $OmitirPruebas) {
    Write-Step 'Ejecutando pruebas Core, Gateway y Web'
    Invoke-DotNet @('test', (Join-Path $root 'core-indotel\Indotel.Core.Tests\Indotel.Core.Tests.csproj'), '--configuration', 'Release', '--no-build')
    Invoke-DotNet @('test', (Join-Path $root 'api-gateway\Indotel.ApiGateway.Tests\Indotel.ApiGateway.Tests.csproj'), '--configuration', 'Release', '--no-build')
    Invoke-DotNet @('test', (Join-Path $root 'INDOTEL.Web.Tests\INDOTEL.Web.Tests.csproj'), '--configuration', 'Release', '--no-build')
}

[Environment]::SetEnvironmentVariable('INDOTEL_GATEWAY_URL', 'http://localhost:5185', 'User')
[Environment]::SetEnvironmentVariable('INDOTEL_CORE_URL', 'http://localhost:5085', 'User')

$desktop = [Environment]::GetFolderPath('Desktop')
$shell = New-Object -ComObject WScript.Shell
$shortcuts = @{
    'INDOTEL - Iniciar todo.lnk' = '02_INICIAR_TODO_EN_WINDOWS.bat'
    'INDOTEL - Estado.lnk' = '03_ESTADO_TODO_EN_WINDOWS.bat'
    'INDOTEL - Detener todo.lnk' = '04_DETENER_TODO_EN_WINDOWS.bat'
}
foreach ($name in $shortcuts.Keys) {
    $shortcut = $shell.CreateShortcut((Join-Path $desktop $name))
    $shortcut.TargetPath = (Join-Path $root $shortcuts[$name])
    $shortcut.WorkingDirectory = $root
    $shortcut.Save()
}

@"
Preparacion completada: $(Get-Date -Format s)
Visual Studio: $(Get-VisualStudioPath)
.NET SDKs:
$((& dotnet --list-sdks) -join "`r`n")
SQL: (localdb)\MSSQLLocalDB
Core: http://localhost:5085
Gateway: http://localhost:5185
Web: http://localhost:5234
Pagos: http://localhost:5285
"@ | Set-Content (Join-Path $runtime 'PREPARACION_OK.txt') -Encoding UTF8

Write-Host "`nPREPARACION COMPLETADA." -ForegroundColor Green
Write-Host 'Ejecute 02_INICIAR_TODO_EN_WINDOWS.bat.' -ForegroundColor Green
