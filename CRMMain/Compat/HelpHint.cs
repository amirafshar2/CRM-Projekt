// -----------------------------------------------------------------------------
// Bedienhinweise
// Zeigt in der unteren Leiste eines Formulars (neben „Zurück“) einen kurzen Text,
// was als Nächstes zu tun ist. Beim Verkauf wechselt der Text mit jedem Schritt.
// Der vollständige Text erscheint zusätzlich als Tooltip.
// -----------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace çağdaşcivata
{
    public static class HelpHint
    {
        const string HintName = "hilfeHinweis";
        static readonly ToolTip Tip = new ToolTip { AutoPopDelay = 20000, InitialDelay = 300 };

        /// <summary>Hinweis neben dem ersten „Zurück“-Label des Formulars anzeigen.</summary>
        public static void Set(Control form, string text)
        {
            Label back = FindBack(form);
            if (back != null) Set(back, text);
        }

        /// <summary>Hinweis neben einem bestimmten „Zurück“-Label anzeigen.</summary>
        public static void Set(Label back, string text)
        {
            Control parent = back.Parent;
            if (parent == null) return;

            Label hint = parent.Controls.OfType<Label>().FirstOrDefault(l => l.Name == HintName);
            if (hint == null)
            {
                hint = new Label
                {
                    Name = HintName,
                    AutoSize = false,
                    AutoEllipsis = true,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(52, 93, 153),
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic)
                };
                parent.Controls.Add(hint);
                hint.BringToFront();
            }

            // Platz zwischen „Zurück“ und dem nächsten Element rechts daneben (z. B. „Anzahl …“)
            int left = back.Right + 20;
            int right = parent.ClientSize.Width - 10;
            // rechte Grenze: die Anzeige „Anzahl …“ bzw. der Pflichtfeld-Hinweis in derselben Zeile
            foreach (Control c in parent.Controls)
            {
                Label l = c as Label;
                if (l == null || l == hint || l == back || !l.Visible) continue;
                bool sameRow = l.Top < back.Bottom + 6 && l.Bottom > back.Top - 6;
                string t = l.Text.Trim();
                if (sameRow && l.Left > back.Right && (t.StartsWith("Anzahl") || t.StartsWith("Mit (*)")) && l.Left - 16 < right)
                    right = l.Left - 16;
            }
            int height = Math.Max(back.Height, 22);
            hint.SetBounds(left, back.Top + (back.Height - height) / 2, Math.Max(80, right - left), height);
            hint.Text = text;
            Tip.SetToolTip(hint, text);
        }

        static Label FindBack(Control root)
        {
            foreach (Control c in root.Controls)
            {
                Label l = c as Label;
                if (l != null && l.Text.Trim() == "Zurück") return l;
                Label inner = FindBack(c);
                if (inner != null) return inner;
            }
            return null;
        }
    }
}
