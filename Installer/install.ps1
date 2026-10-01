# -----------------------------------------------------------------------------
# Schraubwerk CRM – Demo-Installation
#  1. prüft SQL Server Express LocalDB und installiert es bei Bedarf (Microsoft)
#  2. kopiert das Programm nach %LOCALAPPDATA%\Programs\SchraubwerkCRM
#  3. legt Verknüpfungen auf dem Desktop und im Startmenü an
#  4. startet das Programm – Datenbank und Demo-Daten entstehen beim ersten Start
# Keine Administratorrechte nötig (außer für die einmalige LocalDB-Installation).
# -----------------------------------------------------------------------------
$ErrorActionPreference = 'Stop'
$quelle  = Split-Path -Parent $MyInvocation.MyCommand.Path
$appQuelle = Join-Path $quelle 'App'
$ziel    = Join-Path $env:LOCALAPPDATA 'Programs\SchraubwerkCRM'
$exe     = Join-Path $ziel 'SchraubwerkCRM.exe'
$localDbUrl = 'https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi'

function Schritt($text) { Write-Host ''; Write-Host "==> $text" -ForegroundColor Cyan }

function Finde-SqlLocalDb {
    $cmd = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $treffer = Get-ChildItem "$env:ProgramFiles\Microsoft SQL Server\*\Tools\Binn\SqlLocalDB.exe" -ErrorAction SilentlyContinue |
               Sort-Object FullName -Descending | Select-Object -First 1
    if ($treffer) { return $treffer.FullName }
    return $null
}

Write-Host 'Schraubwerk CRM – Demo-Installation' -ForegroundColor White

# --- 1) .NET Framework 4.7.2 (bei Windows 10/11 bereits vorhanden) ---
$release = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue).Release
if (-not $release -or $release -lt 461808) {
    Write-Host '.NET Framework 4.7.2 oder neuer fehlt. Bitte über Windows Update installieren.' -ForegroundColor Red
    exit 1
}

# --- 2) SQL Server Express LocalDB ---
Schritt 'SQL Server LocalDB prüfen'
$sqllocaldb = Finde-SqlLocalDb
if (-not $sqllocaldb) {
    Write-Host 'LocalDB ist nicht installiert – wird jetzt von Microsoft geladen (ca. 70 MB) ...'
    $msi = Join-Path $env:TEMP 'SqlLocalDB.msi'
    $lokal = Join-Path $quelle 'SqlLocalDB.msi'
    if (Test-Path $lokal) { Copy-Item $lokal $msi -Force }
    else {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri $localDbUrl -OutFile $msi -UseBasicParsing
    }
    Write-Host 'LocalDB wird installiert (Windows fragt nach Administratorrechten) ...'
    $p = Start-Process msiexec.exe -ArgumentList "/i `"$msi`" /qb IACCEPTSQLLOCALDBLICENSETERMS=YES" -Verb RunAs -Wait -PassThru
    if ($p.ExitCode -ne 0 -and $p.ExitCode -ne 3010) { throw "LocalDB-Installation fehlgeschlagen (Code $($p.ExitCode))." }
    $sqllocaldb = Finde-SqlLocalDb
    if (-not $sqllocaldb) { throw 'LocalDB wurde nicht gefunden. Bitte Rechner neu starten und die Installation erneut ausführen.' }
}
$ErrorActionPreference = 'Continue'   # SqlLocalDB schreibt Hinweise nach stderr
& $sqllocaldb create MSSQLLocalDB *> $null   # existiert meist schon
& $sqllocaldb start  MSSQLLocalDB *> $null
$ErrorActionPreference = 'Stop'
Write-Host 'LocalDB ist bereit.' -ForegroundColor Green

# --- 3) Programm kopieren ---
Schritt "Programm nach $ziel kopieren"
Get-Process SchraubwerkCRM -ErrorAction SilentlyContinue | Stop-Process -Force
New-Item -ItemType Directory -Force -Path $ziel | Out-Null
Copy-Item (Join-Path $appQuelle '*') $ziel -Recurse -Force
Copy-Item (Join-Path $quelle 'Deinstallieren.cmd') $ziel -Force
Copy-Item (Join-Path $quelle 'uninstall.ps1') $ziel -Force

# --- 4) Verknüpfungen ---
Schritt 'Verknüpfungen anlegen'
$shell = New-Object -ComObject WScript.Shell
$startmenue = Join-Path ([Environment]::GetFolderPath('Programs')) 'Schraubwerk CRM'
New-Item -ItemType Directory -Force -Path $startmenue | Out-Null
foreach ($lnk in @((Join-Path ([Environment]::GetFolderPath('Desktop')) 'Schraubwerk CRM.lnk'),
                   (Join-Path $startmenue 'Schraubwerk CRM.lnk'))) {
    $s = $shell.CreateShortcut($lnk)
    $s.TargetPath = $exe
    $s.WorkingDirectory = $ziel
    $s.IconLocation = "$exe,0"
    $s.Description = 'Schraubwerk CRM (Demo)'
    $s.Save()
}
$u = $shell.CreateShortcut((Join-Path $startmenue 'Schraubwerk CRM deinstallieren.lnk'))
$u.TargetPath = Join-Path $ziel 'Deinstallieren.cmd'
$u.WorkingDirectory = $ziel
$u.Save()

Schritt 'Fertig!'
Write-Host 'Anmeldung:  Benutzer demo  /  Passwort demo123  (wird automatisch eingetragen)' -ForegroundColor Green
Write-Host 'Der erste Start dauert einige Sekunden – Datenbank und Demo-Daten werden angelegt.'
Start-Process $exe -WorkingDirectory $ziel
