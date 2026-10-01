# -----------------------------------------------------------------------------
# Erzeugt Installer\Ausgabe\SchraubwerkCRM-Setup.exe aus CRMMain\bin\Release
# Vorher in Visual Studio: Konfiguration "Release" -> Projektmappe neu erstellen
# -----------------------------------------------------------------------------
$ErrorActionPreference = 'Stop'
$hier = $PSScriptRoot
$root = (Resolve-Path (Join-Path $hier '..\..')).Path
$rel  = Join-Path $root 'CRMMain\bin\Release'
if (-not (Test-Path (Join-Path $rel 'SchraubwerkCRM.exe'))) { throw 'Bitte zuerst in Visual Studio die Konfiguration "Release" erstellen.' }

Write-Host 'Programmdateien vorbereiten ...'
$tmp = Join-Path $env:TEMP 'SchraubwerkPayload'
if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }
Copy-Item $rel $tmp -Recurse
Get-ChildItem $tmp -Recurse -Include *.pdb, *.xml | Remove-Item -Force

# Installationsversion: immer LocalDB und Demo-Modus
$cfg = Join-Path $tmp 'SchraubwerkCRM.exe.config'
$text = [IO.File]::ReadAllText($cfg)
$text = $text -replace 'Data Source=[^;]*;', 'Data Source=(localdb)\MSSQLLocalDB;'
$text = $text -replace 'key="DemoMode" value="[^"]*"', 'key="DemoMode" value="true"'
[IO.File]::WriteAllText($cfg, $text)

Write-Host 'Paket packen ...'
$payload = Join-Path $hier 'payload.zip'
if (Test-Path $payload) { Remove-Item $payload -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($tmp, $payload)
Copy-Item (Join-Path $root 'CRMMain\image\logo.png') $hier -Force
Copy-Item (Join-Path $root 'CRMMain\app.ico') $hier -Force
Copy-Item (Join-Path $root 'LICENSE.txt') $hier -Force
Copy-Item (Join-Path $root 'THIRD-PARTY-NOTICES.txt') $hier -Force

Write-Host 'Setup.exe kompilieren ...'
$aus = Join-Path $root 'Installer\Ausgabe'
New-Item -ItemType Directory -Force -Path $aus | Out-Null
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
Push-Location $hier
& $csc /nologo /target:winexe /optimize+ /win32icon:app.ico /resource:payload.zip,payload.zip /resource:logo.png,logo.png /resource:LICENSE.txt,LICENSE.txt /resource:THIRD-PARTY-NOTICES.txt,THIRD-PARTY-NOTICES.txt `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll `
    "/out:$aus\SchraubwerkCRM-Setup.exe" Setup.cs
Pop-Location
if ($LASTEXITCODE -ne 0) { throw 'Kompilieren fehlgeschlagen.' }
Write-Host ''
Write-Host "Fertig: $aus\SchraubwerkCRM-Setup.exe" -ForegroundColor Green
