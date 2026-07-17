@echo off
setlocal
cd /d "%~dp0"
title INDOTEL - Validar correcciones funcionales V4
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp008_APLICAR_Y_VALIDAR_CORRECCIONES_V4.ps1"
set "CODE=%ERRORLEVEL%"
if not "%CODE%"=="0" (
  echo.
  echo ERROR DURANTE LA VALIDACION V4. Codigo: %CODE%
  pause
  exit /b %CODE%
)
echo.
echo VALIDACION V4 COMPLETADA.
pause
