@echo off
title Schraubwerk CRM - Deinstallation
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1"
echo.
pause
