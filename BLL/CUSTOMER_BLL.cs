using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class CUSTOMER_BLL
    {
        CUSTOMER_DAL DAL = new CUSTOMER_DAL();

        public int GetUserId(int search)
        {
            DataTable dataTable = DAL.customer_user_id_read(search); // DAL'dan DataTable al

            int userId = 0;
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0]["User_id"] != DBNull.Value)
            {
                userId = Convert.ToInt32(dataTable.Rows[0]["User_id"]);
            }

            return userId;
        }

        public int GetUserIdformcustumer(int search)
        {         
                      
            return DAL.Userid_read_from_Customer(search);
        }

        public string create(CUSTOMER c, USER u)
        {

            if (DAL.Check(c))
            {
                return DAL.Create(c, u);
            }
            return "Ein Kunde mit dieser Firma und Telefonnummer existiert bereits.";

        }
        public DataTable readall()
        {
            return DAL.ReadAll();
        }
        public DataTable Search(string s)
        {
            return DAL.SearchCustomer(s);
        }
        public string Update(CUSTOMER c, int id, USER u)
        {
            return DAL.Update(c, id, u);
        }
        public CUSTOMER Readbyid(int id)
        {
            return DAL.Readbyid(id);
        }
        public string Delet(int id)
        {
            return DAL.Delete(id);
        }
        public List<string> Readname()
        {
            return DAL.Readname();
        }
        public CUSTOMER Readname(string s)
        {
            return DAL.Readname(s);
        }
        public string CustomerPics()
        {
            return DAL.CustomerPics();
        }
        public void CreateBakiye(CUSTOMER c, double b)
        {
            DAL.CreateBakiye(c, b);
        }
        public void Create_payment(CUSTOMER c, double p)
        {
            DAL.Create_payment(c, p);
        }
        public DataTable Read_Bakiye(int id)
        {
            return DAL.Read_Bakiye(id);
        }
        public DataTable İnvoice_Customer_Search(string s)
        {
            return DAL.İnvoice_Customer_Search(s);

        }
        public int User_id_From_Custumer_id(int cid)
        {
            return DAL.User_id_From_Custumer_id(cid);
        }
    }
}
