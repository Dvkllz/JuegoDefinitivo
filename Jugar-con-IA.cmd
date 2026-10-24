@echo off
cd /d "%~dp0"
node "%~dp0Server\start.mjs"
if errorlevel 1 pause
