// -----------------------------------------------------------------------------
// Schraubwerk CRM – grafischer Setup-Assistent (Willkommen → Installation → Fertig)
//
// Eine einzige Datei „SchraubwerkCRM-Setup.exe“:
//   - das Programm steckt als eingebettete ZIP-Ressource („payload.zip“) darin
//   - fehlt SQL Server Express LocalDB, wird der Microsoft-Installer geladen und mit
//     Oberfläche gestartet (Zustimmung zu Microsofts Lizenz erfolgt dort durch den Benutzer)
//   - Installation nach %LOCALAPPDATA%\Programs\SchraubwerkCRM (keine Admin-Rechte nötig)
//   - Verknüpfungen auf dem Desktop und im Startmenü, Eintrag unter „Apps & Features“
//   - Aufruf mit /uninstall entfernt das Programm wieder
//
// Erstellen (Windows):  csc /target:winexe /win32icon:app.ico /resource:payload.zip /resource:logo.png
//                        /resource:LICENSE.txt /resource:THIRD-PARTY-NOTICES.txt
//                        /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll Setup.cs
// -----------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Schraubwerk CRM Setup")]
[assembly: AssemblyProduct("Schraubwerk CRM")]
[assembly: AssemblyCompany("Amir Reza Afshar")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace SchraubwerkSetup
{
    static class Program
    {
        [DllImport("user32.dll")] static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main(string[] args)
        {
            try { SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool uninstall = args.Any(a => a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase));
            if (uninstall) Uninstaller.Run();
            else Application.Run(new SetupForm());
        }
    }

    static class App
    {
        public const string Name = "Schraubwerk CRM";
        public const string Exe = "SchraubwerkCRM.exe";
        public const string RegKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\SchraubwerkCRM";
        public const string LocalDbUrl = "https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi";
        public const string LocalDbTerms = "https://www.microsoft.com/content/dam/microsoft/usetm/documents/sql-server/sql-server-2022-developer-express-evaluation/retail-packaged/SQLServer2022_SQLServer2022DeveloperExpressEvaluation_English.pdf";
        public static readonly Color Navy = Color.FromArgb(16, 37, 66);
        public static readonly Color NavyLight = Color.FromArgb(52, 93, 153);
        public static readonly Color Light = Color.FromArgb(235, 242, 250);

        public static string DefaultDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\SchraubwerkCRM"); }
        }
        public static string StartMenuDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), Name); }
        }
        public static string DesktopLink
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Name + ".lnk"); }
        }

        public static void CreateShortcut(string lnk, string target, string workDir, string icon)
        {
            Type t = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(t);
            object sc = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
            Type st = sc.GetType();
            st.InvokeMember("TargetPath", BindingFlags.SetProperty, null, sc, new object[] { target });
            st.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, sc, new object[] { workDir });
            st.InvokeMember("IconLocation", BindingFlags.SetProperty, null, sc, new object[] { icon + ",0" });
            st.InvokeMember("Save", BindingFlags.InvokeMethod, null, sc, null);
            Marshal.FinalReleaseComObject(sc);
            Marshal.FinalReleaseComObject(shell);
        }

        // LocalDB ist installiert, wenn Microsoft es in der Registry einträgt
        public static bool LocalDbInstalled()
        {
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (var k = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions"))
                        if (k != null && k.GetSubKeyNames().Length > 0) return true;
                }
                catch { }
            }
            return false;
        }

        public static Image Logo()
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("logo.png");
            return s == null ? null : Image.FromStream(s);
        }
    }

    // ---------------------------------------------------------------------------
    // Assistent
    // ---------------------------------------------------------------------------
    class SetupForm : Form
    {
        readonly Panel side = new Panel();
        readonly Panel content = new Panel();
        readonly Label[] steps = new Label[3];
        readonly Button btnNext = new Button();
        readonly Button btnCancel = new Button();

        // Seite 1
        readonly Panel page1 = new Panel();
        readonly TextBox txtDir = new TextBox();
        readonly CheckBox chkDesktop = new CheckBox();
        readonly CheckBox chkAccept = new CheckBox();
        // Seite 2
        readonly Panel page2 = new Panel();
        readonly ProgressBar progress = new ProgressBar();
        readonly Label lblStatus = new Label();
        // Seite 3
        readonly Panel page3 = new Panel();
        readonly CheckBox chkStart = new CheckBox();

        int page = 1;
        bool installing;

        public SetupForm()
        {
            Text = App.Name + " – Setup";
            Font = new Font("Segoe UI", 9.75f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            // linke Leiste mit Logo und Schritten
            side.SetBounds(0, 0, 190, 400);
            side.BackColor = App.Navy;
            Controls.Add(side);
            var logo = new PictureBox { Image = App.Logo(), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
            logo.SetBounds(18, 24, 154, 46);
            side.Controls.Add(logo);
            string[] names = { "1  Willkommen", "2  Installation", "3  Fertig" };
            for (int i = 0; i < 3; i++)
            {
                steps[i] = new Label { Text = names[i], AutoSize = false, ForeColor = Color.FromArgb(170, 185, 205), BackColor = Color.Transparent };
                steps[i].SetBounds(22, 110 + i * 36, 160, 26);
                side.Controls.Add(steps[i]);
            }
            var foot = new Label { Text = "Demo-Version\namirrezaafshar.de", ForeColor = Color.FromArgb(150, 165, 190), AutoSize = false, Font = new Font("Segoe UI", 8.25f) };
            foot.SetBounds(22, 340, 160, 40);
            side.Controls.Add(foot);

            content.SetBounds(190, 0, 450, 340);
            Controls.Add(content);

            // untere Leiste
            var bar = new Panel { BackColor = App.Light };
            bar.SetBounds(190, 340, 450, 60);
            Controls.Add(bar);
            StyleButton(btnNext, true);
            btnNext.EnabledChanged += (s, e) => btnNext.BackColor = btnNext.Enabled ? App.Navy : Color.FromArgb(160, 170, 185);
            btnNext.SetBounds(316, 14, 116, 32);
            btnNext.Click += Next_Click;
            StyleButton(btnCancel, false);
            btnCancel.SetBounds(192, 14, 116, 32);
            btnCancel.Text = "Abbrechen";
            btnCancel.Click += (s, e) => Close();
            bar.Controls.Add(btnNext);
            bar.Controls.Add(btnCancel);
            AcceptButton = btnNext;

            BuildPage1();
            BuildPage2();
            BuildPage3();
            ShowPage(1);
            Shown += (s, e) => btnNext.Focus();
        }

        static void StyleButton(Button b, bool primary)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = primary ? 0 : 1;
            b.FlatAppearance.BorderColor = App.NavyLight;
            b.BackColor = primary ? App.Navy : Color.White;
            b.ForeColor = primary ? Color.White : App.Navy;
            b.Cursor = Cursors.Hand;
        }

        static Label Title(string text)
        {
            var l = new Label { Text = text, Font = new Font("Segoe UI Semibold", 15f), ForeColor = App.Navy, AutoSize = false };
            l.SetBounds(30, 26, 400, 36);
            return l;
        }

        static Label Body(string text, int top, int height)
        {
            var l = new Label { Text = text, AutoSize = false, ForeColor = Color.FromArgb(40, 45, 55) };
            l.SetBounds(30, top, 395, height);
            return l;
        }

        void BuildPage1()
        {
            page1.Dock = DockStyle.Fill;
            page1.Controls.Add(Title("Willkommen"));
            page1.Controls.Add(Body("Installiert die Demo-Version von Schraubwerk CRM.\n\n" +
                                    "Datenbank und Demo-Daten entstehen beim ersten Start. " +
                                    "Fehlt LocalDB, wird der Microsoft-Installer geladen – dort gelten Microsofts eigene Lizenzbedingungen.", 66, 100));
            var lbl = Body("Installationsordner:", 172, 22);
            page1.Controls.Add(lbl);
            txtDir.Text = App.DefaultDir;
            txtDir.SetBounds(30, 196, 300, 26);
            page1.Controls.Add(txtDir);
            var browse = new Button { Text = "Ändern …" };
            StyleButton(browse, false);
            browse.SetBounds(338, 194, 88, 28);
            browse.Click += (s, e) =>
            {
                using (var d = new FolderBrowserDialog { SelectedPath = txtDir.Text })
                    if (d.ShowDialog(this) == DialogResult.OK) txtDir.Text = Path.Combine(d.SelectedPath, "SchraubwerkCRM");
            };
            page1.Controls.Add(browse);
            chkDesktop.Text = "Verknüpfung auf dem Desktop anlegen";
            chkDesktop.Checked = true;
            chkDesktop.SetBounds(30, 232, 380, 24);
            page1.Controls.Add(chkDesktop);

            // Lizenzbedingungen müssen bestätigt werden
            chkAccept.Text = "Ich akzeptiere die Lizenz von Schraubwerk CRM";
            chkAccept.SetBounds(30, 266, 396, 24);
            chkAccept.CheckedChanged += (s, e) => btnNext.Enabled = chkAccept.Checked;
            page1.Controls.Add(chkAccept);
            var lnkApp = new LinkLabel { Text = "Lizenz und Drittanbieter", AutoSize = true, UseMnemonic = false, LinkColor = App.NavyLight };
            lnkApp.Location = new Point(48, 294);
            lnkApp.LinkClicked += (s, e) => ShowText("Lizenz und Hinweise", ReadResource("LICENSE.txt") + "\r\n\r\n" + ReadResource("THIRD-PARTY-NOTICES.txt"));
            page1.Controls.Add(lnkApp);
            var lnkMs = new LinkLabel { Text = "LocalDB-Lizenz (Microsoft)", AutoSize = true, UseMnemonic = false, LinkColor = App.NavyLight };
            lnkMs.Location = new Point(220, 294);
            lnkMs.LinkClicked += (s, e) => { try { Process.Start(App.LocalDbTerms); } catch { } };
            page1.Controls.Add(lnkMs);
            content.Controls.Add(page1);
        }

        static string ReadResource(string name)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (s == null) return "";
                using (var r = new StreamReader(s, System.Text.Encoding.UTF8)) return r.ReadToEnd();
            }
        }

        void ShowText(string title, string text)
        {
            using (var f = new Form { Text = title, ClientSize = new Size(640, 480), StartPosition = FormStartPosition.CenterParent, Font = Font, MinimizeBox = false })
            {
                var box = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Text = text, BackColor = Color.White, Font = new Font("Consolas", 9f) };
                f.Controls.Add(box);
                f.Shown += (s, e) => box.Select(0, 0);
                f.ShowDialog(this);
            }
        }

        void BuildPage2()
        {
            page2.Dock = DockStyle.Fill;
            page2.Controls.Add(Title("Installation"));
            page2.Controls.Add(Body("Bitte warten – Schraubwerk CRM wird installiert.", 72, 24));
            progress.SetBounds(30, 130, 395, 22);
            progress.Maximum = 100;
            page2.Controls.Add(progress);
            lblStatus.AutoSize = false;
            lblStatus.ForeColor = App.NavyLight;
            lblStatus.SetBounds(30, 160, 395, 60);
            page2.Controls.Add(lblStatus);
            content.Controls.Add(page2);
        }

        void BuildPage3()
        {
            page3.Dock = DockStyle.Fill;
            page3.Controls.Add(Title("Fertig"));
            page3.Controls.Add(Body("Schraubwerk CRM wurde erfolgreich installiert.\n\n" +
                                    "Anmeldung:   Benutzer  demo   ·   Passwort  demo123\n(wird automatisch eingetragen)\n\n" +
                                    "Der erste Start dauert einige Sekunden, weil Datenbank und Demo-Daten angelegt werden.", 72, 150));
            chkStart.Text = "Schraubwerk CRM jetzt starten";
            chkStart.Checked = true;
            chkStart.SetBounds(30, 236, 380, 24);
            page3.Controls.Add(chkStart);
            content.Controls.Add(page3);
        }

        void ShowPage(int p)
        {
            page = p;
            page1.Visible = p == 1;
            page2.Visible = p == 2;
            page3.Visible = p == 3;
            for (int i = 0; i < 3; i++)
            {
                bool active = i == p - 1;
                steps[i].ForeColor = active ? Color.White : Color.FromArgb(170, 185, 205);
                steps[i].Font = new Font("Segoe UI", 10f, active ? FontStyle.Bold : FontStyle.Regular);
            }
            btnNext.Text = p == 1 ? "Installieren" : p == 2 ? "Bitte warten …" : "Fertigstellen";
            btnNext.Enabled = p == 3 || (p == 1 && chkAccept.Checked);
            btnCancel.Visible = p == 1;
        }

        void Next_Click(object sender, EventArgs e)
        {
            if (page == 1) StartInstall();
            else if (page == 3)
            {
                if (chkStart.Checked)
                {
                    string exe = Path.Combine(txtDir.Text, App.Exe);
                    try { Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = txtDir.Text }); } catch { }
                }
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (installing) e.Cancel = true; // während der Installation nicht schließen
            base.OnFormClosing(e);
        }

        void Status(string text, int value)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => Status(text, value))); return; }
            lblStatus.Text = text;
            progress.Value = Math.Max(0, Math.Min(100, value));
        }

        void StartInstall()
        {
            string dir = txtDir.Text.Trim();
            if (dir.Length == 0) return;
            bool desktop = chkDesktop.Checked;
            ShowPage(2);
            installing = true;
            var worker = new Thread(() =>
            {
                try
                {
                    Install(dir, desktop);
                    BeginInvoke(new Action(() => { installing = false; ShowPage(3); }));
                }
                catch (Exception ex)
                {
                    BeginInvoke(new Action(() =>
                    {
                        installing = false;
                        MessageBox.Show(this, "Die Installation ist fehlgeschlagen:\n\n" + ex.Message, App.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ShowPage(1);
                    }));
                }
            });
            worker.IsBackground = true;
            worker.Start();
        }

        void Install(string dir, bool desktop)
        {
            // 1) LocalDB
            Status("SQL Server LocalDB wird geprüft …", 3);
            if (!App.LocalDbInstalled())
            {
                string msi = Path.Combine(Path.GetTempPath(), "SqlLocalDB.msi");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var wc = new WebClient())
                {
                    var done = new ManualResetEvent(false);
                    Exception error = null;
                    wc.DownloadProgressChanged += (s, e) =>
                        Status(string.Format("SQL Server LocalDB wird von Microsoft geladen … {0} von {1} MB",
                               e.BytesReceived / 1048576, Math.Max(1, e.TotalBytesToReceive / 1048576)), 5 + e.ProgressPercentage * 45 / 100);
                    wc.DownloadFileCompleted += (s, e) => { error = e.Error; done.Set(); };
                    wc.DownloadFileAsync(new Uri(App.LocalDbUrl), msi);
                    done.WaitOne();
                    if (error != null) throw new Exception("LocalDB konnte nicht geladen werden (Internetverbindung prüfen).\n" + error.Message);
                }
                // Kein IACCEPTSQLLOCALDBLICENSETERMS: Der Microsoft-Installer zeigt seine eigenen
                // Lizenzbedingungen, und der Benutzer stimmt ihnen dort selbst zu.
                Status("Bitte im Microsoft-Installer die Lizenzbedingungen lesen und LocalDB installieren …", 55);
                var p = Process.Start(new ProcessStartInfo("msiexec.exe", "/i \"" + msi + "\"")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                });
                p.WaitForExit();
                if (p.ExitCode != 0 && p.ExitCode != 3010)
                    throw new Exception("LocalDB wurde nicht installiert (Code " + p.ExitCode + ").\nOhne LocalDB kann Schraubwerk CRM nicht starten. Setup bitte erneut ausführen und im Microsoft-Installer den Lizenzbedingungen zustimmen.");
            }

            // 2) Dateien
            Status("Programmdateien werden kopiert …", 65);
            foreach (var proc in Process.GetProcessesByName("SchraubwerkCRM")) { try { proc.Kill(); proc.WaitForExit(3000); } catch { } }
            Directory.CreateDirectory(dir);
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
            using (var zip = new ZipArchive(s, ZipArchiveMode.Read))
            {
                int n = 0, total = zip.Entries.Count;
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string target = Path.GetFullPath(Path.Combine(dir, entry.FullName));
                    if (!target.StartsWith(Path.GetFullPath(dir), StringComparison.OrdinalIgnoreCase)) continue;
                    if (entry.FullName.EndsWith("/")) { Directory.CreateDirectory(target); continue; }
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    entry.ExtractToFile(target, true);
                    Status("Programmdateien werden kopiert … " + entry.Name, 65 + (++n * 25 / Math.Max(1, total)));
                }
            }
            string uninstaller = Path.Combine(dir, "Deinstallieren.exe");
            File.Copy(Application.ExecutablePath, uninstaller, true);

            // 3) Verknüpfungen + Eintrag in „Apps & Features“
            Status("Verknüpfungen werden angelegt …", 93);
            string exe = Path.Combine(dir, App.Exe);
            Directory.CreateDirectory(App.StartMenuDir);
            App.CreateShortcut(Path.Combine(App.StartMenuDir, App.Name + ".lnk"), exe, dir, exe);
            if (desktop) App.CreateShortcut(App.DesktopLink, exe, dir, exe);
            using (var k = Registry.CurrentUser.CreateSubKey(App.RegKey))
            {
                k.SetValue("DisplayName", App.Name + " (Demo)");
                k.SetValue("DisplayVersion", "1.0");
                k.SetValue("Publisher", "Amir Reza Afshar");
                k.SetValue("DisplayIcon", exe);
                k.SetValue("InstallLocation", dir);
                k.SetValue("UninstallString", "\"" + uninstaller + "\" /uninstall");
                k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
            Status("Fertig.", 100);
        }
    }

    // ---------------------------------------------------------------------------
    // Deinstallation (Deinstallieren.exe /uninstall bzw. „Apps & Features“)
    // ---------------------------------------------------------------------------
    static class Uninstaller
    {
        public static void Run()
        {
            if (MessageBox.Show("Schraubwerk CRM wirklich entfernen?", App.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            bool db = MessageBox.Show("Soll auch die Demo-Datenbank (DBCRM) gelöscht werden?", App.Name,
                                      MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

            string dir = null;
            using (var k = Registry.CurrentUser.OpenSubKey(App.RegKey))
                if (k != null) dir = k.GetValue("InstallLocation") as string;
            if (string.IsNullOrEmpty(dir)) dir = Path.GetDirectoryName(Application.ExecutablePath);

            foreach (var proc in Process.GetProcessesByName("SchraubwerkCRM")) { try { proc.Kill(); } catch { } }

            if (db)
            {
                try
                {
                    using (var con = new System.Data.SqlClient.SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=true"))
                    {
                        con.Open();
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "IF DB_ID('DBCRM') IS NOT NULL BEGIN ALTER DATABASE DBCRM SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE DBCRM; END";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Die Datenbank konnte nicht gelöscht werden:\n" + ex.Message, App.Name); }
            }

            try { File.Delete(App.DesktopLink); } catch { }
            try { Directory.Delete(App.StartMenuDir, true); } catch { }
            try { Registry.CurrentUser.DeleteSubKeyTree(App.RegKey, false); } catch { }

            MessageBox.Show("Schraubwerk CRM wurde entfernt.", App.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Ordner löschen, sobald dieses Programm beendet ist (es liegt selbst darin)
            Process.Start(new ProcessStartInfo("cmd.exe", "/c timeout /t 2 /nobreak >nul & rmdir /s /q \"" + dir + "\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetTempPath()
            });
        }
    }
}
