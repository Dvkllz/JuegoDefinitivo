param(
    [switch]$Unity,
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$nebulaRoot = Split-Path -Parent $PSScriptRoot
$nebulaCompiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
Push-Location $nebulaRoot
try {
    New-Item -ItemType Directory -Force -Path 'TestResults' | Out-Null
    & $nebulaCompiler /nologo /out:TestResults\PatternDirectorTests.exe Assets\EcosDeNebula\Scripts\PatternDirector.cs Tests\PatternDirectorTests.cs
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar la prueba de patrones.' }
    & .\TestResults\PatternDirectorTests.exe
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de patrones.' }
    & $nebulaCompiler /nologo /out:TestResults\ExpeditionTests.exe Assets\EcosDeNebula\Scripts\PatternDirector.cs Assets\EcosDeNebula\Scripts\ExpeditionRules.cs Assets\EcosDeNebula\Scripts\StarRace.cs Tests\ExpeditionTests.cs
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar la prueba de expediciones.' }
    & .\TestResults\ExpeditionTests.exe
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de expediciones.' }
    if ($Unity) {
        if (!(Test-Path -LiteralPath $UnityPath)) { throw 'No se encontró Unity; especifica -UnityPath.' }
        foreach ($nebulaMethod in @('Run', 'Expansion')) {
            $nebulaLog = Join-Path $nebulaRoot "TestResults\Unity-$nebulaMethod.log"
            $nebulaProcess = Start-Process -FilePath $UnityPath -ArgumentList @('-batchmode', '-projectPath', "`"$nebulaRoot`"", '-executeMethod', "EcosDeNebula.Editor.NebulaSmokeTest.$nebulaMethod", '-logFile', "`"$nebulaLog`"") -WindowStyle Hidden -PassThru -Wait
            if ($nebulaProcess.ExitCode -ne 0) { throw "Falló Unity $nebulaMethod. Revisa $nebulaLog" }
        }
    }
    Write-Output 'PASS: todas las comprobaciones solicitadas. No se llamó a OpenAI.'
} finally { Pop-Location }
