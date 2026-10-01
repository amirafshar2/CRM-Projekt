// -----------------------------------------------------------------------------
// Schlanker Ersatz für "Stimulsoft Reports" (kommerziell, nicht verfügbar).
// Gleiche Schnittstelle wie bisher (StiReport, Dictionary.Variables,
// RegBusinessObject, Render, Show), damit der Code im Rechnungsformular
// unverändert bleibt. Die Rechnung wird als HTML-Seite erzeugt und im
// Standardbrowser geöffnet – von dort kann sie gedruckt oder als PDF
// gespeichert werden.
// -----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace Stimulsoft
{
    internal static class StiInfo
    {
        public const string Description = "Eigener Rechnungsdruck (HTML) anstelle von Stimulsoft Reports";
    }
}

namespace Stimulsoft.Report
{
    public class StiVariable
    {
        public object Value { get; set; }
    }

    public class StiVariablesCollection
    {
        private readonly Dictionary<string, StiVariable> _items =
            new Dictionary<string, StiVariable>(StringComparer.OrdinalIgnoreCase);

        public StiVariable this[string name]
        {
            get
            {
                StiVariable v;
                if (!_items.TryGetValue(name, out v))
                {
                    v = new StiVariable();
                    _items[name] = v;
                }
                return v;
            }
        }

        internal string Text(string name)
        {
            StiVariable v;
            return _items.TryGetValue(name, out v) && v.Value != null ? Convert.ToString(v.Value, CultureInfo.CurrentCulture) : "";
        }
    }

    public class StiDictionary
    {
        public StiDictionary() { Variables = new StiVariablesCollection(); }
        public StiVariablesCollection Variables { get; private set; }
    }

    public class StiReport
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        private string _html;

        public StiReport() { Dictionary = new StiDictionary(); }

        public StiDictionary Dictionary { get; private set; }

        /// <summary>Früher: Laden der .mrt-Vorlage. Die Vorlage ist jetzt fest im Code.</summary>
        public void Load(string path) { }

        public void RegBusinessObject(string name, object data) { _data[name] = data; }

        public void Render() { _html = BuildHtml(); }

        public void Show()
        {
            if (_html == null) Render();
            string file = Path.Combine(Path.GetTempPath(), "Rechnung_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html");
            File.WriteAllText(file, _html, Encoding.UTF8);
            Process.Start(file);
        }

        private string V(string name) { return WebUtility.HtmlEncode(Dictionary.Variables.Text(name)); }

        private string BuildHtml()
        {
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>Rechnung ").Append(V("İnvoiceNumber")).Append("</title>");
            sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{margin:0 0 4px}" +
                      ".row{display:flex;justify-content:space-between;gap:40px;margin:24px 0}.box{white-space:pre-line}" +
                      "table{width:100%;border-collapse:collapse;margin-top:16px}th,td{border:1px solid #ccc;padding:6px 8px;text-align:left}" +
                      "th{background:#f2f2f2}td.n{text-align:right}tfoot td{font-weight:bold}" +
                      "@media print{button{display:none}body{margin:10mm}}</style></head><body>");
            sb.Append("<button onclick='window.print()' style='float:right;padding:8px 16px'>Drucken</button>");
            sb.Append("<h1>").Append(V("Company")).Append("</h1><div class='box'>").Append(V("Adress")).Append("</div>");
            sb.Append("<div class='row'><div class='box'><b>Kunde</b>\n").Append(V("CustomerCompany")).Append('\n')
              .Append(V("CustomerName")).Append('\n').Append(V("CustomerAdress")).Append('\n').Append(V("CustomerPhone"))
              .Append("</div><div class='box'><b>Rechnungs-Nr.:</b> ").Append(V("İnvoiceNumber"))
              .Append("\n<b>Datum:</b> ").Append(V("Date"))
              .Append("\n<b>Bearbeiter:</b> ").Append(V("NameUser"))
              .Append("\n<b>Telefon:</b> ").Append(V("Phone")).Append("</div></div>");

            // Gleiche Spalten wie im Warenkorb des Rechnungsformulars
            string[] cols = { "Name", "Cap", "Boy", "Quality", "Kaplama", "SaledPices", "Price" };
            string[] heads = { "Produkt", "Ø", "Länge", "Festigkeit", "Beschichtung", "Menge", "Preis/Stk." };
            sb.Append("<table><thead><tr>");
            foreach (string h in heads) sb.Append("<th>").Append(h).Append("</th>");
            sb.Append("<th>Betrag</th></tr></thead><tbody>");

            double total = 0;
            object list;
            if (_data.TryGetValue("PRODUCT", out list) && list is IEnumerable)
            {
                foreach (object item in (IEnumerable)list)
                {
                    sb.Append("<tr>");
                    foreach (string c in cols)
                    {
                        object val = Prop(item, c);
                        bool num = val is int || val is double || val is decimal;
                        sb.Append(num ? "<td class='n'>" : "<td>")
                          .Append(WebUtility.HtmlEncode(val is double ? ((double)val).ToString("N2") : Convert.ToString(val)))
                          .Append("</td>");
                    }
                    double line = ToDouble(Prop(item, "Price")) * ToDouble(Prop(item, "SaledPices"));
                    total += line;
                    sb.Append("<td class='n'>").Append(line.ToString("N2")).Append("</td></tr>");
                }
            }
            sb.Append("</tbody><tfoot><tr><td colspan='").Append(cols.Length).Append("'>Summe netto</td><td class='n'>")
              .Append(total.ToString("N2")).Append("</td></tr></tfoot></table></body></html>");
            return sb.ToString();
        }

        private static object Prop(object item, string name)
        {
            if (item == null) return null;
            PropertyInfo p = item.GetType().GetProperty(name);
            return p == null ? null : p.GetValue(item, null);
        }

        private static double ToDouble(object o)
        {
            if (o == null) return 0;
            try { return Convert.ToDouble(o, CultureInfo.CurrentCulture); } catch { return 0; }
        }
    }
}
