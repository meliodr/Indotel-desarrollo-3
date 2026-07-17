@echo off
setlocal EnableExtensions
cd /d "%~dp0"
if exist "vm-config.json" for /f "usebackq delims=" %%U in (`powershell -NoProfile -Command "(Get-Content '.\vm-config.json' -Raw ^| ConvertFrom-Json).WebUrl"`) do set "INDOTEL_WEB_URL=%%U"
if not defined INDOTEL_WEB_URL (
  echo ERROR: Ejecute PREPARAR_VM_WINDOWS_COMPLETA.bat.
  pause
  exit /b 1
)
start "Portal Web INDOTEL" "%INDOTEL_WEB_URL%"
