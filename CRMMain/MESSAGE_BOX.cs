using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;

using System.IO;

namespace çağdaşcivata
{
    public class MESSAGE_BOX
    {
        public DialogResult Show(string Title, string Message, string Eror, Button Butone, Logo Logo)
        {
            MESSAGE_BOX_FORM m = new MESSAGE_BOX_FORM();
            if (Logo == Logo.info)
            {
                m.BackColor = Color.FromArgb(33, 37, 41);
                m.label1.Text = Title;
                m.label1.ForeColor = Color.White;
                m.label2.ForeColor = Color.White;
                m.label3.ForeColor = Color.White;
                m.label2.Text = Message;
                m.label3.Text = Eror;
              

                if (Butone == Button.yesorno)
                {
                    m.label4.Text = "Ja";
                    m.label5.Text = "Nein";

                }

                else
                {
                    m.label4.Visible = false;
                    m.label5.Text = "OK";
                }


                m.ShowDialog();
                return m.DialogResult;

            }
            else if (Logo == Logo.warning)
            {
                m.BackColor = Color.FromArgb(102, 7, 8);
                m.label1.ForeColor = Color.White;
                m.label2.ForeColor = Color.White;
                m.label3.ForeColor = Color.White;
                m.label1.Text = Title;
                m.label2.Text = Message;
                m.label3.Text = Eror;
          
                if (Butone == Button.yesorno)
                {
                    m.label4.Text = "Ja";
                    m.label5.Text = "Nein";
                }
                else
                {
                    m.label4.Visible = false;
                    m.label5.Text = "OK";
                }
                m.ShowDialog();
                return m.DialogResult;
            }
            else
            {
                m.BackColor = Color.FromArgb(33, 37, 41);
                m.label1.Text = Title;
                m.label1.ForeColor = Color.White;
                m.label2.ForeColor = Color.White;
                m.label3.ForeColor = Color.White;
                m.label2.Text = Message;
                m.label3.Text = Eror;
                m.pictureBox1.Image = Image.FromFile(Path.Combine(System.Windows.Forms.Application.StartupPath, "image", "icons8-info-500.png"));
                m.label4.Text = "";
                m.label5.Text = "";
                m.ShowDialog();
                Timer timer1 = new Timer();
                timer1.Interval = 2;
                timer1.Tick += timertick;
                timer1.Start();
                return DialogResult.Yes;
                void timertick(object sender, EventArgs e)
                {
                    m.Close();
                    timer1.Stop();
                }
            }
        }

        //public void wpfShow(string Title, string Message, string Eror, MType Butone, MLogo Logo)
        //{
        //    mesageboxformwpf m = new mesageboxformwpf();
        //    if (Logo == MLogo.info)
        //    {
        //        m.Titr.Content = Title;
        //        m.info.Content = Message;
        //        m.eror.Content = Eror;
        //        //m.image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/icons8-info-500.png"));

        //        if (Butone == MType.yesorno)
        //        {
        //            m.evet.Content = "Ja";
        //            m.hayır.Content = "Nein";

        //        }

        //        else
        //        {
        //            m.hayır.Visibility = Visibility.Collapsed;
        //            m.evet.Content = "OK";
        //        }


        //        m.ShowDialog();


        //    }
        //    else
        //    {
        //        m.Titr.Content = Title;
        //        m.info.Content = Message;
        //        m.eror.Content = Eror;
        //        //m.image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/icons8-warning-100.png"));
        //        if (Butone == MType.yesorno)
        //        {
        //            m.evet.Content = "Ja";
        //            m.hayır.Content = "Nein";
        //        }
        //        else
        //        {
        //            m.hayır.Visibility = Visibility.Collapsed;
        //            m.evet.Content = "OK";
        //        }
        //        m.ShowDialog();

        //    }
        //}



    }
    public enum Button
    {
        yesorno,
        ok

    }
    public enum Logo
    {
        info,
        warning,
      
    }
}