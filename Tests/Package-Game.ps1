param([ValidatePattern('^[a-zA-Z0-9.-]+$')][string]$Version = '4.2.1-mensajes')
$ErrorActionPreference = 'Stop'
$nebulaRoot = Split-Path -Parent $PSScriptRoot
$nebulaStage = Join-Path $nebulaRoot "Builds\Package-$Version"
$nebulaZip = Join-Path $nebulaRoot "Builds\EcosDeNebula-$Version.zip"
if ((Test-Path -LiteralPath $nebulaStage) -or (Test-Path -LiteralPath $nebulaZip)) {
    throw 'El destino ya existe. Usa otra versión; los paquetes anteriores se conservan.'
}
if (!(Test-Path -LiteralPath "$nebulaRoot\Builds\Windows\EcosDeNebula.exe")) { throw 'Falta compilar el juego.' }
New-Item -ItemType Directory -Path "$nebulaStage\Builds", "$nebulaStage\Server" | Out-Null
Copy-Item -LiteralPath "$nebulaRoot\Builds\Windows" -Destination "$nebulaStage\Builds\Windows" -Recurse
foreach ($nebulaFile in @('start.mjs','bridge.mjs','director.mjs')) {
    Copy-Item -LiteralPath "$nebulaRoot\Server\$nebulaFile" -Destination "$nebulaStage\Server"
}
foreach ($nebulaFile in @('Jugar.cmd','Jugar-con-IA.cmd','README.md','CHANGELOG.md','VERIFICACION.md')) {
    Copy-Item -LiteralPath "$nebulaRoot\$nebulaFile" -Destination $nebulaStage
}
$nebulaForbidden = Get-ChildItem -LiteralPath $nebulaStage -Recurse -Force -File | Where-Object {
    $_.Name -match '^\.env|^\.nebula-session|private-key|request\.json|fixture|\.test\.'
}
if ($nebulaForbidden) { throw 'Se detectó un archivo privado o de prueba. No se creará el ZIP.' }
Compress-Archive -LiteralPath "$nebulaStage\Builds", "$nebulaStage\Server", "$nebulaStage\Jugar.cmd", "$nebulaStage\Jugar-con-IA.cmd", "$nebulaStage\README.md", "$nebulaStage\CHANGELOG.md", "$nebulaStage\VERIFICACION.md" -DestinationPath $nebulaZip
Add-Type -AssemblyName System.IO.Compression.FileSystem
$nebulaArchive = [System.IO.Compression.ZipFile]::OpenRead($nebulaZip)
try {
    $nebulaNames = @($nebulaArchive.Entries | ForEach-Object { $_.FullName.Replace('\','/') })
    foreach ($nebulaRequired in @('Jugar.cmd','Server/start.mjs','Builds/Windows/EcosDeNebula.exe')) {
        if ($nebulaNames -notcontains $nebulaRequired) { throw "Falta $nebulaRequired en el ZIP." }
    }
    "PASS: paquete con $($nebulaNames.Count) entradas y sin credenciales."
} finally { $nebulaArchive.Dispose() }
Get-FileHash -LiteralPath $nebulaZip -Algorithm SHA256
