// -----------------------------------------------------------------------------
// Kleine Hilfen für die Tabellen (DataGridView)
// Die Suche (gespeicherte Prozeduren) und die normale Liste liefern teilweise
// unterschiedliche Spaltennamen (z. B. "İd" und "id"). Direkter Zugriff auf eine
// fehlende Spalte führte zum Absturz – diese Hilfen prüfen das vorher.
// -----------------------------------------------------------------------------
using System;
using System.Windows.Forms;

namespace çağdaşcivata
{
    public static class GridHelper
    {
        /// <summary>Blendet die genannten Spalten aus, sofern vorhanden.</summary>
        public static void Hide(DataGridView g, params string[] columns)
        {
            foreach (string c in columns)
                if (g.Columns.Contains(c)) g.Columns[c].Visible = false;
        }

        /// <summary>Liest einen Zahlenwert aus der angeklickten Zeile (erste vorhandene Spalte). 0 = nichts gefunden.</summary>
        public static int GetInt(DataGridView g, int rowIndex, params string[] columns)
        {
            if (rowIndex < 0 || rowIndex >= g.Rows.Count) return 0;
            foreach (string c in columns)
            {
                if (!g.Columns.Contains(c)) continue;
                object v = g.Rows[rowIndex].Cells[c].Value;
                if (v == null || v == DBNull.Value) return 0;
                int r;
                return int.TryParse(Convert.ToString(v), out r) ? r : 0;
            }
            return 0;
        }

        /// <summary>Liest einen Text aus der angeklickten Zeile (erste vorhandene Spalte).</summary>
        public static string GetText(DataGridView g, int rowIndex, params string[] columns)
        {
            if (rowIndex < 0 || rowIndex >= g.Rows.Count) return "";
            foreach (string c in columns)
            {
                if (!g.Columns.Contains(c)) continue;
                object v = g.Rows[rowIndex].Cells[c].Value;
                return v == null || v == DBNull.Value ? "" : Convert.ToString(v);
            }
            return "";
        }
    }
}
