using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace çağdaşcivata
{
    public partial class ACTİVİTY_FORM : Form
    {
        #region form drawing
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
        #endregion
        public ACTİVİTY_FORM()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            // Menüpunkt "Bearbeiten" für Aktivitäten
            ToolStripMenuItem edit = new ToolStripMenuItem("Bearbeiten");
            edit.Click += bearbeitenToolStripMenuItem_Click;
            contextMenuStrip1.Items.Insert(0, edit);
        }

        private void ACTİVİTY_FORM_Load(object sender, EventArgs e)
        {
            HelpHint.Set(this, "Kunde, Benutzer, Kategorie je mit „+“ bestätigen → Titel → speichern.  Eintrag anklicken: Bearbeiten.");
            #region autocomlete
            AutoCompleteStringCollection cnames = new AutoCompleteStringCollection();
            foreach (var item in cbll.Readname())
            {
                cnames.Add(item);

            }
            textBox1.AutoCompleteCustomSource = cnames;
            AutoCompleteStringCollection unames = new AutoCompleteStringCollection();
            foreach (var item in ubll.Readusername())
            {
                unames.Add(item);
            }
            textBox2.AutoCompleteCustomSource = unames;
            AutoCompleteStringCollection aknames = new AutoCompleteStringCollection();
            foreach (var item in acbll.Readkatrgoryname())
            {
                aknames.Add(item);
            }
            textBox3.AutoCompleteCustomSource = aknames;
            #endregion
            datagrid_refresh();
            datagridviewsetting(dataGridView1);
            MainWindow w = (MainWindow)System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault();
            userlogin = w.userlogin;
            false_controls();
            label6.Text = bll.Activitypics();
           
        }

        #region copy
        USER userlogin = new USER();
        CUSTOMER c = new CUSTOMER();
        USER u = new USER();
        ACTİVİTY_CATEGORY ac = new ACTİVİTY_CATEGORY();
        ACTİVİTY_CATEGORY_BLL acbll = new ACTİVİTY_CATEGORY_BLL();
        ACTİVİTY_BLL bll = new ACTİVİTY_BLL();
        REMİNDER_BLL rbll = new REMİNDER_BLL();
        USER_BLL ubll = new USER_BLL();
        CUSTOMER_BLL cbll = new CUSTOMER_BLL();
        MESSAGE_BOX message = new MESSAGE_BOX();
        #endregion
        int id;
        int uid;
        int editId; // != 0 -> Aktivität wird bearbeitet
        #region method
        void false_controls()
        {
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            radioButton1.Enabled = false;
            dateTimePicker1.Enabled = false;
            richTextBox1.Enabled = false;

        }
        void text_take(ACTİVİTY a)
        {
            a.Title = textBox4.Text;
            a.İnfo = richTextBox1.Text;
            a.RegDate = DateTime.Now;

        }
        void text_clear()
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;
            richTextBox1.Text = string.Empty;
            radioButton1.Checked = false;
            textBox1.Enabled = true;
            editId = 0;
            button3.Text = "Aktivität speichern";
            textBox1.Visible = true;
            textBox2.Visible = true;
            textBox3.Visible = true;
            textBox1.Focus();
        }
        void datagrid_refresh()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = bll.Read_All();
            GridHelper.Hide(dataGridView1, "id", "Expr1", "GörevliKodu");
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
        #endregion

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Bearbeiten einer vorhandenen Aktivität
            if (editId != 0)
            {
                if (!ubll.Access(userlogin, "Aktivitäten", can.Update))
                {
                    message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
                    return;
                }
                if (textBox4.Text.Trim() == string.Empty)
                {
                    message.Show("Hinweis", "Bitte einen Titel eingeben.", "", Button.ok, Logo.warning);
                    return;
                }
                message.Show("Information", bll.Update(editId, textBox4.Text, richTextBox1.Text), "", Button.ok, Logo.info);
                text_clear();
                datagrid_refresh();
                false_controls();
                return;
            }
            if (ubll.Access (userlogin, "Aktivitäten", can.Create ))
            {
                // Titel ist Pflicht; das Datum wird nur für eine Erinnerung geprüft
                if (textBox4.Text.Trim() != string.Empty && (!radioButton1.Checked || dateTimePicker1.Value.Date >= DateTime.Now.Date))
                {
                    ACTİVİTY a = new ACTİVİTY();
                    text_take(a);


                    message.Show("Information", bll.Create(a, u, c, ac), "", Button.ok, Logo.info);


                    if (radioButton1.Checked)
                    {
                        REMİNDER r = new REMİNDER();
                        r.Title = textBox4.Text;
                        r.Reminderİnfo = richTextBox1.Text;
                        r.ReminDate = dateTimePicker1.Value.Date;
                        r.RegDate = DateTime.Now;
                        label16.Text = rbll.Create(r, u);
                        timer1.Start();
                    }
                    text_clear();
                    datagrid_refresh();
                    false_controls();
                }
                else
                {
                    message.Show("Hinweis", "Bitte alle Felder ausfüllen.", "Bitte ein gültiges Erinnerungsdatum wählen.", Button.ok, Logo.warning);
                }
                
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (textBox1.Text != string.Empty)
            {
                c = cbll.Readname(textBox1.Text);
                if (c != null)
                {
                    textBox1.Enabled = false;

                    textBox2.Enabled = true;
                    button2.Enabled = true;
                }
                else
                {
                    message.Show("Hinweis","Kein Kunde gefunden.","",Button.ok, Logo.warning);
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != string.Empty)
            {
                u = ubll.Readuser(textBox2.Text);
                if (u != null)
                {
                    textBox2.Enabled = false;

                    textBox3.Enabled = true;
                    button4.Enabled = true;
                }
                else
                {
                    message.Show("Hinweis", "Kein Benutzer gefunden.", "", Button.ok, Logo.warning);
                }


            }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ubll.Access(userlogin, "Aktivitäten", can.Delete))
            {
                DialogResult i = message.Show("Löschen", "Wirklich löschen?", "", Button.yesorno, Logo.warning);
                if (i == DialogResult.Yes)
                {
                    if (id != 0) label16.Text = bll.Delete(id);
                    timer1.Start();
                }
                datagrid_refresh();
                label6.Text = bll.Activitypics();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            id = GridHelper.GetInt(dataGridView1, e.RowIndex, "id");
            uid = GridHelper.GetInt(dataGridView1, e.RowIndex, "GörevliKodu");

           

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != null)
            {
                ac = acbll.Readaccatagory(textBox3.Text);
                if (ac!= null)
                {
                    textBox3.Enabled = false;

                    textBox4.Enabled = true;
                    button3.Enabled = true;
                    radioButton1.Enabled = true;
                    dateTimePicker1.Enabled = true;
                    richTextBox1.Enabled = true;
                }
                else
                {
                    message.Show("Hinweis", "Keine Kategorie gefunden.", "Kategorien können unter „Einstellungen“ angelegt werden.\nFalls Ihnen die Berechtigung fehlt, wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
                }

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label16.Text = string.Empty;
            timer1.Stop();
        }

        private void detaylarıGörToolStripMenuItem_Click(object sender, EventArgs e)
        {
            USER u = ubll.Read_By_İd(uid);
            ACTİVİTY a = bll.Read_byid(id);
            if (a == null) return;
            string name = u != null ? u.Name : "-";
            if (!string.IsNullOrWhiteSpace(a.İnfo))
            {
                message.Show("Details", "Beschreibung: " + a.İnfo, " \n Zuständig: " + name, Button.ok, Logo.info);
            }
            else
            {
                message.Show("Details", "Beschreibung: " + "Keine Beschreibung vorhanden.", " \n Zuständig: " + name, Button.ok, Logo.info);
            }
        }

        private void bearbeitenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ubll.Access(userlogin, "Aktivitäten", can.Update))
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
                return;
            }
            ACTİVİTY a = bll.Read_byid(id);
            if (a == null) return;
            text_clear();
            editId = a.id;
            textBox1.Enabled = false;
            textBox4.Text = a.Title;
            richTextBox1.Text = a.İnfo;
            textBox4.Enabled = true;
            richTextBox1.Enabled = true;
            button3.Enabled = true;
            button3.Text = "Änderungen speichern";
            textBox4.Focus();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = bll.Search(textBox8.Text);
            GridHelper.Hide(dataGridView1, "id", "Expr1", "GörevliKodu");

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
