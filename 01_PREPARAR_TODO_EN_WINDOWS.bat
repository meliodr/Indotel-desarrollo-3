@echo off
setlocal
cd /d "%~dp0"
title INDOTEL - Preparar todo en Windows

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  echo Solicitando permisos de Administrador...
  powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0PREPARAR_TODO_EN_WINDOWS.ps1"
set "RC=%errorlevel%"
echo.
if "%RC%"=="0" (
  echo PREPARACION COMPLETADA.
  echo Ejecute 02_INICIAR_TODO_EN_WINDOWS.bat
) else if "%RC%"=="3010" (
  echo Reinicie Windows y vuelva a ejecutar este archivo.
) else (
  echo ERROR DURANTE LA PREPARACION. Codigo: %RC%
)
pause
exit /b %RC%
