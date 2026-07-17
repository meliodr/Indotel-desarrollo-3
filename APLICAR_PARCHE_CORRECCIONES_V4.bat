@echo off
setlocal
cd /d "%~dp0"
title INDOTEL - Aplicar correcciones funcionales V4
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0APLICAR_PARCHE_CORRECCIONES_V4.ps1"
set "CODE=%ERRORLEVEL%"
if not "%CODE%"=="0" (
  echo.
  echo ERROR AL APLICAR LAS CORRECCIONES V4. Codigo: %CODE%
  pause
  exit /b %CODE%
)
echo.
echo CORRECCIONES V4 APLICADAS Y VALIDADAS.
pause
