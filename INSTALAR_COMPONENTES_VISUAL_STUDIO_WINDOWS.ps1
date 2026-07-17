[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Ejecute este script como Administrador.'
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$installer = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\setup.exe'

if (-not (Test-Path $vswhere) -or -not (Test-Path $installer)) {
    throw 'Visual Studio Installer no está disponible. Termine primero la instalación base de Visual Studio 2022.'
}

$installationPath = & $vswhere -latest -products * -property installationPath
if (-not $installationPath) {
    throw 'No se encontró una instalación de Visual Studio 2022.'
}

$readyPath = & $vswhere -latest -products * `
    -requires Microsoft.VisualStudio.Workload.ManagedDesktop `
    -requires Microsoft.VisualStudio.Workload.NetWeb `
    -property installationPath

if ($readyPath) {
    Write-Host 'Las cargas de trabajo de escritorio .NET y ASP.NET ya están instaladas.' -ForegroundColor Green
}
else {
    Write-Host '==> Instalando cargas de trabajo requeridas' -ForegroundColor Cyan
    & $installer modify `
        --installPath $installationPath `
        --add Microsoft.VisualStudio.Workload.ManagedDesktop `
        --add Microsoft.VisualStudio.Workload.NetWeb `
        --includeRecommended `
        --passive `
        --norestart

    if ($LASTEXITCODE -notin 0,3010) {
        throw "Visual Studio Installer terminó con código $LASTEXITCODE."
    }

    if ($LASTEXITCODE -eq 3010) {
        Write-Host 'Visual Studio solicitó reiniciar Windows. Reinicie y vuelva a ejecutar PREPARAR_VM_WINDOWS_COMPLETA.bat.' -ForegroundColor Yellow
    }
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    throw 'No se encontró dotnet. En Visual Studio Installer confirme el SDK de .NET 8.'
}

$sdks = & dotnet --list-sdks
if (-not ($sdks -match '^8\.0\.')) {
    throw 'No se encontró un SDK .NET 8.0.x. Agréguelo desde Visual Studio Installer.'
}

Write-Host 'Visual Studio y .NET 8 están preparados.' -ForegroundColor Green
