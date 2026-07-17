[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$project = Join-Path $root 'Indotel.Caja\Indotel.Caja.csproj'

if (-not (Test-Path $project)) {
    throw 'No se encontro Indotel.Caja\Indotel.Caja.csproj en la raiz del repositorio.'
}

$fixes = @{
    'Indotel.Caja\Core\CredencialOfflineService.cs' = 'using System.IO;'
    'Indotel.Caja\Core\LogLocalService.cs'          = 'using System.IO;'
    'Indotel.Caja\Core\AlmacenLocalService.cs'     = 'using System.IO;'
    'Indotel.Caja\Core\ApiClient.cs'               = 'using System.IO;'
    'Indotel.Caja\Core\ColaOfflineService.cs'      = 'using System.Net.Http;'
}

foreach ($relativePath in $fixes.Keys) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path $path)) { throw "No se encontro $relativePath" }
    $requiredUsing = $fixes[$relativePath]
    $content = Get-Content $path -Raw
    if ($content -notmatch [regex]::Escape($requiredUsing)) {
        Set-Content -Path $path -Value ($requiredUsing + [Environment]::NewLine + $content) -Encoding UTF8
        Write-Host "Corregido: $relativePath" -ForegroundColor Green
    }
    else {
        Write-Host "Ya estaba correcto: $relativePath" -ForegroundColor DarkGreen
    }
}

Remove-Item -Recurse -Force (Join-Path $root 'Indotel.Caja\bin') -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force (Join-Path $root 'Indotel.Caja\obj') -ErrorAction SilentlyContinue

& dotnet restore $project
if ($LASTEXITCODE -ne 0) { throw 'Fallo dotnet restore de Caja WPF.' }

& dotnet build $project --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Fallo dotnet build de Caja WPF.' }

Write-Host 'Caja WPF compilada correctamente.' -ForegroundColor Green
