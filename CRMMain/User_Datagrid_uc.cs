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
    public partial class User_Datagrid_uc : UserControl
    {
        public User_Datagrid_uc()
        {
            InitializeComponent();
            UiTheme.Apply(this);
        }
        public int id(string id)
        {
            return Convert.ToInt32(id);
        }

        private void User_Datagrid_uc_Load(object sender, EventArgs e)
        {
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void User_Datagrid_uc_Click(object sender, EventArgs e)
        {
            USER_FORM w = Application.OpenForms.OfType<USER_FORM>().FirstOrDefault();
            w.id = id(idlbl.Text);
            w.contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
        }
    }
}
