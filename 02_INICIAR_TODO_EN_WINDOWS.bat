@echo off
setlocal
cd /d "%~dp0"
title INDOTEL - Iniciar todo en Windows
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0INICIAR_TODO_EN_WINDOWS.ps1"
set "RC=%errorlevel%"
if not "%RC%"=="0" (
  echo.
  echo ERROR AL INICIAR INDOTEL. Revise .windows-runtime\logs
  pause
)
exit /b %RC%
