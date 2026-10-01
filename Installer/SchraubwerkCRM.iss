; -----------------------------------------------------------------------------
; Optional: klassischer Setup-Assistent (setup.exe) mit Inno Setup 6
; (https://jrsoftware.org/isinfo.php). Datei in Inno Setup öffnen -> Kompilieren.
; Vorher in Visual Studio "Release" erstellen. Die ZIP-Variante mit
; Installieren.cmd funktioniert auch ohne Inno Setup.
; -----------------------------------------------------------------------------
#define AppName "Schraubwerk CRM"
#define AppVersion "1.0"

[Setup]
AppId={{6C4C2A47-5A8E-4C35-9C62-5B7C0B1F3D21}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Amir Reza Afshar
AppPublisherURL=https://amirrezaafshar.de
DefaultDirName={localappdata}\Programs\SchraubwerkCRM
DefaultGroupName={#AppName}
PrivilegesRequired=lowest
OutputDir=Ausgabe
OutputBaseFilename=SchraubwerkCRM-Demo-Setup
SetupIconFile=..\CRMMain\app.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Files]
Source: "..\CRMMain\bin\Release\*"; DestDir: "{app}"; Excludes: "*.pdb,*.xml"; Flags: recursesubdirs ignoreversion
Source: "Ausgabe\SchraubwerkCRM-Demo\App\SchraubwerkCRM.exe.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\SchraubwerkCRM.exe"
Name: "{userdesktop}\{#AppName}"; Filename: "{app}\SchraubwerkCRM.exe"
Name: "{group}\{#AppName} deinstallieren"; Filename: "{uninstallexe}"

[Run]
; LocalDB prüfen/installieren (nutzt denselben Teil wie die ZIP-Installation)
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""if (-not (Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue)) {{ $m=Join-Path $env:TEMP 'SqlLocalDB.msi'; Invoke-WebRequest 'https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi' -OutFile $m -UseBasicParsing; Start-Process msiexec.exe -ArgumentList ('/i ""' + $m + '"" /qb IACCEPTSQLLOCALDBLICENSETERMS=YES') -Verb RunAs -Wait }"""; StatusMsg: "SQL Server LocalDB wird geprüft ..."; Flags: runhidden
Filename: "{app}\SchraubwerkCRM.exe"; Description: "{#AppName} starten"; Flags: nowait postinstall skipifsilent
