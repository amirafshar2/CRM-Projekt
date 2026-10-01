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
using System.IO;
using BLL;
using BE;
using HandyControl.Themes;

namespace çağdaşcivata
{    
    public partial class PAMENT_FORM : Form
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
        public PAMENT_FORM()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));

        }

        private void PAMENT_FORM_Load(object sender, EventArgs e)
        {
            HelpHint.Set(this, "1. Kunde suchen → „+“   2. Rechnung in der Liste wählen   3. Zahlungsart und Betrag → Zahlung speichern.");
            AutoCompleteStringCollection names = new AutoCompleteStringCollection();
            foreach (var item in cbll.Readname())
            {
                names.Add(item.ToString());
            }
            textBox1.AutoCompleteCustomSource = names;
            MainWindow w = (MainWindow)System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault();
            userlogin = w.userlogin;
            comboBox3.Items.AddRange(Enum.GetValues(typeof(PAYMENT_METHOD)).Cast<PAYMENT_METHOD>().Select(İ => İ.ToString().Replace("_", " ")).ToArray());
            dateTimePicker1.Enabled = false;
            comboBox3.Enabled = false;
            textBox5.Enabled = false;
            button4.Enabled = false;
            datagridviewsetting(dataGridView3);
            // Aufruf aus dem Verkauf: Kunde ist schon bekannt
            if (!string.IsNullOrEmpty(PresetCompany))
            {
                textBox1.Text = PresetCompany;
                button1_Click(this, EventArgs.Empty);
            }
        }

        /// <summary>Firmenname, der beim Öffnen direkt gesucht wird (optional)</summary>
        public string PresetCompany { get; set; }
        İNVOCE_BLL ibll = new İNVOCE_BLL();
        MESSAGE_BOX message = new MESSAGE_BOX();

        void Rechnungen_laden()
        {
            dataGridView3.DataSource = null;
            dataGridView3.DataSource = cbll.Read_Bakiye(custumer.id);
            GridHelper.Hide(dataGridView3, "c_id", "id");
            // erste noch offene Rechnung vorauswählen
            foreach (DataGridViewRow r in dataGridView3.Rows)
            {
                double offen;
                object v = dataGridView3.Columns.Contains("Offen") ? r.Cells["Offen"].Value : null;
                if (v != null && v != DBNull.Value && double.TryParse(Convert.ToString(v), out offen) && offen > 0.005)
                {
                    r.Selected = true;
                    dataGridView3.CurrentCell = r.Cells.Cast<DataGridViewCell>().First(c => c.Visible);
                    break;
                }
            }
        }

        #region COPY
        USER userlogin = new USER();
        CUSTOMER_BLL cbll = new CUSTOMER_BLL();
        public CUSTOMER custumer = new CUSTOMER();
        int uid;
        public USER user = new USER();
        USER_BLL UserBLL = new USER_BLL();
        #endregion
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
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {
                custumer = cbll.Readname(textBox1.Text);
                if (custumer != null)
                {
                    Rechnungen_laden();
                    textBox1.Enabled = false;
                    comboBox3.Enabled = true;
                    label10.Text = custumer.Name;
                    label17.Text = custumer.Phone.ToString();
                    label22.Text = custumer.Company;
                    uid = cbll.GetUserIdformcustumer(custumer.id);
                    user = UserBLL.Read_By_İd(uid);
                    label27.Text = user.UserName;
                    label19.Text = DateTime.Now.Date.ToString("dd.MM.yyyy");
                }
                else
                {
                    custumer = new CUSTOMER();
                    message.Show("Hinweis", "Kein Kunde gefunden.", "", Button.ok, Logo.warning);
                }


            }
            else
            {
                return;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox5.Enabled = true;
            dateTimePicker1.Enabled = true;
            button4.Enabled = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Zahlung speichern
        private void button4_Click(object sender, EventArgs e)
        {
            double betrag;
            if (custumer == null || custumer.id == 0)
            {
                message.Show("Hinweis", "Bitte zuerst einen Kunden suchen.", "", Button.ok, Logo.warning);
                return;
            }
            if (comboBox3.SelectedItem == null || !double.TryParse(textBox5.Text.Replace("€", "").Trim(), out betrag) || betrag <= 0)
            {
                message.Show("Hinweis", "Bitte Zahlungsart und einen gültigen Betrag eingeben (z. B. 150,00).", "", Button.ok, Logo.warning);
                return;
            }
            int invoiceId = dataGridView3.CurrentRow != null ? GridHelper.GetInt(dataGridView3, dataGridView3.CurrentRow.Index, "id") : 0;
            if (invoiceId == 0)
            {
                message.Show("Hinweis", "Bitte in der Liste die Rechnung auswählen, die bezahlt wird.", "", Button.ok, Logo.warning);
                return;
            }
            PAYMENT_METHOD art = (PAYMENT_METHOD)Enum.Parse(typeof(PAYMENT_METHOD), comboBox3.SelectedItem.ToString().Replace(" ", "_"));
            string ergebnis = ibll.AddPayment(invoiceId, betrag, art, dateTimePicker1.Value);
            if (ergebnis.StartsWith("Zahlung"))
            {
                cbll.Create_payment(custumer, betrag); // "Bezahlt gesamt" des Kunden
                textBox5.Text = "";
            }
            message.Show("Zahlung", ergebnis, "", Button.ok, Logo.info);
            Rechnungen_laden();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
