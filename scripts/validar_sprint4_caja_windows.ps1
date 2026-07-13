$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$cajaProject = Join-Path $root 'INDOTEL_CAJA(REAL)\INDOTEL_CAJA(REAL).csproj'
$testProject = Join-Path $root 'INDOTEL_CAJA.Tests\INDOTEL_CAJA.Tests.csproj'
$publishDir = Join-Path $root 'artifacts\caja'

Write-Host "`n==> SDK de .NET"
dotnet --info

Write-Host "`n==> Limpieza"
Get-ChildItem -Path (Join-Path $root 'INDOTEL_CAJA(REAL)'), (Join-Path $root 'INDOTEL_CAJA.Tests') -Directory -Recurse -Force |
    Where-Object { $_.Name -in @('bin', 'obj', 'TestResults') } |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`n==> Restauracion"
dotnet restore $cajaProject
dotnet restore $testProject

Write-Host "`n==> Compilacion Release de Caja"
dotnet build $cajaProject --configuration Release --no-restore

Write-Host "`n==> Pruebas automaticas"
dotnet test $testProject --configuration Release --no-restore --collect:"XPlat Code Coverage"

Write-Host "`n==> Publicacion win-x64"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
dotnet publish $cajaProject --configuration Release --runtime win-x64 --self-contained false --no-restore --output $publishDir

Write-Host "`nSprint 4 validado en Windows: restauracion, compilacion, pruebas y publicacion completadas."
Write-Host "Artefacto: $publishDir"
