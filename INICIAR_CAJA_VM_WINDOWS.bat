@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title Caja INDOTEL - VM Windows

if exist "vm-config.json" for /f "usebackq delims=" %%U in (`powershell -NoProfile -Command "(Get-Content '.\vm-config.json' -Raw ^| ConvertFrom-Json).GatewayUrl"`) do set "INDOTEL_GATEWAY_URL=%%U"
if not defined INDOTEL_GATEWAY_URL (
  echo ERROR: No existe configuracion del Gateway.
  echo Ejecute PREPARAR_VM_WINDOWS_COMPLETA.bat.
  pause
  exit /b 1
)

echo Gateway: %INDOTEL_GATEWAY_URL%
powershell -NoProfile -ExecutionPolicy Bypass -Command "try{Invoke-WebRequest -UseBasicParsing '%INDOTEL_GATEWAY_URL%/health/ready' -TimeoutSec 8 ^| Out-Null; exit 0}catch{exit 1}"
if errorlevel 1 (
  echo ADVERTENCIA: Gateway no responde. La Caja intentara operar en contingencia
  echo solamente si existen credenciales y jornada almacenadas previamente.
)

set "CAJA_EXE=%~dp0Indotel.Caja\bin\Release\net8.0-windows\Indotel.Caja.exe"
if exist "%CAJA_EXE%" (
  start "INDOTEL Caja" "%CAJA_EXE%"
  exit /b 0
)

echo No se encontro la compilacion Release. Compilando y abriendo Caja...
dotnet run --project "Indotel.Caja\Indotel.Caja.csproj" --configuration Release
