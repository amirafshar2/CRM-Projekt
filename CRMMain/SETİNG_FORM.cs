using BE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;

namespace çağdaşcivata
{
    public partial class SETİNG_FORM : Form
    {
        ACTİVİTY_CATEGORY_BLL bll = new ACTİVİTY_CATEGORY_BLL();
        MESSAGE_BOX message = new MESSAGE_BOX();
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect, // sol üst köşenin x koordinatı
        int nTopRect, // sol üst köşenin y kordinatı
        int nRightRect, // sağ alt köşenin x kordinatı
         int nBottomRect, // sağ alt köşenin y kordinatı
        int nWidthEllipse, // height of ellipse
         int nHeightEllipse // elipsin genişliği
         );
        public SETİNG_FORM()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            datagridviewsetting(dataGridView1);
        }
        int id;

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            id = GridHelper.GetInt(dataGridView1, e.RowIndex, "id");
         

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            if (name == string.Empty)
            {
                label1.Text = "Bitte einen Namen eingeben.";
                timer1.Start();
                return;
            }
            if (bll.Readkatrgoryname().Contains(name))
            {
                label1.Text = "Diese Kategorie existiert bereits.";
                timer1.Start();
                return;
            }
            ACTİVİTY_CATEGORY a = new ACTİVİTY_CATEGORY();
            a.CategoryName = name;
            label1.Text = bll.Create(a);
            timer1.Start();
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = bll.Read_All();
            GridHelper.Hide(dataGridView1, "id", "DeletStatus", "activities");
            textBox1.Text = string.Empty;
            textBox1.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = string.Empty;
            timer1.Stop();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DialogResult i = message.Show("Löschen", "Wirklich löschen?", "", Button.yesorno, Logo.warning);

            if (i == DialogResult.Yes)
            {
                if (id != 0)
                {
                    label1.Text = bll.Delete(id);
                    id = 0;
                }
                else { label1.Text = "Bitte einen Eintrag auswählen."; }
                timer1.Start();
            }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = bll.Read_All();
            GridHelper.Hide(dataGridView1, "id", "DeletStatus", "activities");

        }
        public void datagridviewsetting(DataGridView d)
        {
            d.DefaultCellStyle.BackColor = Color.FromArgb(108, 117, 125);
            d.RowHeadersVisible = false;
            d.BackgroundColor = Color.FromArgb(108, 117, 125);
            d.BorderStyle = BorderStyle.None;
            d.DefaultCellStyle.SelectionForeColor = Color.White;
            d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(73, 80, 87);
            d.EnableHeadersVisualStyles = false;
            d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            d.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(108, 117, 125);
            d.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SETİNG_FORM_Load(object sender, EventArgs e)
        {
            HelpHint.Set(this, "Aktivitätskategorie eingeben → Speichern.  Zum Löschen eine Kategorie in der Liste anklicken.");
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = bll.Read_All();
            GridHelper.Hide(dataGridView1, "id", "DeletStatus", "activities");

        }
    }
}
