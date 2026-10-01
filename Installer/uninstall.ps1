# Schraubwerk CRM – Deinstallation (Programm, Verknüpfungen und optional die Demo-Datenbank)
$ErrorActionPreference = 'Continue'
$ziel = Join-Path $env:LOCALAPPDATA 'Programs\SchraubwerkCRM'
Get-Process SchraubwerkCRM -ErrorAction SilentlyContinue | Stop-Process -Force

$antwort = Read-Host 'Demo-Datenbank DBCRM ebenfalls löschen? (j/n)'
if ($antwort -match '^[jJyY]') {
    try {
        $con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=true'
        $con.Open()
        $cmd = $con.CreateCommand()
        $cmd.CommandText = "IF DB_ID('DBCRM') IS NOT NULL BEGIN ALTER DATABASE DBCRM SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE DBCRM; END"
        [void]$cmd.ExecuteNonQuery()
        $con.Close()
        Write-Host 'Datenbank gelöscht.'
    } catch { Write-Host "Datenbank konnte nicht gelöscht werden: $($_.Exception.Message)" }
}

Remove-Item (Join-Path ([Environment]::GetFolderPath('Desktop')) 'Schraubwerk CRM.lnk') -ErrorAction SilentlyContinue
Remove-Item (Join-Path ([Environment]::GetFolderPath('Programs')) 'Schraubwerk CRM') -Recurse -Force -ErrorAction SilentlyContinue
# Programmordner nach kurzer Pause löschen (dieses Skript liegt selbst darin)
Start-Process cmd.exe -ArgumentList "/c timeout /t 2 >nul & rmdir /s /q `"$ziel`"" -WindowStyle Hidden
Write-Host 'Schraubwerk CRM wurde entfernt.'
