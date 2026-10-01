using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace çağdaşcivata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Copy
        public USER userlogin =  new USER();
         int product_id_my_datagrid;
        LOAD_FORMcs lf = new LOAD_FORMcs();        
        USER_BLL ubll = new USER_BLL();
        MESSAGE_BOX message = new MESSAGE_BOX();
        CALNDER_UC c = new CALNDER_UC   ();
        MAİN_BLL mainbll = new MAİN_BLL();
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }

        #region EVENT
        private void maingrid_Loaded(object sender, RoutedEventArgs e)
        {
            Openwinform(lf);
            Window g = this.FindName("HomeMain") as Window;
            BlurBitmapEffect blurBitmapEffect = new BlurBitmapEffect();
            blurBitmapEffect.Radius = 20;
            g.BitmapEffect = blurBitmapEffect;
            MESSAGE_BOX messagebox = new MESSAGE_BOX();
            //messagebox.Show("Giriş Başarılı", userlogin.Name + " Çalışma Ekranına Hoşgeldın.", "İyi Çalışmalar", Button.ok, Logo.info);
            blurBitmapEffect.Radius = 0;
            g.BitmapEffect = blurBitmapEffect;

            UsernameTxt.Text = userlogin.Name;
            FillMaine();
           
        }
        void Openwinform(Form f)
        {
            Window g = this.FindName("HomeMain") as Window;
            BlurBitmapEffect blurBitmapEffect = new BlurBitmapEffect();
            blurBitmapEffect.Radius = 20;
            g.BitmapEffect = blurBitmapEffect;
            f.ShowDialog();
            blurBitmapEffect.Radius = 0;
            g.BitmapEffect = blurBitmapEffect;
        }
        public void FillMaine()
        {
            if (userlogin.Name != null)
            {
                UsernameTxt.Text = userlogin.Name;
            }
            ReminderTxt.Text = mainbll.TotalReminders(userlogin);
            monthlisale.Text = mainbll.Totalmonthlisales(userlogin) + " €";
            MonthliPayment.Text = mainbll.Total_monthli_Payment(userlogin) + " €";
            MonthlyBakiye.Text = ((Convert.ToDouble(mainbll.Totalmonthlisales(userlogin))) - (Convert.ToDouble(mainbll.Total_monthli_Payment(userlogin)))).ToString("N2") + " €";
            customersayısıtxt.Text = mainbll.Total_custumer(userlogin);
            newcustomersayısıtxt.Text = mainbll.NewCustumerİnmonth(userlogin);
            TotalStock.Text = mainbll.TotalStock() + " Stück";
                TotalStocksales.Text = mainbll.Product_sale_pices();
            int a = 0;
            if (mainbll.Reminder_ihtar(userlogin))
            {
                ihtarpic.Visibility = Visibility.Visible;
            }
            else
            {
                ihtarpic.Visibility = Visibility.Hidden;
            }

            foreach (var item in mainbll.Getuserreminder(userlogin))
            {
                if (a < 7)
                {
                    REMİNDER_UC uc = new REMİNDER_UC();
                    uc.Titletxt.Text = item.Title;
                    uc.İnfotxt.Text = item.Reminderİnfo;
                    Grid.SetColumn(uc, 0);
                    Grid.SetRow(uc,  a);
                    groupboxgrid.Children.Add(uc);
                    a++;
                }
            }
        }

        private void MusteriYonetimi_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Kunden", can.Read))
            {
                CUSTOMER_FORM c = new CUSTOMER_FORM();
                Openwinform(c);
                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void User_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Benutzer", can.Read))
            {
                USER_FORM u = new USER_FORM();
                Openwinform(u);
                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void urunyonetim_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Produkte", can.Read))
            {
                PRODUCT_FORM uf = new PRODUCT_FORM();
                Openwinform(uf);
                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void satis_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Verkauf", can.Read))
            {
                İNVOİCE_FORM2 s = new İNVOİCE_FORM2();
                Openwinform(s);
                FillMaine();

            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void aktiviti_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Aktivitäten", can.Read))
            {
                ACTİVİTY_FORM a = new ACTİVİTY_FORM();
                Openwinform(a);
                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void remainder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Erinnerungen", can.Read))
            {
                REMİNDER_FORM r = new REMİNDER_FORM();
                Openwinform(r);
                FillMaine();

            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void Smsportali_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "SMS-Portal", can.Read))
            {
                SMS_FORM s = new SMS_FORM();
                Openwinform(s);
                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void Exit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window g = this.FindName("HomeMain") as Window;
            BlurBitmapEffect blurBitmapEffect = new BlurBitmapEffect();
            blurBitmapEffect.Radius = 20;
            g.BitmapEffect = blurBitmapEffect;
            MESSAGE_BOX messagebox = new MESSAGE_BOX();
            DialogResult İ = messagebox.Show("Sind Sie sicher?", "Programm beenden?", "", Button.yesorno, Logo.warning);
            blurBitmapEffect.Radius = 0;
            g.BitmapEffect = blurBitmapEffect;



            if (İ == System.Windows.Forms.DialogResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void ayarlar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Einstellungen", can.Read))
            {
                SETİNG_FORM s = new SETİNG_FORM();
                Openwinform(s);
                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void Raporlar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ubll.Access(userlogin, "Berichte", can.Read))
            {

                FillMaine();
            }
            else
            {
                message.Show("Hinweis", "Dafür haben Sie keine Berechtigung.", "Bitte wenden Sie sich an den Administrator.", Button.ok, Logo.warning);
            }
        }

        private void cheanchuser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow w = (MainWindow)System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            w.userlogin = null;
            LOAD_FORMcs l = new LOAD_FORMcs();
            Openwinform(l);
            FillMaine();
        }

        private void Ödeme_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PAMENT_FORM p = new PAMENT_FORM();
            Openwinform(p);
            FillMaine();
        }




        #endregion

        private void clanderr_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clanderr.Height = 351 ;
        }

        private void clanderr_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clanderr.Height = 0;
        }

        private void CLOCK_UC_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            clanderr.Height = 351;
        }
    }
}
