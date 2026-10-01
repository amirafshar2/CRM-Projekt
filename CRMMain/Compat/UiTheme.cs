// -----------------------------------------------------------------------------
// Einheitliches Erscheinungsbild für alle Windows-Forms-Fenster.
// Wird in jedem Formular direkt nach InitializeComponent() aufgerufen und ändert
// nur die Optik (Schrift, Tabellen, Schaltflächen, Eingabefelder) – keine Logik.
// -----------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace çağdaşcivata
{
    /// <summary>Einstiegspunkt für die Formulare: UiTheme.Apply(this)</summary>
    public static class UiTheme
    {
        public static void Apply(System.Windows.Forms.Control root) { SchraubwerkTheme.Styler.Apply(root); }
    }
}

// Eigener Namespace, damit Typnamen wie "Button" nicht mit gleichnamigen Typen
// des Projekts (z. B. dem Button-Enum der Meldungsbox) kollidieren.
namespace SchraubwerkTheme
{
    public static class Styler
    {
        public static readonly Color Navy = Color.FromArgb(16, 37, 66);       // wie Hauptfenster
        public static readonly Color NavyLight = Color.FromArgb(52, 93, 153);
        public static readonly Color RowAlt = Color.FromArgb(235, 242, 250);   // wie Hauptfenster
        public static readonly Color GridLine = Color.FromArgb(222, 226, 230);
        const string FontName = "Segoe UI";

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, object> Done =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Control, object>();

        public static void Apply(Control root)
        {
            if (root == null) return;
            Run(root);
            // Manche Formulare setzen im Load-Ereignis eigene Farben -> danach nochmals anwenden
            var form = root as Form;
            if (form != null) form.Load += (s, e) => Run(root);
            var uc = root as UserControl;
            if (uc != null) uc.Load += (s, e) => Run(root);
        }

        static void Run(Control root)
        {
            root.SuspendLayout();
            try { Walk(root); }
            finally { root.ResumeLayout(true); }
        }

        public static readonly Color Background = Color.FromArgb(235, 242, 250); // helles Blau-Grau wie Hauptfenster
        public static readonly Color TextDark = Color.FromArgb(16, 37, 66);

        static void Walk(Control c)
        {
            FixFont(c);
            FixColors(c);

            var grid = c as DataGridView;
            if (grid != null)
            {
                object tmp;
                if (Done.TryGetValue(grid, out tmp)) ApplyGridStyle(grid);
                else { Done.Add(grid, true); StyleGrid(grid); }
            }

            var button = c as Button;
            if (button != null) StyleButton(button);

            var text = c as TextBox;
            if (text != null)
            {
                // Rahmenlose Felder bleiben rahmenlos (sonst wachsen sie und überlappen sich)
                if (text.BorderStyle == BorderStyle.Fixed3D) text.BorderStyle = BorderStyle.FixedSingle;
                if (!text.ReadOnly) WhiteField(text);
            }
            var rich = c as RichTextBox;
            if (rich != null) { if (rich.BorderStyle == BorderStyle.Fixed3D) rich.BorderStyle = BorderStyle.FixedSingle; if (!rich.ReadOnly) WhiteField(rich); }
            var masked = c as MaskedTextBox;
            if (masked != null) { if (masked.BorderStyle == BorderStyle.Fixed3D) masked.BorderStyle = BorderStyle.FixedSingle; WhiteField(masked); }

            var combo = c as ComboBox;
            if (combo != null) WhiteField(combo); // Standardrahmen bleibt, damit Felder sichtbar getrennt sind

            // Rechtsbündige Beschriftungen (einzeilig) dürfen nicht abgeschnitten werden:
            // reicht die Breite nicht, wächst das Label nach links – die rechte Kante bleibt
            var lbl = c as Label;
            if (lbl != null && !lbl.AutoSize && lbl.TextAlign == ContentAlignment.MiddleRight
                && lbl.Text.Length > 0 && lbl.Height < lbl.Font.Height * 2)
            {
                int need = lbl.PreferredWidth;
                if (need > lbl.Width)
                {
                    int r = lbl.Right;
                    lbl.Width = need;
                    lbl.Left = Math.Max(0, r - need);
                }
            }

            // Langes deutsches Datum ("Donnerstag, 1. Oktober 2026") passt nicht ins Feld -> kurzes Format
            var dtp = c as DateTimePicker;
            if (dtp != null && dtp.Format == DateTimePickerFormat.Long) dtp.Format = DateTimePickerFormat.Short;

            foreach (Control child in c.Controls) Walk(child);
        }

        // Weißes Eingabefeld – Schrift muss dunkel genug sein (sonst weiß auf weiß)
        static void WhiteField(Control c)
        {
            c.BackColor = Color.White;
            if (Contrast(c.ForeColor, Color.White) < 3.0) c.ForeColor = TextDark;
        }

        // Alle Schriften auf Segoe UI vereinheitlichen (Größe und Stil bleiben)
        static void FixFont(Control c)
        {
            // Formulare/UserControls selbst nicht ändern: eine neue Schrift würde dort das
            // automatische Skalieren (AutoScaleMode.Font) auslösen und das Layout verschieben.
            if (!(c is Button)) return; // nur Schaltflächen; Beschriftungen/Felder behalten ihre Schrift und damit ihr Layout
            if (c is ContainerControl) return;
            // Eingabefelder behalten ihre Schrift: Segoe UI ist höher und Felder würden sich überlappen
            if (c is TextBoxBase || c is ComboBox || c is DateTimePicker || c is NumericUpDown || c is ListBox) return;
            Font f = c.Font;
            if (f == null || f.Name.StartsWith(FontName, StringComparison.OrdinalIgnoreCase)) return;
            float size = f.Size;
            if (f.Name.IndexOf("Narrow", StringComparison.OrdinalIgnoreCase) >= 0) size *= 0.9f; // schmale Schrift ausgleichen
            try { c.Font = new Font(FontName, size, f.Style, f.Unit); } catch { /* Schrift bleibt */ }
        }

        // Uneinheitliche Grautöne -> zwei Flächenfarben (hell bzw. dunkel wie das Hauptfenster)
        static readonly int[] LightGrays =
        {
            Color.FromArgb(173, 181, 189).ToArgb(), Color.FromArgb(206, 212, 218).ToArgb(), Color.Silver.ToArgb(),
            Color.DarkGray.ToArgb(), SystemColors.AppWorkspace.ToArgb(), SystemColors.Control.ToArgb(), Color.Gainsboro.ToArgb()
        };
        static readonly int[] DarkGrays =
        {
            Color.FromArgb(108, 117, 125).ToArgb(), Color.Gray.ToArgb(), Color.DimGray.ToArgb()
        };

        static void FixColors(Control c)
        {
            if (c is TextBoxBase || c is ComboBox || c is DataGridView || c is Button || c is ListControl || c is DateTimePicker || c is NumericUpDown)
                return;

            if (c.BackColor != Color.Transparent)
            {
                int argb = c.BackColor.ToArgb();
                if (Array.IndexOf(LightGrays, argb) >= 0) c.BackColor = Background;
                else if (Array.IndexOf(DarkGrays, argb) >= 0) c.BackColor = Navy;
            }

            // Lesbarkeit: Text muss sich deutlich vom Hintergrund abheben
            Color fore = c.ForeColor;
            if (IsColorful(fore)) return; // z. B. rote Pflichtfeld-Sterne bleiben
            Color back = EffectiveBack(c);
            if (Contrast(fore, back) < 3.0)
                c.ForeColor = Luma(back) > 0.5 ? TextDark : Color.White;
            if (c is GroupBox && Luma(back) > 0.5) c.ForeColor = TextDark;
        }

        static Color EffectiveBack(Control c)
        {
            Control p = c;
            while (p != null && (p.BackColor == Color.Transparent || p.BackColor.A < 255)) p = p.Parent;
            return p == null ? Background : p.BackColor;
        }

        static bool IsColorful(Color c)
        {
            int max = Math.Max(c.R, Math.Max(c.G, c.B)), min = Math.Min(c.R, Math.Min(c.G, c.B));
            return max - min > 60;
        }

        static double Luma(Color c)
        {
            Func<double, double> lin = v => { v /= 255.0; return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4); };
            return 0.2126 * lin(c.R) + 0.7152 * lin(c.G) + 0.0722 * lin(c.B);
        }

        static double Contrast(Color a, Color b)
        {
            double la = Luma(a), lb = Luma(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        // Spaltenüberschriften aus der Datenbank (SQL-Aliasse) für die Anzeige übersetzen.
        // Nur HeaderText ändert sich – die Spaltennamen im Code bleiben unverändert.
        static readonly string[,] HeaderPairs =
        {
            {"İsim", "Name"},
            {"Firma", "Firma"},
            {"Telefon No", "Telefon"},
            {"Telefon NO", "Telefon"},
            {"Tel No", "Telefon"},
            {"Email Adres", "E-Mail"},
            {"E-mail Adresi", "E-Mail"},
            {"email", "E-Mail"},
            {"Kayıt Tarihi", "Angelegt am"},
            {"Kayıt tarihi", "Angelegt am"},
            {"Kayit tarihi", "Angelegt am"},
            {"Adres", "Adresse"},
            {"Açıklama", "Beschreibung"},
            {"Başlık", "Titel"},
            {"Fiyat", "Preis"},
            {"Görevli", "Zuständig"},
            {"Görsel", "Bild"},
            {"Kalite", "Festigkeit"},
            {"Konu", "Betreff"},
            {"Marka", "Marke"},
            {"Müşteri", "Kunde"},
            {"Paket", "Packung"},
            {"Stok", "Bestand"},
            {"Temsilci", "Betreuer"},
            {"Yetkili", "Ansprechpartner"},
            {"Fatura No", "Rechnungs-Nr."},
            {"Fiş Numarası", "Beleg-Nr."},
            {"Fiş numarası", "Beleg-Nr."},
            {"Hatırlatma Konusu", "Betreff"},
            {"Hatırlatma Tarihi", "Erinnerung am"},
            {"Hatırlatıcı Açıklaması", "Beschreibung"},
            {"Muşteri Firması", "Kundenfirma"},
            {"Müşteri Adı", "Kunde"},
            {"Müşteri Temsilcisi", "Betreuer"},
            {"Müşteri isim", "Kunde"},
            {"Müştri Temsilcisi", "Betreuer"},
            {"S.Tarih", "Bestelldatum"},
            {"Satiş Temsilcisi", "Verkäufer"},
            {"Sipariş Tarihi", "Bestelldatum"},
            {"T.Tutar", "Gesamt"},
            {"Toplam Tutar", "Gesamtbetrag"},
            {"Vade.Tarihi", "Fällig am"},
            {"Yapıldı mı?", "Erledigt?"},
            {"Yetki Ünvanı", "Rolle"},
            {"Yetkili İsmi", "Ansprechpartner"},
            {"bu Faturadan Kalan Bakiye", "Offener Betrag"},
            {"konnu Başlığı", "Kategorie"},
            {"CategoryName", "Kategorie"},
            {"Ö.Tarih", "Zahlungsdatum"},
            {"Ö.Tarihi", "Zahlungsdatum"},
            {"Ö.Tutar", "Gezahlt"},
            {"Ö.Şekli", "Zahlungsart"},
            {"Ödeme Tarihi", "Zahlungsdatum"},
            {"Ürün Adı", "Produktname"},
            {"Ürün", "Produkt"},
            {"Çap", "Gewinde"},
            {"Boy", "Länge"},
            {"Kaplama", "Überzug"},
            {"Özellik", "Merkmal"},
            {"DIN", "DIN"},
            {"Bakiye", "Saldo"},
            {"Alacak", "Forderung"}
        };
        static readonly System.Collections.Generic.Dictionary<string, string> Headers = BuildHeaders();
        static System.Collections.Generic.Dictionary<string, string> BuildHeaders()
        {
            var d = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < HeaderPairs.GetLength(0); i++) d[HeaderPairs[i, 0]] = HeaderPairs[i, 1];
            return d;
        }

        static void StyleGrid(DataGridView g)
        {
            ApplyGridStyle(g);
            // Formulare setzen nach dem Laden der Daten teils eigene Farben -> danach erneut anwenden
            g.DataBindingComplete += (s, e) =>
            {
                if (g.IsHandleCreated) g.BeginInvoke(new Action(() => ApplyGridStyle(g)));
                else ApplyGridStyle(g);
            };
            g.HandleCreated += (s, e) => g.BeginInvoke(new Action(() => ApplyGridStyle(g)));
        }

        static void ApplyGridStyle(DataGridView g)
        {
            if (g.IsDisposed) return;
            foreach (DataGridViewColumn col in g.Columns)
            {
                string de;
                if (col.HeaderText != null && Headers.TryGetValue(col.HeaderText.Trim(), out de)) col.HeaderText = de;
                // Beträge mit 2 Nachkommastellen, Datum ohne Uhrzeit
                Type t = col.ValueType;
                if (t == typeof(double) || t == typeof(decimal) || t == typeof(float) || t == typeof(double?) || t == typeof(decimal?))
                {
                    col.DefaultCellStyle.Format = "N2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (t == typeof(DateTime) || t == typeof(DateTime?))
                {
                    col.DefaultCellStyle.Format = "dd.MM.yyyy";
                }
            }
            // Listen dienen nur zur Anzeige/Auswahl – Bearbeitung erfolgt über die Formulare
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            EnableDoubleBuffer(g); // flüssigeres Scrollen, kein Flackern
            g.BorderStyle = BorderStyle.None;
            g.BackgroundColor = Color.White;
            g.GridColor = GridLine;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 34;

            var h = g.ColumnHeadersDefaultCellStyle;
            h.BackColor = Navy; h.ForeColor = Color.White;
            h.SelectionBackColor = Navy; h.SelectionForeColor = Color.White;
            h.Font = new Font(FontName, 9.75f, FontStyle.Bold);
            h.Padding = new Padding(6, 0, 0, 0);

            var d = g.DefaultCellStyle;
            d.Font = new Font(FontName, 9.75f);
            d.BackColor = Color.White; d.ForeColor = Color.FromArgb(33, 37, 41);
            d.SelectionBackColor = NavyLight; d.SelectionForeColor = Color.White;
            d.Padding = new Padding(6, 0, 0, 0);

            // Von Formularen gesetzte Zeilenfarben zurücksetzen (sonst weiße Schrift auf weißem Grund)
            g.RowsDefaultCellStyle.BackColor = Color.White;
            g.RowsDefaultCellStyle.ForeColor = d.ForeColor;
            g.AlternatingRowsDefaultCellStyle.ForeColor = d.ForeColor;
            g.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
            g.RowTemplate.Height = 30;
        }

        static void StyleButton(Button b)
        {
            // Bildschaltflächen (Icons) bleiben unverändert
            if (b.Image != null || b.BackgroundImage != null) return;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            bool darkBack = b.Parent != null && Luma(EffectiveBack(b.Parent)) < 0.2;
            if (darkBack)
            {
                // auf dunklem Grund (z. B. Anmeldung): helle Schaltfläche
                b.BackColor = Color.White; b.ForeColor = Navy;
                b.FlatAppearance.MouseOverBackColor = RowAlt; b.FlatAppearance.MouseDownBackColor = GridLine;
            }
            else
            {
                b.BackColor = Navy; b.ForeColor = Color.White;
                b.FlatAppearance.MouseOverBackColor = NavyLight; b.FlatAppearance.MouseDownBackColor = Navy;
            }
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;

            // Deaktivierte Schaltflächen hell darstellen (sonst graue Schrift auf Dunkelblau)
            Color normal = b.BackColor;
            object dummy;
            if (ButtonHooked.TryGetValue(b, out dummy)) ButtonHooked.Remove(b);
            else b.EnabledChanged += (s, e) => ShowEnabled(b);
            ButtonHooked.Add(b, normal);
            ShowEnabled(b);
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Button, object> ButtonHooked =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Button, object>();

        static void ShowEnabled(Button b)
        {
            object normal;
            if (!ButtonHooked.TryGetValue(b, out normal)) return;
            b.BackColor = b.Enabled ? (Color)normal : Color.FromArgb(206, 212, 218);
        }

        static void EnableDoubleBuffer(Control c)
        {
            PropertyInfo p = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (p != null) p.SetValue(c, true, null);
        }
    }
}
