[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

function Write-Step([string]$Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Get-VsWherePath {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'),
        (Join-Path $env:ProgramFiles 'Microsoft Visual Studio\Installer\vswhere.exe')
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    return $null
}

function Get-VisualStudioInstallationPath {
    $vswhere = Get-VsWherePath
    if (-not $vswhere) {
        return $null
    }

    $path = & $vswhere -latest -products * -version '[17.0,18.0)' -property installationPath 2>$null
    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    if ($path) {
        return ($path | Select-Object -First 1).Trim()
    }

    return $null
}

function Get-VisualStudioInstallerPath {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\setup.exe'),
        (Join-Path $env:ProgramFiles 'Microsoft Visual Studio\Installer\setup.exe')
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    return $null
}

function Find-VisualStudioBootstrapper {
    $searchRoots = @(
        (Join-Path $env:USERPROFILE 'Downloads'),
        (Join-Path $env:USERPROFILE 'Descargas'),
        $PSScriptRoot,
        (Split-Path $PSScriptRoot -Parent)
    ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

    $names = @(
        'vs_Community.exe',
        'VisualStudioSetup.exe',
        'vs_Professional.exe',
        'vs_Enterprise.exe'
    )

    foreach ($root in $searchRoots) {
        foreach ($name in $names) {
            $item = Get-ChildItem -Path $root -Filter $name -File -Recurse -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1
            if ($item) {
                return $item.FullName
            }
        }
    }

    return $null
}

function Invoke-VisualStudioBootstrapper([string]$Bootstrapper) {
    Write-Step "Instalando Visual Studio 2022 Community y las cargas requeridas"
    Write-Host "Instalador: $Bootstrapper"
    Write-Host 'Esta operacion puede tardar bastante. No cierre esta ventana.' -ForegroundColor Yellow

    $arguments = @(
        '--passive',
        '--wait',
        '--norestart',
        '--add', 'Microsoft.VisualStudio.Workload.ManagedDesktop',
        '--add', 'Microsoft.VisualStudio.Workload.NetWeb',
        '--includeRecommended'
    )

    $process = Start-Process -FilePath $Bootstrapper -ArgumentList $arguments -Wait -PassThru
    if ($process.ExitCode -notin @(0, 3010)) {
        throw "El instalador de Visual Studio termino con codigo $($process.ExitCode)."
    }

    if ($process.ExitCode -eq 3010) {
        Write-Host 'Visual Studio solicito reiniciar Windows.' -ForegroundColor Yellow
    }
}

function Install-VisualStudioWithWinget {
    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if (-not $winget) {
        return $false
    }

    Write-Step "Instalando Visual Studio 2022 Community mediante WinGet"
    Write-Host 'Esta operacion puede tardar bastante. No cierre esta ventana.' -ForegroundColor Yellow

    $override = '--passive --wait --norestart --add Microsoft.VisualStudio.Workload.ManagedDesktop --add Microsoft.VisualStudio.Workload.NetWeb --includeRecommended'
    & winget.exe install `
        --id Microsoft.VisualStudio.2022.Community `
        --exact `
        --source winget `
        --accept-source-agreements `
        --accept-package-agreements `
        --override $override

    if ($LASTEXITCODE -notin @(0, 3010)) {
        throw "WinGet termino con codigo $LASTEXITCODE al instalar Visual Studio."
    }

    return $true
}

function Download-VisualStudioBootstrapper {
    $destination = Join-Path $env:TEMP 'vs_Community.exe'
    Write-Step "Descargando el instalador oficial de Visual Studio 2022 Community"
    Invoke-WebRequest -Uri 'https://aka.ms/vs/17/release/vs_Community.exe' -OutFile $destination -UseBasicParsing

    if (-not (Test-Path $destination)) {
        throw 'No se pudo descargar el instalador de Visual Studio.'
    }

    return $destination
}

function Ensure-VisualStudioInstalled {
    $installationPath = Get-VisualStudioInstallationPath
    if ($installationPath) {
        Write-Host "Visual Studio detectado en: $installationPath" -ForegroundColor Green
        return $installationPath
    }

    Write-Host 'Visual Studio 2022 aun no esta instalado. El archivo descargado era solo el instalador.' -ForegroundColor Yellow

    $bootstrapper = Find-VisualStudioBootstrapper
    if ($bootstrapper) {
        Invoke-VisualStudioBootstrapper -Bootstrapper $bootstrapper
    }
    elseif (-not (Install-VisualStudioWithWinget)) {
        $bootstrapper = Download-VisualStudioBootstrapper
        Invoke-VisualStudioBootstrapper -Bootstrapper $bootstrapper
    }

    Start-Sleep -Seconds 10
    $installationPath = Get-VisualStudioInstallationPath
    if (-not $installationPath) {
        throw 'La instalacion de Visual Studio no termino correctamente. Revise Visual Studio Installer y vuelva a ejecutar PREPARAR_VM_WINDOWS_COMPLETA.bat.'
    }

    Write-Host "Visual Studio instalado en: $installationPath" -ForegroundColor Green
    return $installationPath
}

function Ensure-RequiredWorkloads([string]$InstallationPath) {
    $vswhere = Get-VsWherePath
    $installer = Get-VisualStudioInstallerPath

    if (-not $vswhere -or -not $installer) {
        throw 'Visual Studio Installer o vswhere no estan disponibles despues de la instalacion.'
    }

    $readyPath = & $vswhere -latest -products * -version '[17.0,18.0)' `
        -requires Microsoft.VisualStudio.Workload.ManagedDesktop `
        -requires Microsoft.VisualStudio.Workload.NetWeb `
        -property installationPath 2>$null

    if ($readyPath) {
        Write-Host 'Las cargas .NET Desktop y ASP.NET ya estan instaladas.' -ForegroundColor Green
        return
    }

    Write-Step "Agregando cargas .NET Desktop y ASP.NET a Visual Studio"
    $arguments = @(
        'modify',
        '--installPath', $InstallationPath,
        '--add', 'Microsoft.VisualStudio.Workload.ManagedDesktop',
        '--add', 'Microsoft.VisualStudio.Workload.NetWeb',
        '--includeRecommended',
        '--passive',
        '--norestart'
    )

    $process = Start-Process -FilePath $installer -ArgumentList $arguments -Wait -PassThru
    if ($process.ExitCode -notin @(0, 3010)) {
        throw "Visual Studio Installer termino con codigo $($process.ExitCode)."
    }
}

function Ensure-DotNet8 {
    $dotnet = Get-Command dotnet.exe -ErrorAction SilentlyContinue
    if ($dotnet) {
        $sdks = & dotnet.exe --list-sdks 2>$null
        if ($sdks -match '^8\.0\.') {
            Write-Host 'SDK .NET 8 detectado.' -ForegroundColor Green
            return
        }
    }

    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if (-not $winget) {
        throw 'No se encontro el SDK .NET 8 y WinGet no esta disponible para instalarlo automaticamente.'
    }

    Write-Step "Instalando SDK .NET 8"
    & winget.exe install `
        --id Microsoft.DotNet.SDK.8 `
        --exact `
        --source winget `
        --accept-source-agreements `
        --accept-package-agreements `
        --silent

    if ($LASTEXITCODE -notin @(0, 3010)) {
        throw "No se pudo instalar .NET 8. WinGet termino con codigo $LASTEXITCODE."
    }

    $machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    $env:Path = "$machinePath;$userPath"

    $sdks = & dotnet.exe --list-sdks 2>$null
    if (-not ($sdks -match '^8\.0\.')) {
        throw 'El SDK .NET 8 no aparece despues de la instalacion. Reinicie Windows y vuelva a ejecutar el preparador.'
    }
}

if (-not (Test-Administrator)) {
    throw 'Ejecute este script como Administrador.'
}

Write-Step "Comprobando Visual Studio 2022"
$installationPath = Ensure-VisualStudioInstalled
Ensure-RequiredWorkloads -InstallationPath $installationPath
Ensure-DotNet8

Write-Host ''
Write-Host 'Visual Studio 2022, WPF, WinForms, ASP.NET y .NET 8 estan preparados.' -ForegroundColor Green
