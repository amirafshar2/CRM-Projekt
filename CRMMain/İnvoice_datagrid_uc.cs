using BE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace çağdaşcivata
{
    public partial class İnvoice_datagrid_uc : UserControl
    {
        public İnvoice_datagrid_uc()
        {
            InitializeComponent();
            UiTheme.Apply(this);
        }
        string pp(string p, int a)
        {
            // Packung kann Text enthalten ("100 Stück") -> nur die Zahl verwenden
            var m = System.Text.RegularExpressions.Regex.Match(p ?? "", @"\d+");
            int n;
            return m.Success && int.TryParse(m.Value, out n) ? (n * a).ToString() : "";
        }
        private void İnvoice_datagrid_uc_Load(object sender, EventArgs e)
        {
           
        }

        private void İnvoice_datagrid_uc_Click(object sender, EventArgs e)
        {
            İNVOİCE_FORM2 w = Application.OpenForms.OfType<İNVOİCE_FORM2>().FirstOrDefault();
            w.id_product =Convert.ToInt32 (idlbl.Text);

        }

        private void İnvoice_datagrid_uc_MouseMove(object sender, MouseEventArgs e)
        {
            this.BackColor = Color.FromArgb(233, 236, 239);
            panel1.BackColor = Color.FromArgb(233, 236, 239);
        }

        private void İnvoice_datagrid_uc_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(206, 212, 218);
            panel1.BackColor = Color.FromArgb(206, 212, 218);
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            button1.BackColor = Color.FromArgb(252, 163, 17);
            this.BackColor = Color.FromArgb(233, 236, 239);
            panel1.BackColor = Color.FromArgb(233, 236, 239);
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(173, 181, 189);
            this.BackColor = Color.FromArgb(206, 212, 218);
            panel1.BackColor = Color.FromArgb(206, 212, 218);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            this.BackColor = Color.FromArgb(233, 236, 239);
            panel1.BackColor = Color.FromArgb(233, 236, 239);
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(206, 212, 218);
            panel1.BackColor = Color.FromArgb(206, 212, 218);
        }      

        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(206, 212, 218);
            panel1.BackColor = Color.FromArgb(206, 212, 218);
        }

        private void textBox1_MouseMove(object sender, MouseEventArgs e)
        {
            this.BackColor = Color.FromArgb(233, 236, 239);
            panel1.BackColor = Color.FromArgb(233, 236, 239);

            if (textBox1.AutoCompleteCustomSource != null && textBox1.AutoCompleteCustomSource.Count > 0) return; // nur einmal aufbauen
            if (pp(paketlbl.Text, 1) == "") return;
            int a = 1;
            List<string> names = new List<string>();

            while (a < 10)
            {
                names.Add(a.ToString() + " Verpackung " + pp(paketlbl.Text, a));
                a++;
            }

            AutoCompleteStringCollection nameCollection = new AutoCompleteStringCollection();
            nameCollection.AddRange(names.ToArray());

            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox1.AutoCompleteCustomSource = nameCollection;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            İNVOİCE_FORM2 w = Application.OpenForms.OfType<İNVOİCE_FORM2>().FirstOrDefault();
            w.id_product = Convert.ToInt32 (idlbl.Text);
            // Stückzahl aus dem Feld lesen (z. B. "250" oder "2 Verpackung 200" -> letzte Zahl)
            var zahlen = System.Text.RegularExpressions.Regex.Matches(textBox1.Text ?? "", @"\d+");
            w.urunadet.Text = zahlen.Count > 0 ? zahlen[zahlen.Count - 1].Value : "";
            w.UrunSearchtxt.Enabled = false;
            w.urunadet.Enabled = true;
            w.checkBoxfiyat.Enabled = true;            
            w.UrunSearchtxt.Text = Hederlbl.Text;
            w.Take_produt(Convert.ToInt32(idlbl.Text));
            w.button2.Enabled = true;
            w.ürün_eklebutonu.Enabled = true;
            w.datagrid_fill_reeadall_product();
            w.paneldata.Enabled = false;
            w.fiyattxt.Text = pricelbl.Text;
            w.label34.Text = pricelbl.Text;
            if (w.urunadet.Text == "") w.urunadet.Focus(); // Menge fehlt -> im Warenkorb eingeben
            
          
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
       
    }
}
