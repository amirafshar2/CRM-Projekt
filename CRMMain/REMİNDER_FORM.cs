using BE;
using BLL;
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

namespace çağdaşcivata
{
    public partial class REMİNDER_FORM : Form
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
        public REMİNDER_FORM()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }
        private void REMİNDER_FORM_Load(object sender, EventArgs e)
        {
            HelpHint.Set(this, "Benutzer → „+“ → Betreff, Datum → Speichern.  Eintrag anklicken: Bearbeiten, Erledigt, Löschen.");
            AutoCompleteStringCollection names = new AutoCompleteStringCollection();
            foreach (var item in ubll.Readusername())
            {
                names.Add(item);
            }
            textBox1.AutoCompleteCustomSource = names;
            datagrid_refresh();
            datagridviewsetting(dataGridView1);
            MainWindow w = (MainWindow)System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault();
            userlogin = w.userlogin;
            textBox2.Enabled = false;
            richTextBox1.Enabled = false;
            dateTimePicker1.Enabled = false;
            button3.Enabled = false;
            label6.Text = BLL.TotalReminders();
        }
        #region copy
        USER userlogin = new USER();
        USER_BLL ubll = new USER_BLL();
        MESSAGE_BOX messageb = new MESSAGE_BOX();
        REMİNDER_BLL BLL = new REMİNDER_BLL();
        CUSTOMER c = new CUSTOMER();
        USER u = new USER();
        #endregion
        #region method 
        void datagrid_refresh()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = BLL.Read_all();
            GridHelper.Hide(dataGridView1, "id");
            label6.Text = BLL.TotalReminders();

        }
        public void datagridviewsetting(DataGridView d)
        {
            d.DefaultCellStyle.BackColor = Color.FromArgb(108, 117, 125);
            d.RowHeadersVisible = false;
            d.BorderStyle = BorderStyle.None;
            d.BackgroundColor = Color.FromArgb(108, 117, 125);
            d.DefaultCellStyle.SelectionForeColor = Color.White;
            d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(73, 80, 87);
            d.EnableHeadersVisualStyles = false;
            d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            d.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(108, 117, 125);
            d.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        void Text_clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            richTextBox1.Text = "";
            textBox1.Enabled = true;
            textBox2.Enabled = false;
            richTextBox1.Enabled = false;
            dateTimePicker1.Enabled = false;
            button3.Enabled = false;
            button3.Text = "Speichern";
            dateTimePicker1.Value = DateTime.Now;
            u = new USER();
            textBox1.Focus();
        }
        void text_take(REMİNDER r)
        {
            r.Title = textBox2.Text;
            r.Reminderİnfo = richTextBox1.Text;
            r.RegDate = DateTime.Now;
            r.ReminDate = dateTimePicker1.Value.Date;
        }
        #endregion
        int id;
        string Username;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (u != null && u.id != 0 && textBox2.Text.Trim() != string.Empty)
            {
                REMİNDER r = new REMİNDER();
                text_take(r);
                if (button3.Text == "Speichern")
                {
                    if (ubll.Access(userlogin, "Erinnerungen", can.Create))
                    {
                        messageb.Show("Information", BLL.Create(r, u), "", Button.ok, Logo.info);
                        datagrid_refresh();
                        Text_clear();
                    }
                    else
                    {
                        messageb.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.info);
                    }
                }
                else
                {
                    if (ubll.Access(userlogin, "Erinnerungen", can.Update))
                    {
                        messageb.Show("Information", BLL.Update(r, id, u ), "", Button.ok, Logo.info);
                        datagrid_refresh();
                        Text_clear();
                    }
                    else
                    {
                        messageb.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.info);
                    }
                }
            }
            else
            {
                messageb.Show("Hinweis", "Bitte Benutzer und Betreff angeben.", "", Button.ok, Logo.info);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            u = ubll.Readuser(textBox1.Text);
            if (u != null)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = true;
                richTextBox1.Enabled = true;
                dateTimePicker1.Enabled = true;
                button3.Enabled = true;
            }
            else
            {
                u = new USER();
                messageb.Show("Hinweis", "Kein Benutzer gefunden.", "", Button.ok, Logo.warning);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            id = GridHelper.GetInt(dataGridView1, e.RowIndex, "id");
            Username = GridHelper.GetText(dataGridView1, e.RowIndex, "Görevli");

        }

        private void düzenleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ubll.Access(userlogin, "Erinnerungen", can.Update))
            {
                if (id != 0)
                {
                    USER user = ubll.Readuser(Username);
                    REMİNDER r = BLL.Readbyid(id);

                    if (r == null) return;
                    if (user != null)
                    {
                        textBox1.Text = user.UserName;
                        u = user;
                    }
                    else
                    {
                        textBox1.Text = "Benutzer nicht gefunden.";
                    }

                    textBox2.Text = r.Title;
                    richTextBox1.Text = r.Reminderİnfo;
                    dateTimePicker1.Value = r.ReminDate;
                    button3.Text = "Bearbeiten";
                    // Felder zum Bearbeiten freigeben
                    textBox1.Enabled = false;
                    textBox2.Enabled = true;
                    richTextBox1.Enabled = true;
                    dateTimePicker1.Enabled = true;
                    button3.Enabled = true;
                }
                else
                {
                    messageb.Show("Information", "Bitte eine Erinnerung auswählen.", "Bitte erneut versuchen", Button.ok, Logo.info);
                }
            }
            else
            {
                messageb.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.info);
            }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (ubll.Access(userlogin, "Erinnerungen", can.Delete))
            {

                DialogResult i = messageb.Show("Wirklich löschen?", "Sind Sie sicher?", " ", Button.yesorno, Logo.warning);
                if (i == DialogResult.Yes)
                {
                    messageb.Show("Information", BLL.Delete(id), " ", Button.ok, Logo.info);

                }
                datagrid_refresh();
            }
            else
            {
                messageb.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.info);
            }
        }

        private void yapıldıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ubll.Access(userlogin, "Erinnerungen", can.Update))
            {
                messageb.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.info);
                return;
            }
            DialogResult i = messageb.Show("Erledigt?", "Erinnerung schließen?", "", Button.yesorno, Logo.info);
            if (i == DialogResult.Yes && id != 0)
            {
                REMİNDER R = BLL.Readbyid(id);
                BLL.İsDone(R, id);
            }
            datagrid_refresh();
        }
        private void dataGridView1_Click(object sender, EventArgs e)
        {

        }

        // Suche (Betreff oder Benutzer)
        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = textBox8.Text == string.Empty ? BLL.Read_all() : BLL.ReminderSearch(textBox8.Text);
            GridHelper.Hide(dataGridView1, "id");
        }
    }
}
