// -----------------------------------------------------------------------------
// Demo-Modus für die Installationsversion
//
// Ist in der .config  <add key="DemoMode" value="true" />  gesetzt, bereitet das
// Programm beim Start alles selbst vor – der Tester muss nichts einrichten:
//   1. Datenbank + Tabellen werden über Entity Framework automatisch erstellt
//   2. Gespeicherte Prozeduren werden aus "Setup\Prozeduren.sql" angelegt bzw.
//      aktualisiert (so kommen Korrekturen auch in bestehende Datenbanken)
//   3. Demo-Daten werden ergänzt: Benutzer, Rollen, Kunden, Produkte mit Bildern,
//      Aktivitäten, Erinnerungen und Rechnungen. Jeder Teil wird nur angelegt,
//      wenn er noch fehlt – vorhandene Daten bleiben erhalten.
//
// Benutzername und Passwort (demo / demo123) werden auf dem Startbildschirm
// automatisch eingetragen. Ohne DemoMode passiert hier nichts.
// -----------------------------------------------------------------------------
using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace çağdaşcivata
{
    public static class DemoSetup
    {
        public const string DemoUser = "demo";
        public const string DemoPassword = "demo123";
        const string SalesUser = "m.schulz";

        static string ConStr { get { return ConfigurationManager.ConnectionStrings["constr"].ConnectionString; } }
        static string Bild(string file) { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup", "Bilder", file); }

        public static bool Enabled
        {
            get
            {
                string v = ConfigurationManager.AppSettings["DemoMode"];
                return string.Equals(v, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void EnsureReady()
        {
            // 1) Erster Datenbankzugriff -> EF legt DBCRM samt Tabellen an, falls sie fehlt
            USER_BLL ubll = new USER_BLL();
            bool hasUsers = ubll.İsregestered();

            // 2) Gespeicherte Prozeduren anlegen / aktualisieren
            EnsureProcedures();

            // 3) Demo-Daten
            if (!hasUsers) SeedAdmin();
            RepairOldDemoData();
            SeedMissing();
        }

        #region Datenbank-Hilfen
        static object Scalar(string sql, params SqlParameter[] p)
        {
            using (var con = new SqlConnection(ConStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(p);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }
        static int Count(string sql, params SqlParameter[] p) { return Convert.ToInt32(Scalar(sql, p)); }
        static void Exec(string sql, params SqlParameter[] p)
        {
            using (var con = new SqlConnection(ConStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(p);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        static SqlParameter P(string n, object v) { return new SqlParameter(n, v ?? DBNull.Value); }
        #endregion

        static void EnsureProcedures()
        {
            string script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup", "Prozeduren.sql");
            if (!File.Exists(script)) return;

            string sql = File.ReadAllText(script, Encoding.UTF8);
            using (var con = new SqlConnection(ConStr))
            {
                con.Open();
                foreach (string batch in Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    string b = batch.Trim();
                    if (b.Length == 0 || b.StartsWith("USE ", StringComparison.OrdinalIgnoreCase)) continue;
                    // vorhandene Prozedur ersetzen -> Korrekturen kommen auch in alte Datenbanken
                    Match m = Regex.Match(b, @"create\s+procedure\s+(?:\[?dbo\]?\.)?\[?([^\s\[\]]+)\]?", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        using (var drop = new SqlCommand("IF OBJECT_ID(N'dbo." + m.Groups[1].Value + "', 'P') IS NOT NULL DROP PROCEDURE dbo.[" + m.Groups[1].Value + "]", con))
                            drop.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand(b, con)) cmd.ExecuteNonQuery();
                }
            }
        }

        static USER_ACCESS_ROLE Role(string section, bool read, bool create, bool update, bool delete)
        {
            return new USER_ACCESS_ROLE { Section = section, CanRead = read, CanCreate = create, CanUpdate = update, CamDelete = delete };
        }

        static readonly string[] Sections = { "Kunden", "Produkte", "Verkauf", "Aktivitäten", "Erinnerungen", "Benutzer", "SMS-Portal", "Berichte", "Einstellungen" };

        // Leere Datenbank: Admin-Rolle und Demo-Admin (wie beim normalen Ersteinrichten)
        static void SeedAdmin()
        {
            USER_GROUP_BLL ugbll = new USER_GROUP_BLL();
            USER_GROUP ug = new USER_GROUP { Title = "Admin" };
            foreach (string s in Sections) ug.Roles.Add(Role(s, true, true, true, true));
            ugbll.Create(ug);

            new USER_BLL().Create(new USER
            {
                Name = "Demo Admin",
                UserName = DemoUser,
                Password = DemoPassword,
                TC = "D-1001",
                E_mail = "demo@schraubwerk.example",
                PhoneNumber = "0711 000000",
                Adress = "Musterstraße 1, 70173 Stuttgart",
                Status = "Admin",
                Pic = Bild("benutzer_demo.jpg"),
                Regtime = DateTime.Now.Date
            }, ugbll.getug_bytitle("Admin"), true);
        }

        // Ältere Demo-Daten an die aktuelle Schreibweise anpassen (nur Demo-Einträge)
        static void RepairOldDemoData()
        {
            Exec("UPDATE dbo.PRODUCTs SET Cap = SUBSTRING(Cap, 2, 20) WHERE BrandName = 'SCHRAUBWERK' AND Cap LIKE 'M[0-9]%'");
            Exec("UPDATE dbo.PRODUCTs SET Kaplama = 'Verzinkt' WHERE Kaplama COLLATE Latin1_General_CS_AS = 'VERZINKT'");
            Exec("UPDATE dbo.PRODUCTs SET Kaplama = 'Schwarz' WHERE Kaplama COLLATE Latin1_General_CS_AS = 'SCHWARZ'");
            Exec("UPDATE dbo.PRODUCTs SET Name = 'Zylinderschraube' WHERE BrandName = 'SCHRAUBWERK' AND Name = 'Zylinderschraube Innensechskant'");
            Exec("UPDATE dbo.USERs SET Name = 'Demo Admin' WHERE UserName = @u AND Name = 'Demo-Administrator'", P("@u", DemoUser));
            Exec("UPDATE dbo.USERs SET Pic = @p WHERE UserName = @u AND (Pic IS NULL OR Pic = '')", P("@p", Bild("benutzer_demo.jpg")), P("@u", DemoUser));
            var mails = new Dictionary<string, string>
            {
                { "einkauf@mueller-maschinenbau.example", "einkauf@mueller.example" },
                { "info@wagner-metallbau.example", "info@wagner.example" },
                { "m.becker@becker-fzt.example", "becker@becker.example" },
                { "bestellung@schneider-bau.example", "info@schneider.example" }
            };
            foreach (var m in mails)
                Exec("UPDATE dbo.CUSTOMERs SET Email = @n WHERE Email = @o", P("@n", m.Value), P("@o", m.Key));
        }

        static void SeedMissing()
        {
            USER_BLL ubll = new USER_BLL();
            USER admin = ubll.Readuser(DemoUser);
            if (admin == null) return; // Datenbank gehört nicht zur Demo -> nichts ergänzen

            // --- zweiter Benutzer mit eingeschränkter Rolle "Vertrieb" ---
            USER_GROUP_BLL ugbll = new USER_GROUP_BLL();
            if (ugbll.getug_bytitle("Vertrieb") == null)
            {
                USER_GROUP ug = new USER_GROUP { Title = "Vertrieb" };
                foreach (string s in Sections)
                {
                    bool arbeit = s == "Kunden" || s == "Verkauf" || s == "Aktivitäten" || s == "Erinnerungen";
                    bool lesen = s != "Benutzer" && s != "Einstellungen";
                    ug.Roles.Add(Role(s, lesen, arbeit, arbeit, false));
                }
                ugbll.Create(ug);
            }
            if (ubll.Readuser(SalesUser) == null)
            {
                ubll.Create(new USER
                {
                    Name = "Max Schulz",
                    UserName = SalesUser,
                    Password = DemoPassword,
                    TC = "D-1002",
                    E_mail = "schulz@schraubwerk.example",
                    PhoneNumber = "0711 000010",
                    Adress = "Königstraße 20, 70173 Stuttgart",
                    Status = "Vertrieb",
                    Pic = Bild("benutzer_schulz.jpg"),
                    Regtime = DateTime.Now.Date.AddMonths(-3)
                }, new USER_GROUP_BLL().getug_bytitle("Vertrieb"), false);
            }
            USER sales = new USER_BLL().Readuser(SalesUser) ?? admin;

            // --- Aktivitätskategorien ---
            ACTİVİTY_CATEGORY_BLL acbll = new ACTİVİTY_CATEGORY_BLL();
            foreach (string k in new[] { "Telefonat", "E-Mail", "Besuch", "Angebot", "Reklamation" })
                if (acbll.Readaccatagory(k) == null) acbll.Create(new ACTİVİTY_CATEGORY { CategoryName = k });

            // --- Kunden ---
            var kunden = new[]
            {
                new { Name = "Thomas Müller",   Firma = "Müller Maschinenbau GmbH", Tel = "0711 1111111", Mail = "einkauf@mueller.example",  Adr = "Industriestraße 12, 70565 Stuttgart",  Ust = "DE000000001", Betreuer = admin, Tage = -60 },
                new { Name = "Sabine Wagner",   Firma = "Wagner Metallbau KG",      Tel = "07031 222222", Mail = "info@wagner.example",      Adr = "Gewerbepark 3, 71034 Böblingen",       Ust = "DE000000002", Betreuer = admin, Tage = -45 },
                new { Name = "Michael Becker",  Firma = "Becker Fahrzeugtechnik",   Tel = "07141 333333", Mail = "becker@becker.example",    Adr = "Hafenstraße 8, 71636 Ludwigsburg",     Ust = "DE000000003", Betreuer = admin, Tage = -30 },
                new { Name = "Julia Schneider", Firma = "Schneider Bau AG",         Tel = "0721 444444",  Mail = "info@schneider.example",   Adr = "Am Rheinhafen 5, 76185 Karlsruhe",     Ust = "DE000000004", Betreuer = admin, Tage = -5 },
                new { Name = "Andreas Fischer", Firma = "Fischer Anlagenbau GmbH",  Tel = "07071 555555", Mail = "info@fischer.example",     Adr = "Wilhelmstraße 40, 72074 Tübingen",     Ust = "DE000000005", Betreuer = sales, Tage = -3 },
                new { Name = "Laura Weber",     Firma = "Weber Holzbau",            Tel = "07121 666666", Mail = "kontakt@weber.example",    Adr = "Lindenweg 2, 72764 Reutlingen",        Ust = "DE000000006", Betreuer = sales, Tage = -1 },
            };
            CUSTOMER_BLL cbll = new CUSTOMER_BLL();
            foreach (var k in kunden)
            {
                if (cbll.Readname(k.Firma) != null) continue;
                cbll.create(new CUSTOMER
                {
                    Name = k.Name, Company = k.Firma, Phone = k.Tel, Email = k.Mail, Adress = k.Adr,
                    vergidairesi_bilgileri = "USt-IdNr. " + k.Ust, Regdate = DateTime.Now.Date.AddDays(k.Tage)
                }, k.Betreuer);
            }

            // --- Produkte mit Bildern ---
            var produkte = new List<PRODUCT>
            {
                Prod("Schraube", "Sechskantschraube", "8", "40", "8.8", "Verzinkt", "DIN 933", 500, 0.35, "sechskantschraube_verzinkt.jpg", "Vollgewinde"),
                Prod("Schraube", "Sechskantschraube", "10", "60", "10.9", "Schwarz", "DIN 933", 300, 0.60, "sechskantschraube_schwarz.jpg", "hochfest"),
                Prod("Schraube", "Sechskantschraube", "6", "30", "A2", "Ohne", "DIN 933", 400, 0.42, "sechskantschraube_a2.jpg", "Edelstahl, rostfrei"),
                Prod("Schraube", "Zylinderschraube", "6", "20", "12.9", "Schwarz", "DIN 912", 800, 0.25, "zylinderschraube.jpg", "Innensechskant"),
                Prod("Mutter", "Sechskantmutter", "8", "-", "8", "Verzinkt", "DIN 934", 1000, 0.08, "sechskantmutter.jpg", ""),
                Prod("Mutter", "Sicherungsmutter", "10", "-", "8", "Verzinkt", "DIN 985", 600, 0.15, "sicherungsmutter.jpg", "mit Polyamidring"),
                Prod("Unterlegscheibe", "Scheibe", "8", "-", "-", "Verzinkt", "DIN 125", 2000, 0.03, "scheibe.jpg", ""),
                Prod("Holz-/Blechschraube", "Spanplattenschraube", "4", "40", "-", "Gelb verzinkt", "DIN 7505", 1500, 0.04, "spanplattenschraube.jpg", "Kreuzschlitz PZ2"),
                Prod("Gewindestange", "Gewindestange", "10", "1000", "4.8", "Verzinkt", "DIN 976", 80, 2.90, "gewindestange.jpg", "1 Meter"),
                Prod("Dübel", "Spreizdübel", "8", "40", "-", "Ohne", "-", 900, 0.06, "duebel.jpg", "Nylon"),
            };
            PRODUCT_BLL pbll = new PRODUCT_BLL();
            foreach (var p in produkte)
            {
                object vorhanden = Scalar("SELECT TOP 1 id FROM dbo.PRODUCTs WHERE DeletStatus = 0 AND SaledPices = 0 AND Category = @c AND Name = @n AND Cap = @cap AND Kaplama = @k",
                    P("@c", p.Category), P("@n", p.Name), P("@cap", p.Cap), P("@k", p.Kaplama));
                if (vorhanden == null) pbll.create(p);
                else Exec("UPDATE dbo.PRODUCTs SET picture = @pic WHERE id = @id AND (picture IS NULL OR picture = '')", P("@pic", p.picture), P("@id", vorhanden));
            }

            // --- Aktivitäten ---
            if (Count("SELECT COUNT(*) FROM dbo.ACTİVİTY WHERE DeletStatus = 0") == 0)
            {
                ACTİVİTY_BLL abll = new ACTİVİTY_BLL();
                Akt(abll, admin, cbll, acbll, "Müller Maschinenbau GmbH", "Telefonat", "Rückfrage Lieferzeit", "Kunde fragt nach Lieferzeit für 2.000 Stk. M8x40.", -6);
                Akt(abll, admin, cbll, acbll, "Wagner Metallbau KG", "Angebot", "Angebot Sicherungsmuttern", "Angebot über 5.000 Stk. DIN 985 M10 versendet.", -4);
                Akt(abll, admin, cbll, acbll, "Becker Fahrzeugtechnik", "Besuch", "Vor-Ort-Termin", "Bedarf für neue Fertigungslinie aufgenommen.", -2);
                Akt(abll, admin, cbll, acbll, "Schneider Bau AG", "E-Mail", "Rahmenvertrag", "Unterlagen zum Rahmenvertrag per E-Mail geschickt.", -1);
                Akt(abll, sales, cbll, acbll, "Fischer Anlagenbau GmbH", "Telefonat", "Erstkontakt", "Interesse an Edelstahlschrauben A2.", -1);
                Akt(abll, sales, cbll, acbll, "Weber Holzbau", "Reklamation", "Falsche Länge geliefert", "Spanplattenschrauben 4x40 statt 4x50 – Ersatz zugesagt.", 0);
            }

            // --- Erinnerungen ---
            REMİNDER_BLL rbll = new REMİNDER_BLL();
            Erinnerung(rbll, admin, "Kundentermin", "Müller Maschinenbau wegen neuer Bestellung anrufen.", 0);
            Erinnerung(rbll, admin, "Angebot nachfassen", "Wagner Metallbau: Rückmeldung zum Angebot einholen.", 0);
            Erinnerung(rbll, admin, "Lager prüfen", "Bestand Gewindestangen M10 prüfen und nachbestellen.", 2);
            Erinnerung(rbll, sales, "Muster versenden", "Weber Holzbau: Ersatzlieferung 4x50 versenden.", 1);

            // --- Rechnungen ---
            if (Count("SELECT COUNT(*) FROM dbo.İNVOİCE") == 0)
            {
                var katalog = pbll.Readall();
                Rechnung(katalog, cbll, "Müller Maschinenbau GmbH", -8, PAYMENT_METHOD.Bar, 1.0,
                    Pos("Sechskantschraube", "8", 400), Pos("Sechskantmutter", "8", 400), Pos("Scheibe", "8", 800));
                Rechnung(katalog, cbll, "Wagner Metallbau KG", -3, PAYMENT_METHOD.Kreditkarte, 0.5,
                    Pos("Sicherungsmutter", "10", 300), Pos("Sechskantschraube", "10", 150));
                Rechnung(katalog, cbll, "Becker Fahrzeugtechnik", -1, PAYMENT_METHOD.Auf_Ziel, 0,
                    Pos("Zylinderschraube", "6", 250), Pos("Gewindestange", "10", 10));
                Rechnung(katalog, cbll, "Weber Holzbau", 0, PAYMENT_METHOD.Bar, 1.0,
                    Pos("Spanplattenschraube", "4", 600), Pos("Spreizdübel", "8", 200));
            }
        }

        static PRODUCT Prod(string cat, string name, string cap, string boy, string quality, string kaplama, string din, int stock, double price, string bild, string merkmal)
        {
            return new PRODUCT
            {
                Category = cat, Name = name, Cap = cap, Boy = boy, Quality = quality, Kaplama = kaplama,
                DINnumber = din, Stock = stock, Price = price, Packing = "100 Stück", BrandName = "SCHRAUBWERK",
                Feature = merkmal, picture = Bild(bild)
            };
        }

        static void Akt(ACTİVİTY_BLL abll, USER u, CUSTOMER_BLL cbll, ACTİVİTY_CATEGORY_BLL acbll, string firma, string kategorie, string titel, string info, int tage)
        {
            CUSTOMER c = cbll.Readname(firma);
            ACTİVİTY_CATEGORY k = acbll.Readaccatagory(kategorie);
            if (c == null || k == null) return;
            abll.Create(new ACTİVİTY { Title = titel, İnfo = info, RegDate = DateTime.Now.AddDays(tage) }, u, c, k);
        }

        static void Erinnerung(REMİNDER_BLL rbll, USER u, string titel, string info, int tage)
        {
            if (Count("SELECT COUNT(*) FROM dbo.REMİNDER WHERE Title = @t AND DeletStatus = 0", P("@t", titel)) > 0) return;
            rbll.Create(new REMİNDER { Title = titel, Reminderİnfo = info, RegDate = DateTime.Now, ReminDate = DateTime.Now.Date.AddDays(tage) }, u);
        }

        static KeyValuePair<string, KeyValuePair<string, int>> Pos(string name, string cap, int menge)
        {
            return new KeyValuePair<string, KeyValuePair<string, int>>(name, new KeyValuePair<string, int>(cap, menge));
        }

        // Rechnung wie im Verkaufsformular anlegen: Positionen = Kopien der Katalogprodukte
        static void Rechnung(List<PRODUCT> katalog, CUSTOMER_BLL cbll, string firma, int tage, PAYMENT_METHOD art, double bezahltAnteil,
                             params KeyValuePair<string, KeyValuePair<string, int>>[] positionen)
        {
            CUSTOMER c = cbll.Readname(firma);
            if (c == null) return;
            USER betreuer = new USER_BLL().Read_By_İd(cbll.User_id_From_Custumer_id(c.id));
            if (betreuer == null) return;

            var zeilen = new List<PRODUCT>();
            double netto = 0;
            foreach (var pos in positionen)
            {
                PRODUCT k = katalog.FirstOrDefault(x => x.Name == pos.Key && x.Cap == pos.Value.Key);
                if (k == null) continue;
                zeilen.Add(new PRODUCT
                {
                    id = k.id, Category = k.Category, Name = k.Name, Cap = k.Cap, Boy = k.Boy, Packing = k.Packing,
                    Quality = k.Quality, Feature = k.Feature, Price = k.Price, Kaplama = k.Kaplama, DINnumber = k.DINnumber,
                    BrandName = k.BrandName, picture = k.picture, SaledPices = pos.Value.Value
                });
                netto += k.Price * pos.Value.Value;
            }
            if (zeilen.Count == 0) return;

            double brutto = Math.Round(netto * 1.2, 2); // wie im Verkaufsformular: 20 % MwSt.
            double bezahlt = Math.Round(brutto * bezahltAnteil, 2);
            DateTime datum = DateTime.Now.Date.AddDays(tage);
            var i = new İNVOİCE
            {
                RegDate = datum,
                TotalPrice = brutto,
                VadeDate = datum.AddDays(14),
                Bakiye = brutto - bezahlt
            };
            if (bezahlt > 0)
            {
                i.ödemeŞekli = art;
                i.ÖdemeDate = datum;
                i.ÖdemeTurarı = bezahlt;
                i.İsCheckedOut = bezahlt >= brutto;
                if (i.İsCheckedOut) i.CeackOutDate = datum;
            }

            string res = new İNVOCE_BLL().Create(i, c, zeilen, betreuer);
            if (!res.StartsWith("Bestellung")) return;
            cbll.CreateBakiye(c, brutto);
            if (bezahlt > 0) cbll.Create_payment(c, bezahlt);
        }
    }

    public partial class App
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            if (DemoSetup.Enabled)
            {
                try
                {
                    DemoSetup.EnsureReady();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        "Die Demo-Datenbank konnte nicht vorbereitet werden.\n" +
                        "Bitte prüfen, ob SQL Server LocalDB installiert ist.\n\n" + ex.Message,
                        "CRM Demo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            base.OnStartup(e);
        }
    }
}
