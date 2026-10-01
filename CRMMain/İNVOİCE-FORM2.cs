using BE;
using BLL;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using Stimulsoft;
using Stimulsoft.Report;
using System.Windows.Media.Effects;

namespace çağdaşcivata
{
    public partial class İNVOİCE_FORM2 : Form
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
        public İNVOİCE_FORM2()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            panel1.Visible=false;
            dateTimePickervade.Value = DateTime.Now;
            // Menüpunkt "Zahlung erfassen" für Rechnungen
            ToolStripMenuItem zahlung = new ToolStripMenuItem("Zahlung erfassen");
            zahlung.Click += zahlungToolStripMenuItem_Click;
            contextMenuStrip2.Items.Add(zahlung);
            // "Neuen Kunden anlegen" öffnet die Kundenverwaltung
            yeniMüşteriEkleToolStripMenuItem.Click += (o, ev) =>
            {
                new CUSTOMER_FORM().ShowDialog();
                var namen = new AutoCompleteStringCollection();
                namen.AddRange(cbll.Readname().ToArray());
                Custumert_Search_Txt.AutoCompleteCustomSource = namen;
            };
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = ibll.readall();
            GridHelper.Hide(dataGridView1, "id");
        }

       

        private void İNVOİCE_FORM2_Load(object sender, EventArgs e)
        {
            #region cutomer Search

            AutoCompleteStringCollection name = new AutoCompleteStringCollection();
            foreach (var item in cbll.Readname())
            {
                name.Add(item);
            }
            Custumert_Search_Txt.AutoCompleteCustomSource = name;
            #endregion
            #region Datagrid
            datagridviewsetting(dataGridView4);
            datagridviewsetting(dataGridView2);
            datagridviewsetting(dataGridView1);
            #endregion            
            UrundatagridFill(ConvertDataTableToProductList(pbll.ReadAll()));
            #region Enable Controls
            pictureBox2.Enabled = false;
            button1.Enabled = false;
            Enable_controls(false);
            #endregion
            MainWindow w = (MainWindow)System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault();
            userlogin = w.userlogin;
            comboBoxödemeşekli.Items.AddRange(Enum.GetValues(typeof(PAYMENT_METHOD)).Cast<PAYMENT_METHOD>().Select(İ => İ.ToString().Replace("_", " ")).ToArray());
            label9.Text = ibll.invoicepics();
            Hinweis(1);
        }

        /// <summary>Bedienhinweis für den aktuellen Schritt im Verkauf</summary>
        public void Hinweis(int schritt)
        {
            switch (schritt)
            {
                case 1: HelpHint.Set(label10, "Schritt 1: Kunden suchen → in der Liste anklicken → „Kunde auswählen“.   Rechnung anklicken: Drucken, Zahlung, Löschen."); break;
                case 2: HelpHint.Set(label10, "Schritt 2: Rechts oben auf „Warenkorb“ klicken, um Produkte hinzuzufügen."); break;
                case 3: HelpHint.Set(label17, "Schritt 3: Beim gewünschten Produkt die Stückzahl eingeben → „In den Warenkorb“."); break;
                case 4: HelpHint.Set(label17, "Schritt 4: Menge (und optional eigenen Preis) prüfen → auf ✓ klicken."); break;
                case 5: HelpHint.Set(label17, "Schritt 5: Weitere Produkte hinzufügen oder (optional „Bezahlt?“) → „Bestellung anlegen“."); break;
            }
        }
        #region Copy
        public int id_product;//İnvoice_datagrid_uc den id geliyor
        int id_Custumer; // datagrid4.cellclick den gelior
        CUSTOMER ChoseCudtomer = new CUSTOMER();
        CUSTOMER_BLL cbll = new CUSTOMER_BLL();
        USER Chose_Customer_user = new USER();
        USER_BLL ubll = new USER_BLL();
        public PRODUCT product = new PRODUCT();
        public List<PRODUCT> PRODUCTs = new List<PRODUCT>();
        PRODUCT_BLL pbll = new PRODUCT_BLL();
        MESSAGE_BOX mesage = new MESSAGE_BOX();
        USER userlogin = new USER();
        List<PRODUCT> productsstimulsoft = new List<PRODUCT>();


        void Enable_controls(bool e)
        {
            urunadet.Enabled = e;
            checkBoxfiyat.Enabled = e;
            fiyattxt.Enabled = e;
            radioButton100adet.Enabled = e;
            radioButton1adet.Enabled = e;
            comboBoxiskonto.Enabled = e;
            comboBoxkar.Enabled = e;
            checkBoxödeme.Enabled = e;
            comboBoxödemeşekli.Enabled = e;
            dateTimePickervade.Enabled = e;
            ödemetutartxt.Enabled = e;
            buttonsiparişoluştur.Enabled = e;
            button2.Enabled = e;
            ürün_eklebutonu.Enabled = e;

        }
        #endregion
        public void datagrid_fill_reeadall_product()
        {
            UrundatagridFill(ConvertDataTableToProductList(pbll.ReadAll()));
        }
        public void Take_produt(int id)
        {
            // Kopie für den Warenkorb: das Original im Katalog darf nicht verändert werden
            PRODUCT p = pbll.Readbyid(id);
            product = p == null ? null : new PRODUCT
            {
                id = p.id, Category = p.Category, Name = p.Name, Cap = p.Cap, Boy = p.Boy, Packing = p.Packing,
                Quality = p.Quality, Feature = p.Feature, Stock = p.Stock, SaledPices = 0, Price = p.Price,
                Kaplama = p.Kaplama, DINnumber = p.DINnumber, BrandName = p.BrandName, Product_cod = p.Product_cod,
                picture = p.picture
            };
        }
        private void Custumert_Search_Txt_TextChanged(object sender, EventArgs e)
        {
            dataGridView4.DataSource = null;
            dataGridView4.DataSource = cbll. İnvoice_Customer_Search(Custumert_Search_Txt.Text);
            GridHelper.Hide(dataGridView4, "id");
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
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
        public static List<PRODUCT> ConvertDataTableToProductList(DataTable dt)
        {
            List<PRODUCT> productList = new List<PRODUCT>();

            foreach (DataRow row in dt.Rows)
            {
                PRODUCT product = new PRODUCT
                {
                    id = Convert.ToInt32(row["id"]),
                    Category = row["Ürün"].ToString(),
                    Name = row["Ürün Adı"].ToString(),
                    Cap = row["Çap"].ToString(),
                    Boy = row["Boy"].ToString(),
                    Packing = row["Paket"].ToString(),
                    Quality = row["Kalite"].ToString(),

                    Stock = Convert.ToInt32(row["Stok"]),
                    Price = Convert.ToDouble(row["Fiyat"]),
                    Kaplama = row["Kaplama"].ToString(),

                    DINnumber = row["DIN"].ToString(),
                    BrandName = row["Marka"].ToString(),

                    picture = row["Görsel"].ToString(),


                };

                productList.Add(product);
            }

            return productList;
        }
        
        void Cart_Txt_clear()
        {
            urunadet.Text = "";
            UrunSearchtxt.Text = "";
            checkBoxfiyat.Checked = false;
            fiyattxt.Text = "";
            radioButton100adet.Checked = false;
            radioButton1adet.Checked = false;
            label34.Text = "........";
       
          

        }
        public void UrundatagridFill( List<PRODUCT> p)
        {
            paneldata.Controls.Clear();
            List<PRODUCT> products = p;
            int a = 0;
            int s = 0;
            foreach (var u in products)
            {
                if (s<= 10)
                {
                    İnvoice_datagrid_uc i = new İnvoice_datagrid_uc();
                    i.idlbl.Text = u.id.ToString();
                    i.categorylbl.Text = u.Category;
                    i.namelbl.Text = u.Name;
                    i.caplbl.Text = u.Cap;
                    i.boylbl.Text = u.Boy;
                    i.paketlbl.Text = u.Packing;
                    i.textBox1.Text = u.Packing;
                    i.Qualitylbl.Text = u.Quality;
                    i.Stocklbl.Text = u.Stock.ToString();
                    i.pricelbl.Text = u.Price.ToString("N2");
                    i.Brandlbl.Text = u.BrandName;
                    i.kaplamalbl.Text = u.Kaplama;
                    i.salepicslbl.Text = u.SaledPices.ToString();
                    i.dınlbl.Text = u.DINnumber;
                    i.pictureBox1.Image = ImageHelper.Load(u.picture, Properties.Resources.Adsız_tasarım__2_1);
                    i.ozelliklbl.Text = u.Feature;
                    if (i.categorylbl.Text == "Schraube" || i.categorylbl.Text == "Sonderschraube")
                    {
                        i.Hederlbl.Text = u.DINnumber +" "+ u.Name + " M" + u.Cap + " x " + u.Boy + " " + u.Quality + " " + u.Kaplama;
                    }
                    else if (i.categorylbl.Text == "Holz-/Blechschraube" || i.categorylbl.Text == "Sonder-Holzschraube")
                    {
                        if (i.namelbl.Text == "YSB METRİK" || i.namelbl.Text == "YHB METRİK" || i.namelbl.Text == "RYSB METRİK" || i.namelbl.Text == "TORX METRISCH" || i.namelbl.Text == "SONDERSCHRAUBE METRISCH")
                        {
                            i.Hederlbl.Text = u.DINnumber + " " + u.Name + " M" + u.Cap + " x " + u.Boy + " " + u.Quality + " " + u.Kaplama;
                        }
                        else
                        {
                            i.Hederlbl.Text = u.DINnumber + " " + u.Name + " " + u.Cap + " x " + u.Boy + " " + u.Quality + " " + u.Kaplama;
                        }                       
                    }
                    else if (i.categorylbl.Text == "Mutter" || i.categorylbl.Text == "Sondermutter")
                    {
                        i.Hederlbl.Text =  u.Name + " M" + u.Cap + " " + u.Quality + " " + u.Kaplama;
                    }
                    else if(i.categorylbl.Text == "Dübel" || i.categorylbl.Text == "Sonderdübel")
                    {
                        if (i.caplbl.Text == "10 mm" || i.caplbl.Text == "12 mm" || i.caplbl.Text == "3 mm" || i.caplbl.Text == "6 mm" || i.caplbl.Text == "7 mm" || i.caplbl.Text == "8 mm")
                        {
                            i.Hederlbl.Text =  u.Name + " " + u.Cap + " x " + u.Boy + " " + u.Kaplama;
                        }
                        else
                        {
                            i.Hederlbl.Text = u.Name + " M" + u.Cap + " x " + u.Boy + " " + u.Kaplama;
                        }
                    }
                    else if (i.categorylbl.Text == "Gewindestange" || i.categorylbl.Text == "Sonder-Gewindestange")
                    {
                        i.Hederlbl.Text = u.Name + " M" + u.Cap + " x " + u.Boy + " " + u.Kaplama;
                    }
                    else if (i.categorylbl.Text == "Unterlegscheibe" || i.categorylbl.Text == "Sonderscheibe")
                    {
                        i.Hederlbl.Text =u.DINnumber+" " + u.Name + " M" + u.Cap  + " " + u.Quality +" "+ u.Kaplama;
                    }
                    
                    paneldata.Controls.Add(i);
                    i.Location = new System.Drawing.Point(0, a);
                    a = a + i.Height + 25; // Abstand passt sich der Bildschirmskalierung an
                    s = s + 1;
                }

                //Civata
                //Vida
                //Somun
                //Dübel
                //Saplama
                //Pul
                //Özel Ürün

            }

        }
        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Klick auf Kopfzeile oder leere Fläche ignorieren (vorher NullReferenceException)
            if (e.RowIndex < 0) return;
            id_Custumer = GridHelper.GetInt(dataGridView4, e.RowIndex, "id");
            if (id_Custumer != 0) contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        private void müşteriSeçToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (id_Custumer != 0)
                {
                    ChoseCudtomer = cbll.Readbyid(id_Custumer);
                
                
                    if (ChoseCudtomer != null)
                    {
                       Custumert_Search_Txt.Text = ChoseCudtomer.Company;
                       Custumert_Search_Txt.Enabled = false;
                       dataGridView4.Enabled = false;
                       pictureBox2.Enabled = true;
                       button1.Enabled = true;
                        
                        label52.Text = ChoseCudtomer.Company;
                       label58.Text = ChoseCudtomer.Name;
                       Emaillbl.Text = ChoseCudtomer.Email;
                       label56.Text = ChoseCudtomer.Phone;
                       label1.Text = ChoseCudtomer.Alacak.ToString("N2");
                       label3.Text = ChoseCudtomer.Bakiye.ToString("N2");
                       label5.Text = ChoseCudtomer.Adress;
                       Chose_Customer_user = ubll.Read_By_İd(cbll.User_id_From_Custumer_id(ChoseCudtomer.id));
                       label38.Text = Chose_Customer_user.Name;
                      label54.Text = ChoseCudtomer.Regdate.ToString("dd.MM.yyyy");
                      Hinweis(2);
                    }
                    else
                    {
                        mesage.Show("Hinweis", "Kunde nicht gefunden.", "", Button.ok, Logo.warning);
                    }
                }
                else
                {
                    mesage.Show("Hinweis", "Bitte einen Kunden auswählen.", "", Button.ok, Logo.warning);
                }
            }
            catch (Exception q)
            {

                mesage.Show("Hinweis", "Bei der Verarbeitung ist ein Fehler aufgetreten", q.Message, Button.ok, Logo.warning);
            }
           


        }

        private void button1_Click(object sender, EventArgs e)
        {
            Custumert_Search_Txt.Text = "";
            Custumert_Search_Txt.Enabled = true;
            dataGridView4.DataSource = null;
            dataGridView4.Enabled = true;
            pictureBox2.Enabled = false;
            button1.Enabled = false;
            label52.Text = "";
            label58.Text = "";
            Emaillbl.Text ="";
            label56.Text = "";
            label1.Text =   "";
            label3.Text = "";
            label5.Text = "";
            Chose_Customer_user = null;
            label38.Text = "";
            label54.Text = "";
            id_Custumer = 0;    
            Hinweis(1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
           
            panel1.Visible = true;
            panel1.Location = new System.Drawing.Point(10, 12);
            Hinweis(PRODUCTs.Count > 0 ? 5 : 3);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            panel1.Visible=false;
            //panel1.Controls.Clear();
            //Enable_controls(false);
            urunadet.Enabled = false;
            checkBoxfiyat.Enabled = false;
            fiyattxt.Enabled = false;
            radioButton100adet.Enabled = false;
            radioButton1adet.Enabled = false;
            comboBoxiskonto.Enabled = false;
            comboBoxkar.Enabled = false;
            checkBoxödeme.Enabled = false;
            comboBoxödemeşekli.Enabled = false;
            dateTimePickervade.Enabled = false;
            ödemetutartxt.Enabled = false;
            buttonsiparişoluştur.Enabled = false;
            button2.Enabled = false;
            ürün_eklebutonu.Enabled = false;
            Cart_Txt_clear();
            toplamtutarlbl.Text = "";
            kdvlbl.Text = "";
            tutarlbl.Text = "";
            checkBoxödeme.Checked = false;
            comboBoxödemeşekli.Text = "";
            ödemetutartxt.Text = "";
            PRODUCTs.Clear();
            dataGridView2.DataSource = null;
            product = null;
            paneldata.Enabled = true;
            UrunSearchtxt.Enabled = true;
            Summen_zuruecksetzen();
        }

        private void UrunSearchtxt_TextChanged(object sender, EventArgs e)
        {
            if (!UrunSearchtxt.Enabled) return; // Text wird beim Auswählen gesetzt – keine Suche nötig
            if (UrunSearchtxt.Text != string.Empty)
            {

                UrundatagridFill(ConvertDataTableToProductList(pbll.Search(UrunSearchtxt.Text)));
            }
            else
            {
                UrundatagridFill(ConvertDataTableToProductList(pbll.ReadAll()));

            }
            
        }
       
        private void button2_Click(object sender, EventArgs e)
        {
            UrunSearchtxt.Enabled = true;
            Enable_controls(false);
            Cart_Txt_clear();
            paneldata.Enabled = true;          
            Hinweis(PRODUCTs.Count > 0 ? 5 : 3);

        }

        private void urunadet_TextChanged(object sender, EventArgs e)
        {

        }

        private void UrunSearchtxt_EnabledChanged(object sender, EventArgs e)
        {
            // Liste wird beim Auswählen bereits vom Produkt selbst neu geladen (vorher 2–3x)
            if (UrunSearchtxt.Enabled == false)
            {
                Hinweis(4);
            }
        }
        void fiyat_kendim_belirleyeceğım_checkbox(bool b)
        {
              fiyattxt.Enabled = b;
              radioButton100adet.Enabled = b;
              radioButton1adet.Enabled = b;
            //comboBoxkar.Enabled= b;
            //comboBoxiskonto.Enabled= b;
        }
        private void checkBoxfiyat_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxfiyat.Checked)
            {
                fiyat_kendim_belirleyeceğım_checkbox(true);
            }
            else
            {
                fiyat_kendim_belirleyeceğım_checkbox(false);
            }
        }

        private void radioButton100adet_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton100adet.Checked)
            {
                comboBoxiskonto.Enabled = true;
                comboBoxkar.Enabled = true;

            }
            else
            {
                comboBoxiskonto.Enabled = false;
                comboBoxkar.Enabled = false;
            }
            
        }

        private void radioButton1adet_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1adet.Checked)
            {
                comboBoxiskonto.Enabled = true;
                comboBoxkar.Enabled = true;

            }
            else
            {
                comboBoxiskonto.Enabled = false;
                comboBoxkar.Enabled = false;
            }
        }
        İNVOCE_BLL ibll = new İNVOCE_BLL();
        void FillDatsgrid()
        {
           
               
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = PRODUCTs.ToList(); // Kopie, damit die Tabelle neu gezeichnet wird
            GridHelper.Hide(dataGridView2, "id", "DeletStatus", "Feature", "Product_cod", "Packing", "Stock", "DINnumber", "BrandName", "picture", "Category", "invoices");
            dataGridView2.Columns["Name"].HeaderText = "Produkt";
            dataGridView2.Columns["Cap"].HeaderText = "Ø";
            dataGridView2.Columns["Boy"].HeaderText = "Länge";
            dataGridView2.Columns["Quality"].HeaderText = "Güte";
            dataGridView2.Columns["Price"].HeaderText = "Preis/Stk.";
            dataGridView2.Columns["Kaplama"].HeaderText = "Überzug";
            dataGridView2.Columns["SaledPices"].HeaderText = "Stück";


        }
        private void ürün_eklebutonu_Click(object sender, EventArgs e)
        {
            //try
            //{
                int menge;
                if (product == null || UrunSearchtxt.Text == string.Empty || !int.TryParse(urunadet.Text.Trim(), out menge) || menge <= 0)
                {
                    mesage.Show("Hinweis", "Bitte ein Produkt wählen und eine gültige Menge (ganze Zahl) eingeben.", "", Button.ok, Logo.warning);
                    return;
                }
                else
                {
                    try
                    {
                        Hesaplama();
                    }
                    catch (FormatException)
                    {
                        mesage.Show("Hinweis", "Bitte gültige Zahlen für Preis, Rabatt und Gewinn eingeben.", "", Button.ok, Logo.warning);
                        errorr = true;
                    }
                    if (!errorr)
                    {
                        
                        buttonsiparişoluştur.Enabled = true;
                        checkBoxödeme.Enabled = true;
                        // Satılan miktar ve fiyat – nur für das gerade hinzugefügte Produkt
                        product.SaledPices = menge;
                        product.Price = Fiyat;
                        PRODUCTs.Add(product);
                        product = null;
                        Hinweis(5);
                        urunadet.Text = "";
                        paneldata.Enabled = true;
                        UrunSearchtxt.Enabled = true;
                        FillDatsgrid();
                   
                    urunadet.Enabled = false;
                    checkBoxfiyat.Enabled = false;
                    fiyattxt.Enabled = false;
                    radioButton100adet.Enabled = false;
                    radioButton1adet.Enabled = false;
                    comboBoxiskonto.Enabled = false;
                    comboBoxkar.Enabled = false;
                    checkBoxödeme.Enabled = false;
                    comboBoxödemeşekli.Enabled = false;
                    dateTimePickervade.Enabled = false;
                    ödemetutartxt.Enabled = false;
                    buttonsiparişoluştur.Enabled = false;
                    button2.Enabled = false;
                    ürün_eklebutonu.Enabled = false;

                    Cart_Txt_clear();
                        buttonsiparişoluştur.Enabled = true;
                        checkBoxödeme.Enabled =true;
                    UrundatagridFill(ConvertDataTableToProductList(pbll.ReadAll()));
                }
                    else
                    {
                        errorr = false;
                        return;
                    }
                }
                errorr = false;

               
                UrunSearchtxt.Text = "";
                checkBoxfiyat.Checked = false;
                fiyattxt.Text = "";
                radioButton100adet.Checked = false;
                radioButton1adet.Checked = false;
                label34.Text = "........";
            //}
            //catch (Exception x)
            //{

            //    mesage.Show("Hinweis", "İşlem Sırasında Bir sorun Oluştu!", x.Message, Button.ok, Logo.warning);
            //}
            
        }
        #region varibale
        int idselect;
        bool errorr = false;
        int uid;
        

        double ürünadeti;

        double Fiyat;

        double adetxfiyat;

        double toplamtutar = 0;

        double kdv;

        double totalfinal = 0;

        double total;

        double kdvlifiyat = 0;

        double karsızfiyat;
        int idinv;

        #endregion

        void Hesaplama()
        {
            if (!errorr)
            {
                Fiyat = product.Price;
                label34.Text = Fiyat.ToString("N2");
                if (checkBoxfiyat.Checked)
                {
                    Fiyat_hesaplama_formul();
                  
                }
                label34.Text = Fiyat.ToString("N2");
                ürünadeti = Convert.ToDouble(urunadet.Text);

                adetxfiyat = ürünadeti * Fiyat;

                toplamtutar = adetxfiyat + toplamtutar;

                kdv = ((adetxfiyat / 100) * 20);

                kdvlifiyat = kdv + kdvlifiyat;

                total = toplamtutar + kdvlifiyat;
                toplamtutarlbl.Text = toplamtutar.ToString("N2");
                kdvlbl.Text = kdvlifiyat.ToString("N2");
                tutarlbl.Text = total.ToString("N2");
                
            }
            else
            {
                return;
            }


        }
       
        void Fiyat_hesaplama_formul()
        {
            if (radioButton100adet.Checked)
            {
                if (fiyattxt.Text != string.Empty)
                {
                    if (comboBoxiskonto.Text == string.Empty)
                    {
                        if (comboBoxkar.Text == string.Empty)
                        {
                            Fiyat = Convert.ToDouble(fiyattxt.Text) / 100;
                        }
                        else if (comboBoxkar.Text != string.Empty)
                        {
                            Fiyat = (((Convert.ToDouble(fiyattxt.Text) / 100) / 100) * Convert.ToDouble(comboBoxkar.Text)) + (Convert.ToDouble(fiyattxt.Text) / 100);
                        }
                    }
                    else
                    {
                        if (comboBoxkar.Text == string.Empty)
                        {
                            Fiyat = (Convert.ToDouble(fiyattxt.Text) / 100) - (((Convert.ToDouble(fiyattxt.Text) / 100) / 100) * Convert.ToDouble(comboBoxiskonto.Text));
                        }
                        else if (comboBoxkar.Text != string.Empty)
                        {
                            karsızfiyat = ((Convert.ToDouble(fiyattxt.Text) / 100) - (((Convert.ToDouble(fiyattxt.Text) / 100) / 100) * Convert.ToDouble(comboBoxiskonto.Text)));
                            Fiyat = ((karsızfiyat / 100) * Convert.ToDouble(comboBoxkar.Text)) + karsızfiyat;
                        }
                    }
                }
                else
                {
                    mesage.Show("Achtung", "Bitte den Listenpreis eingeben.", "", Button.ok, Logo.warning);
                    errorr = true;
                    return;
                }

            }
            else if (radioButton1adet.Checked)
            {
                if (fiyattxt.Text != string.Empty)
                {
                    if (comboBoxiskonto.Text == string.Empty)
                    {
                        if (comboBoxkar.Text == string.Empty)
                        {
                            Fiyat = Convert.ToDouble(fiyattxt.Text);
                        }
                        else if (comboBoxkar.Text != string.Empty)
                        {
                            Fiyat = (((Convert.ToDouble(fiyattxt.Text) / 100)) * Convert.ToDouble(comboBoxkar.Text)) + (Convert.ToDouble(fiyattxt.Text));
                        }
                    }
                    else
                    {
                        if (comboBoxkar.Text == string.Empty)
                        {
                            Fiyat = (Convert.ToDouble(fiyattxt.Text)) - (((Convert.ToDouble(fiyattxt.Text) / 100)) * Convert.ToDouble(comboBoxiskonto.Text));
                        }
                        else if (comboBoxkar.Text != string.Empty)
                        {
                            karsızfiyat = (Convert.ToDouble(fiyattxt.Text)) - (((Convert.ToDouble(fiyattxt.Text) / 100)) * Convert.ToDouble(comboBoxiskonto.Text));
                            Fiyat = ((karsızfiyat / 100) * Convert.ToDouble(comboBoxkar.Text)) + karsızfiyat;
                        }
                    }
                }
                else
                {
                    mesage.Show("Achtung", "Bitte den Listenpreis eingeben.", "", Button.ok, Logo.warning);
                    errorr = true;
                    return;
                }
            }
            else
            {
                mesage.Show("Achtung", "Bitte angeben: Preis pro 100 Stück oder pro Stück?", "", Button.ok, Logo.warning);
                errorr = true;
                return;
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void checkBoxödeme_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxödeme.Checked)
            {
                comboBoxödemeşekli.Enabled = true;
                dateTimePickervade.Enabled = true;
                ödemetutartxt.Enabled = true;
            }
            else
            {
                comboBoxödemeşekli.Enabled = false;
                dateTimePickervade.Enabled = false;
                ödemetutartxt.Enabled = false;
            }
        }

        private void buttonsiparişoluştur_Click(object sender, EventArgs e)
        {
            if (ubll.Access(userlogin, "Verkauf", can.Create))
            {
                // Erst alles prüfen – erst danach werden Salden und Rechnung gespeichert
                double zahlung = 0;
                if (ChoseCudtomer == null || ChoseCudtomer.id == 0 || PRODUCTs.Count == 0)
                {
                    mesage.Show("Hinweis", "Bitte einen Kunden wählen und mindestens ein Produkt in den Warenkorb legen.", "", Button.ok, Logo.warning);
                    return;
                }
                if (checkBoxödeme.Checked && (comboBoxödemeşekli.SelectedItem == null || !double.TryParse(ödemetutartxt.Text.Replace("€", "").Trim(), out zahlung) || zahlung < 0))
                {
                    mesage.Show("Hinweis", "Bitte Zahlungsart und einen gültigen Zahlungsbetrag angeben.", "", Button.ok, Logo.warning);
                    return;
                }
                try
                {
                    İNVOİCE i = new İNVOİCE();
                    i.TotalPrice = Math.Round(total, 2);
                    i.RegDate = DateTime.Now.Date;
                    i.VadeDate = dateTimePickervade.Value.Date;
                    i.Bakiye = i.TotalPrice;
                    if (checkBoxödeme.Checked)
                    {
                        PAYMENT_METHOD seçilenödeme = (PAYMENT_METHOD)Enum.Parse(typeof(PAYMENT_METHOD), comboBoxödemeşekli.SelectedItem.ToString().Replace(" ", "_"));
                        i.ödemeŞekli = seçilenödeme;
                        i.ÖdemeDate = DateTime.Now.Date;
                        i.ÖdemeTurarı = zahlung;
                        i.Bakiye = i.TotalPrice - zahlung;
                        i.İsCheckedOut = i.Bakiye <= 0.005;
                        if (i.İsCheckedOut) i.CeackOutDate = DateTime.Now.Date;
                    }
                    else
                    {
                        i.İsCheckedOut = false;
                    }

                    string ergebnis = ibll.Create(i, ChoseCudtomer, PRODUCTs, Chose_Customer_user);
                    if (!ergebnis.StartsWith("Bestellung"))
                    {
                        mesage.Show("Hinweis", "Die Bestellung konnte nicht gespeichert werden.", ergebnis, Button.ok, Logo.warning);
                        return;
                    }
                    // Salden des Kunden erst nach erfolgreicher Speicherung anpassen
                    cbll.CreateBakiye(ChoseCudtomer, i.TotalPrice);
                    if (checkBoxödeme.Checked && zahlung > 0)
                    {
                        cbll.Create_payment(ChoseCudtomer, zahlung);
                    }

                    DialogResult res = mesage.Show("Speichern", "Rechnung drucken?", ergebnis + " Rechnungs-Nr.: " + i.invoiceNumber, Button.yesorno, Logo.info);
                    if (res == DialogResult.Yes)
                    {
                         StiReport sti = new StiReport();
                        sti.Load(AppDomain.CurrentDomain.BaseDirectory+ "İnvoice.mrt");
                        sti.Dictionary.Variables["Company"].Value = "Schraubwerk Demo GmbH";
                        sti.Dictionary.Variables["Adress"].Value = "Musterstraße 1 \n 70173 Stuttgart";
                        sti.Dictionary.Variables["NameUser"].Value = userlogin.Name ;
                        sti.Dictionary.Variables["Phone"].Value = userlogin.PhoneNumber;
                        sti.Dictionary.Variables["E-mail"].Value = ChoseCudtomer.Email;
                        sti.Dictionary.Variables["CustomerCompany"].Value = ChoseCudtomer.Company;
                        sti.Dictionary.Variables["CustomerAdress"].Value =ChoseCudtomer.Adress;
                        sti.Dictionary.Variables["CustomerName"].Value = ChoseCudtomer.Name;
                        sti.Dictionary.Variables["CustomerPhone"].Value = ChoseCudtomer.Phone;
                        sti.Dictionary.Variables["CustomerE-mail"].Value = ChoseCudtomer.Email;
                        sti.Dictionary.Variables["İnvoiceNumber"].Value = i.invoiceNumber;
                        sti.Dictionary.Variables["Date"].Value = DateTime.Now.Date.ToString("dd,MM,yyyy") ;
                        sti.RegBusinessObject("PRODUCT",PRODUCTs ) ;
                        sti.Render();
                        sti.Show();

                    }
                    urunadet.Enabled = false;
                    checkBoxfiyat.Enabled = false;
                    fiyattxt.Enabled = false;
                    radioButton100adet.Enabled = false;
                    radioButton1adet.Enabled = false;
                    comboBoxiskonto.Enabled = false;
                    comboBoxkar.Enabled = false;
                    checkBoxödeme.Enabled = false;
                    comboBoxödemeşekli.Enabled = false;
                    dateTimePickervade.Enabled = false;
                    ödemetutartxt.Enabled = false;
                    buttonsiparişoluştur.Enabled = false;
                    button2.Enabled = false;
                    ürün_eklebutonu.Enabled = false;
                    Cart_Txt_clear();
                    toplamtutarlbl.Text = "";
                    kdvlbl.Text = "";
                    tutarlbl.Text = "";
                    checkBoxödeme.Checked = false;
                    comboBoxödemeşekli.Text = "";
                    ödemetutartxt.Text = "";
                    PRODUCTs.Clear();
                    dataGridView2.DataSource = null;
                    product = null;
                    paneldata.Enabled = true;
                    UrunSearchtxt.Enabled = true;
                    Summen_zuruecksetzen();
                    comboBoxödemeşekli.SelectedIndex = -1;
                    // Salden des Kunden in der Anzeige aktualisieren
                    label1.Text = ChoseCudtomer.Alacak.ToString("N2");
                    label3.Text = ChoseCudtomer.Bakiye.ToString("N2");
                    UrundatagridFill(ConvertDataTableToProductList(pbll.ReadAll())); // Bestand aktualisiert
                    Hinweis(3);
                }
                catch (Exception a)
                {
                    mesage.Show("Information", "Beim Anlegen der Bestellung ist ein Fehler aufgetreten.", a.Message, Button.ok, Logo.warning);
                }
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = ibll.readall();
                GridHelper.Hide(dataGridView1, "id");
                label9.Text = ibll.invoicepics();
            }
            else
            {
                mesage.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        // Zwischensummen für die nächste Bestellung auf 0 setzen
        void Summen_zuruecksetzen()
        {
            toplamtutar = 0;
            kdvlifiyat = 0;
            total = 0;
            toplamtutarlbl.Text = "";
            kdvlbl.Text = "";
            tutarlbl.Text = "";
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            contextMenuStrip2.Show(Cursor.Position.X, Cursor.Position.Y);
            idinv = GridHelper.GetInt(dataGridView1, e.RowIndex, "id");
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ubll.Access(userlogin, "Verkauf", can.Delete))
            {
                mesage.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
                return;
            }
            if (idinv != 0 && DialogResult.Yes == mesage.Show("Löschen","Rechnung wirklich löschen?","Bestand und Kundensaldo werden zurückgebucht.",Button.yesorno,Logo.warning))
            {
                mesage.Show("Information", ibll.Delete(idinv), "", Button.ok, Logo.info);
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = ibll.readall();
                GridHelper.Hide(dataGridView1, "id");
                label9.Text = ibll.invoicepics();
                UrundatagridFill(ConvertDataTableToProductList(pbll.ReadAll()));
            }
        }
        
        private void ödemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PAMENT_FORM P = new PAMENT_FORM();    
            
            P.ShowDialog();
          
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = textBox8.Text == string.Empty ? ibll.readall() : ibll.SearchCustomer(textBox8.Text);
            GridHelper.Hide(dataGridView1, "id");
        }

        private void çıktıAlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Vorhandene Rechnung erneut drucken
            İNVOİCE i = ibll.read_full(idinv);
            if (i == null)
            {
                mesage.Show("Hinweis", "Bitte zuerst eine Rechnung auswählen.", "", Button.ok, Logo.info);
                return;
            }
            CUSTOMER c = i.Customer ?? new CUSTOMER();
            USER u = i.User ?? userlogin;
            StiReport sti = new StiReport();
            sti.Dictionary.Variables["Company"].Value = "Schraubwerk Demo GmbH";
            sti.Dictionary.Variables["Adress"].Value = "Musterstraße 1 \n 70173 Stuttgart";
            sti.Dictionary.Variables["NameUser"].Value = u.Name;
            sti.Dictionary.Variables["Phone"].Value = u.PhoneNumber;
            sti.Dictionary.Variables["CustomerCompany"].Value = c.Company;
            sti.Dictionary.Variables["CustomerAdress"].Value = c.Adress;
            sti.Dictionary.Variables["CustomerName"].Value = c.Name;
            sti.Dictionary.Variables["CustomerPhone"].Value = c.Phone;
            sti.Dictionary.Variables["CustomerE-mail"].Value = c.Email;
            sti.Dictionary.Variables["İnvoiceNumber"].Value = i.invoiceNumber;
            sti.Dictionary.Variables["Date"].Value = i.RegDate.ToString("dd.MM.yyyy");
            sti.RegBusinessObject("PRODUCT", i.products);
            sti.Render();
            sti.Show();
        }

        // Zahlung zur ausgewählten Rechnung erfassen
        private void zahlungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PAMENT_FORM p = new PAMENT_FORM();
            İNVOİCE i = ibll.read_full(idinv);
            if (i != null && i.Customer != null) p.PresetCompany = i.Customer.Company;
            p.ShowDialog();
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = ibll.readall();
            GridHelper.Hide(dataGridView1, "id");
        }
    }
}
