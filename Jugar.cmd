@echo off
set NEBULA_DISABLE_AI=1
if not exist "%~dp0Builds\Windows\EcosDeNebula.exe" (
    echo No se encontro el ejecutable. Conserva la carpeta Builds junto a este archivo.
    pause
    exit /b 1
)
start "" "%~dp0Builds\Windows\EcosDeNebula.exe"
