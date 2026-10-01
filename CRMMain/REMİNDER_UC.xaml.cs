using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace çağdaşcivata
{
    /// <summary>
    /// Interaction logic for REMİNDER_UC.xaml
    /// </summary>
    public partial class REMİNDER_UC : System.Windows.Controls.UserControl
    {
        public REMİNDER_UC()
        {
            InitializeComponent();
        }
        REMİNDER_BLL bLL = new REMİNDER_BLL();
        MAİN_BLL MainBLL = new MAİN_BLL();
        MESSAGE_BOX message = new MESSAGE_BOX();

    

        private void İnfoimg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            İnfoimg.Width = 34;
        }

        private void İnfoimg_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            İnfoimg.Width = 32;
        }

        private void infogrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            infogrid.Height = 42;
        }

        private void infogrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            infogrid.Height = 40;
        }

        private void İnfoimg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DialogResult.Yes == message.Show("Erledigt?", "Erinnerung schließen?", "", Button.yesorno, Logo.info)) ;
            {
                bLL.İsDone_byreminder(MainBLL.GetReminder_bayinfoandtitle(Titletxt.Text, İnfotxt.Text));
                MainWindow w = (MainWindow)System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                w.groupboxgrid.Children.Clear();
                w.FillMaine();
            }
        }
    }
}
