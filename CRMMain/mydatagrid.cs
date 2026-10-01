using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace çağdaşcivata
{
    public partial class mydatagrid : UserControl
    {
        public mydatagrid()
        {
            InitializeComponent();
            UiTheme.Apply(this);
        }
        public int id (string id)
        {
            return Convert.ToInt32(id);
        }
        private void mydatagrid_MouseClick(object sender, MouseEventArgs e)
        {
            PRODUCT_FORM w = Application.OpenForms.OfType<PRODUCT_FORM>().FirstOrDefault();


            if (w != null)
            {
                w.id = id(idlbl.Text);
            }
            w.contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
        }
        private void mydatagrid_MouseMove(object sender, MouseEventArgs e)
        {
          
        }

        private void mydatagrid_MouseLeave(object sender, EventArgs e)
        {
          
        }

        private void mydatagrid_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox2.Height = 30;
            pictureBox2.Width = 30;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Height = 20;
            pictureBox2.Width = 20;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            PRODUCT_FORM w = Application.OpenForms.OfType<PRODUCT_FORM>().FirstOrDefault();
            w.id = id(idlbl.Text);
            w.contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
        }
    }
}
