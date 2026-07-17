@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title INDOTEL Core Administrativo - VM Windows

if exist "vm-config.json" for /f "usebackq delims=" %%U in (`powershell -NoProfile -Command "(Get-Content '.\vm-config.json' -Raw ^| ConvertFrom-Json).CoreUrl"`) do set "INDOTEL_CORE_URL=%%U"
if not defined INDOTEL_CORE_URL (
  echo ERROR: No existe configuracion del Core.
  echo Ejecute PREPARAR_VM_WINDOWS_COMPLETA.bat.
  pause
  exit /b 1
)

echo Core API: %INDOTEL_CORE_URL%
powershell -NoProfile -ExecutionPolicy Bypass -Command "try{Invoke-WebRequest -UseBasicParsing '%INDOTEL_CORE_URL%/health/ready' -TimeoutSec 8 ^| Out-Null; exit 0}catch{exit 1}"
if errorlevel 1 (
  echo ERROR: Core API no responde.
  pause
  exit /b 1
)

set "CORE_UI_EXE=%~dp0core-indotel\INDOTEL.CORE.UI\bin\Release\net8.0-windows\INDOTEL.CORE.UI.exe"
if exist "%CORE_UI_EXE%" (
  start "INDOTEL Core Administrativo" "%CORE_UI_EXE%"
  exit /b 0
)

echo No se encontro la compilacion Release. Compilando y abriendo Core UI...
dotnet run --project "core-indotel\INDOTEL.CORE.UI\INDOTEL.CORE.UI.csproj" --configuration Release
