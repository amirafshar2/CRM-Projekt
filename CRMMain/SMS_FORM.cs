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
    public partial class SMS_FORM : Form
    {
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
        public SMS_FORM()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            // In der Demo ist kein SMS-Anbieter angebunden – Hinweis statt stiller Schaltflächen
            button1.Click += Demo_Senden;
            button2.Click += Demo_Senden;
            button4.Click += Demo_Senden;
            Load += (s, e) => HelpHint.Set(this, "Demo-Version: Der SMS-Versand ist nicht angebunden.");
        }

        private void Demo_Senden(object sender, EventArgs e)
        {
            new MESSAGE_BOX().Show("SMS-Portal", "In der Demo-Version ist kein SMS-Anbieter angebunden.", "Es wurde keine Nachricht versendet.", Button.ok, Logo.info);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
