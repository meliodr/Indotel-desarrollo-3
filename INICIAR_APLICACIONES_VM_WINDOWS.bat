@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title INDOTEL - Aplicaciones VM Windows

powershell -NoProfile -ExecutionPolicy Bypass -File ".\PROBAR_CONEXION_VM_WINDOWS.ps1"
if errorlevel 1 (
  echo ERROR: La VM no puede comunicarse con Ubuntu.
  pause
  exit /b 1
)

start "Portal Web INDOTEL" "%~dp0ABRIR_WEB_VM_WINDOWS.bat"
start "Core Administrativo INDOTEL" "%~dp0INICIAR_GUI_CORE_VM_WINDOWS.bat"
start "Caja INDOTEL" "%~dp0INICIAR_CAJA_VM_WINDOWS.bat"
