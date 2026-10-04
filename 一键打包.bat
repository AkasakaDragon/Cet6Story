@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0一键打包.ps1"
if errorlevel 1 echo Package failed. Please check the message above.
pause
