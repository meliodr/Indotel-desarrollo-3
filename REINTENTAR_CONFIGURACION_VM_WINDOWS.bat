@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title Reintentar configuracion INDOTEL VM

echo ============================================================
echo  INDOTEL - REINTENTO DE CONFIGURACION WINDOWS
echo ============================================================
echo.
echo Este paso supone que Ubuntu ya publica:
echo   Core    5085
echo   Gateway 5185
echo   Web     5234
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File ".\CONFIGURAR_VM_WINDOWS.ps1"
if errorlevel 1 (
  echo.
  echo ERROR: La configuracion aun no termino.
  echo Revise CONFIGURACION_VM_DIAGNOSTICO.txt.
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File ".\PROBAR_CONEXION_VM_WINDOWS.ps1"
if errorlevel 1 (
  echo.
  echo ERROR: La compilacion termino, pero la prueba de red fallo.
  pause
  exit /b 1
)

echo.
echo ============================================================
echo  CONFIGURACION COMPLETADA
echo ============================================================
echo Ejecute INICIAR_APLICACIONES_VM_WINDOWS.bat
pause
