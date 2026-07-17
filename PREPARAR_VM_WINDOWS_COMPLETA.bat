@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title Preparar VM Windows para INDOTEL

echo ============================================================
echo  INDOTEL - PREPARACION AUTOMATICA DE LA MAQUINA VIRTUAL
echo ============================================================
echo.
echo Carpeta del proyecto:
echo %CD%
echo.
echo Este archivo funciona aunque el proyecto este dentro de
echo Descargas, Escritorio o cualquier otra carpeta de Windows.
echo No mueva archivos individuales fuera de esta carpeta.
echo.

net session >nul 2>nul
if errorlevel 1 (
  echo Solicitando permisos de administrador...
  powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -WorkingDirectory '%~dp0' -Verb RunAs"
  exit /b
)

if not exist ".\vm-config.json" (
  echo ERROR: Falta vm-config.json. Extraiga nuevamente el ZIP completo.
  pause
  exit /b 1
)

if not exist ".\INDOTEL.sln" (
  echo ERROR: No se encontro INDOTEL.sln. Ejecute este archivo dentro de
  echo la carpeta INDOTEL-Proyecto-Completo extraida del ZIP.
  pause
  exit /b 1
)

echo [1/3] Verificando componentes de Visual Studio...
powershell -NoProfile -ExecutionPolicy Bypass -File ".\INSTALAR_COMPONENTES_VISUAL_STUDIO_WINDOWS.ps1"
if errorlevel 1 (
  echo.
  echo ERROR: No se pudieron preparar los componentes de Visual Studio.
  pause
  exit /b 1
)

echo.
echo [2/3] Configurando red, URLs y compilando aplicaciones...
powershell -NoProfile -ExecutionPolicy Bypass -File ".\CONFIGURAR_VM_WINDOWS.ps1"
if errorlevel 1 (
  echo.
  echo ERROR: No se pudo completar la configuracion de INDOTEL.
  echo Revise CONFIGURACION_VM_DIAGNOSTICO.txt si fue creado.
  pause
  exit /b 1
)

echo.
echo [3/3] Validando conexion final...
powershell -NoProfile -ExecutionPolicy Bypass -File ".\PROBAR_CONEXION_VM_WINDOWS.ps1"
if errorlevel 1 (
  echo.
  echo ERROR: La configuracion termino, pero Windows no llega a Ubuntu.
  pause
  exit /b 1
)

echo.
echo ============================================================
echo  VM PREPARADA CORRECTAMENTE
echo ============================================================
echo Use INICIAR_APLICACIONES_VM_WINDOWS.bat para abrir:
echo   - Caja WPF
echo   - Core administrativo WinForms
echo   - Portal Web
echo.
pause
