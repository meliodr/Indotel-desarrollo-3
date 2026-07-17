@echo off
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0ESTADO_TODO_EN_WINDOWS.ps1"
pause
