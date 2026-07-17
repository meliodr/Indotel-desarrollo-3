[CmdletBinding()]
param(
    [string]$UbuntuIp,
    [switch]$OmitirCompilacion
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Require-DotNet8 {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) { throw 'No se encontró dotnet. Instale el SDK de .NET 8.' }
    $sdks = & dotnet --list-sdks
    if (-not ($sdks -match '^8\.0\.')) { throw 'No se encontró un SDK .NET 8.0.x.' }
}

function Test-TcpPort {
    param(
        [Parameter(Mandatory)][string]$HostName,
        [Parameter(Mandatory)][int]$Port,
        [int]$TimeoutMilliseconds = 350
    )

    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $async = $client.BeginConnect($HostName, $Port, $null, $null)
        if (-not $async.AsyncWaitHandle.WaitOne($TimeoutMilliseconds, $false)) {
            return $false
        }
        $client.EndConnect($async)
        return $client.Connected
    }
    catch {
        return $false
    }
    finally {
        $client.Close()
    }
}

function Test-IndotelHost {
    param([string]$Ip)
    if (-not $Ip) { return $false }
    return (Test-TcpPort -HostName $Ip -Port 5185 -TimeoutMilliseconds 500) -and
           (Test-TcpPort -HostName $Ip -Port 5234 -TimeoutMilliseconds 500)
}

function Find-IndotelHost {
    param([string[]]$PreferredIps)

    foreach ($ip in $PreferredIps | Where-Object { $_ } | Select-Object -Unique) {
        Write-Host "Probando host Ubuntu: $ip"
        if (Test-IndotelHost -Ip $ip) { return $ip }
    }

    $localIps = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object {
            $_.IPAddress -notmatch '^(127\.|169\.254\.)' -and
            $_.IPAddress -match '^(10\.|192\.168\.|172\.(1[6-9]|2[0-9]|3[01])\.)'
        } |
        Select-Object -ExpandProperty IPAddress -Unique

    foreach ($localIp in $localIps) {
        $parts = $localIp.Split('.')
        if ($parts.Count -ne 4) { continue }
        $prefix = "$($parts[0]).$($parts[1]).$($parts[2])"
        Write-Host "Buscando Ubuntu automáticamente en ${prefix}.0/24..." -ForegroundColor Cyan

        $likely = @(124, 1, 2, 10, 20, 50, 100, 101, 102, 110, 120, 121, 122, 123, 125, 126, 130, 150, 200, 254)
        $remaining = 1..254 | Where-Object { $_ -notin $likely }
        foreach ($last in @($likely + $remaining)) {
            $candidate = "$prefix.$last"
            if ($candidate -eq $localIp) { continue }
            if (Test-IndotelHost -Ip $candidate) { return $candidate }
        }
    }

    return $null
}

function Get-Configuration {
    param([string]$ExplicitIp)

    $fileConfig = $null
    if (Test-Path '.\vm-config.json') {
        $fileConfig = Get-Content '.\vm-config.json' -Raw | ConvertFrom-Json
    }

    $preferred = @(
        $ExplicitIp,
        $(if ($fileConfig) { [string]$fileConfig.UbuntuIp } else { $null }),
        '192.168.1.124'
    )

    $resolvedIp = Find-IndotelHost -PreferredIps $preferred
    if (-not $resolvedIp) {
        throw 'No se encontró automáticamente el host Ubuntu con Gateway 5185 y Web 5234. Confirme que Docker esté encendido y que VirtualBox use Adaptador puente.'
    }

    return [pscustomobject]@{
        UbuntuIp = $resolvedIp
        CoreUrl = "http://${resolvedIp}:5085"
        GatewayUrl = "http://${resolvedIp}:5185"
        WebUrl = "http://${resolvedIp}:5234"
        PaymentsUrl = "http://${resolvedIp}:5285"
    }
}

function Test-HttpEndpoint {
    param([string]$Name, [string]$Url)
    Write-Host "Probando ${Name}: $Url"
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -TimeoutSec 15
        if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 400) {
            throw "HTTP $($response.StatusCode)"
        }
        Write-Host "OK: $Name" -ForegroundColor Green
    }
    catch {
        throw "No se pudo acceder a $Name ($Url). $($_.Exception.Message)"
    }
}

try {
    Require-DotNet8
    $config = Get-Configuration -ExplicitIp $UbuntuIp

    $gatewayUrl = ([string]$config.GatewayUrl).TrimEnd('/')
    $coreUrl = ([string]$config.CoreUrl).TrimEnd('/')
    $webUrl = ([string]$config.WebUrl).TrimEnd('/')

    $config | ConvertTo-Json -Depth 10 | Set-Content '.\vm-config.json' -Encoding UTF8

    Write-Host '==> Guardando configuración de usuario' -ForegroundColor Cyan
    [Environment]::SetEnvironmentVariable('INDOTEL_GATEWAY_URL', $gatewayUrl, 'User')
    [Environment]::SetEnvironmentVariable('INDOTEL_CORE_URL', $coreUrl, 'User')
    [Environment]::SetEnvironmentVariable('INDOTEL_WEB_URL', $webUrl, 'User')
    $env:INDOTEL_GATEWAY_URL = $gatewayUrl
    $env:INDOTEL_CORE_URL = $coreUrl
    $env:INDOTEL_WEB_URL = $webUrl

    Write-Host '==> Actualizando configuraciones locales de respaldo' -ForegroundColor Cyan
    $cajaPath = '.\Indotel.Caja\appsettings.json'
    $caja = Get-Content $cajaPath -Raw | ConvertFrom-Json
    $caja.Api.BaseUrl = $gatewayUrl
    $caja | ConvertTo-Json -Depth 10 | Set-Content $cajaPath -Encoding UTF8

    $coreUiPath = '.\core-indotel\INDOTEL.CORE.UI\appsettings.json'
    $coreUi = Get-Content $coreUiPath -Raw | ConvertFrom-Json
    $coreUi.CoreApi.BaseUrl = "$coreUrl/"
    $coreUi | ConvertTo-Json -Depth 10 | Set-Content $coreUiPath -Encoding UTF8

    Test-HttpEndpoint -Name 'Core API' -Url "$coreUrl/health/ready"
    Test-HttpEndpoint -Name 'API Gateway' -Url "$gatewayUrl/health/ready"
    Test-HttpEndpoint -Name 'Portal Web' -Url "$webUrl/"

    if (-not $OmitirCompilacion) {
        Write-Host '==> Restaurando Caja WPF' -ForegroundColor Cyan
        dotnet restore '.\Indotel.Caja\Indotel.Caja.csproj'
        if ($LASTEXITCODE -ne 0) { throw 'Falló restore de Caja.' }

        Write-Host '==> Compilando Caja WPF' -ForegroundColor Cyan
        dotnet build '.\Indotel.Caja\Indotel.Caja.csproj' -c Release --no-restore -warnaserror
        if ($LASTEXITCODE -ne 0) { throw 'Falló compilación de Caja.' }

        Write-Host '==> Restaurando Core WinForms' -ForegroundColor Cyan
        dotnet restore '.\core-indotel\INDOTEL.CORE.UI\INDOTEL.CORE.UI.csproj'
        if ($LASTEXITCODE -ne 0) { throw 'Falló restore de Core UI.' }

        Write-Host '==> Compilando Core WinForms' -ForegroundColor Cyan
        dotnet build '.\core-indotel\INDOTEL.CORE.UI\INDOTEL.CORE.UI.csproj' -c Release --no-restore -warnaserror
        if ($LASTEXITCODE -ne 0) { throw 'Falló compilación de Core UI.' }
    }

    Write-Host '==> Creando accesos directos' -ForegroundColor Cyan
    $desktop = [Environment]::GetFolderPath('Desktop')
    $shell = New-Object -ComObject WScript.Shell
    $links = @(
        @{ Name='INDOTEL Caja.lnk'; Target=(Join-Path $PSScriptRoot 'INICIAR_CAJA_VM_WINDOWS.bat') },
        @{ Name='INDOTEL Core Administrativo.lnk'; Target=(Join-Path $PSScriptRoot 'INICIAR_GUI_CORE_VM_WINDOWS.bat') },
        @{ Name='INDOTEL Portal Web.lnk'; Target=(Join-Path $PSScriptRoot 'ABRIR_WEB_VM_WINDOWS.bat') }
    )
    foreach ($item in $links) {
        $shortcut = $shell.CreateShortcut((Join-Path $desktop $item.Name))
        $shortcut.TargetPath = $item.Target
        $shortcut.WorkingDirectory = $PSScriptRoot
        $shortcut.Save()
    }

    @"
Configuración aplicada: $(Get-Date -Format s)
Carpeta: $PSScriptRoot
Ubuntu: $($config.UbuntuIp)
Gateway: $gatewayUrl
Core: $coreUrl
Web: $webUrl
Caja: compilada en Release
Core UI: compilado en Release
"@ | Set-Content '.\CONFIGURACION_VM_APLICADA.txt' -Encoding UTF8

    Remove-Item '.\CONFIGURACION_VM_DIAGNOSTICO.txt' -ErrorAction SilentlyContinue

    Write-Host ''
    Write-Host 'VM WINDOWS CONFIGURADA CORRECTAMENTE' -ForegroundColor Green
    Write-Host "Gateway: $gatewayUrl"
    Write-Host "Core:    $coreUrl"
    Write-Host "Web:     $webUrl"
    Write-Host 'Use INICIAR_APLICACIONES_VM_WINDOWS.bat para abrir las aplicaciones.'
}
catch {
    @"
Fecha: $(Get-Date -Format s)
Carpeta: $PSScriptRoot
Error: $($_.Exception.Message)

Compruebe:
- Docker activo en Ubuntu.
- Core 5085, Gateway 5185 y Web 5234 publicados.
- Adaptador puente en VirtualBox.
- SDK .NET 8 instalado en Windows.
"@ | Set-Content '.\CONFIGURACION_VM_DIAGNOSTICO.txt' -Encoding UTF8
    throw
}
