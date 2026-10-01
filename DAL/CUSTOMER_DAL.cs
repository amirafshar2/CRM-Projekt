using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class CUSTOMER_DAL
    {
        DB db = new DB();
        public string Create(CUSTOMER c, USER u)
        {
            try
            {
                c.User = db.users.Find(u.id);
                db.Customers.Add(c);
                db.SaveChanges();
                return "Erfolgreich gespeichert.";
            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;
            }
        }
        public bool Check(CUSTOMER c)
        {
            var q = db.Customers.Where(i => i.Company == c.Company & i.Phone == c.Phone & i.DeletStatus == false);
            if (q.Count() == 0)
            {
                return true;
            }
            return false;
        }
        public DataTable ReadAll()
        {
            string cmd = "SELECT        id AS İd, Name AS İsim, Company AS Firma, Phone AS [Telefon No], Email AS [Email Adres], Regdate AS [Kayıt Tarihi]\r\nFROM            dbo.CUSTOMERs\r\nWHERE        (DeletStatus = 0)";
            SqlConnection con = new SqlConnection(DB.ConStr);
            var sqladapter = new SqlDataAdapter(cmd, con);
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];
        }
        public DataTable SearchCustomer(string s)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.SearchCustumer");
            com.Parameters.AddWithValue("@Search", s);
            com.Connection = con;
            com.CommandType = CommandType.StoredProcedure;

            var sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = com;
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];

        }
        public string Update(CUSTOMER c, int id, USER u)
        {

            try
            {
                var q = db.Customers.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.User = db.users.Find(u.id);
                    q.Name = c.Name;
                    q.Company = c.Company;
                    q.Phone = c.Phone;
                    q.Email = c.Email;
                    // Anlagedatum bleibt beim Bearbeiten erhalten
                    q.Adress = c.Adress;
                    q.vergidairesi_bilgileri = c.vergidairesi_bilgileri;
                    db.SaveChanges();
                    return "Änderungen gespeichert.";
                }
                return "Eintrag nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Ändern ist ein Fehler aufgetreten:\n" + e.Message;

            }
        }
        public string Delete(int id)
        {
            try
            {
                var q = db.Customers.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeletStatus = true;
                    db.SaveChanges();
                    return "Erfolgreich gelöscht.";
                }
                return "Eintrag nicht gefunden.";

            }
            catch (Exception e)
            {
                return "Beim Löschen ist ein Fehler aufgetreten:\n" + e.Message;
            }
        }
        public CUSTOMER Readbyid(int id)
        {
            var q = db.Customers.Where(i => i.id == id).FirstOrDefault();
            if (q != null)
            {
                return q;
            }
            return null;
        }
        public List<string> Readname()
        {
            return db.Customers.Where(i => i.DeletStatus == false).Select(i => i.Company).ToList();
        }
        public CUSTOMER Readname(string s)
        {
            return db.Customers.Where(i => i.Company == s && i.DeletStatus == false).FirstOrDefault();
        }
        //public UserClass customer_user_id_read (Customerclass c)
        //{
        //    if (c.User != null)
        //    {
        //        return c.User ;
        //    }
        //    return null;
        //}
        public DataTable customer_user_id_read(int s)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.customer_user_id_read");
            com.Parameters.AddWithValue("@Search", s);
            com.Connection = con;
            com.CommandType = CommandType.StoredProcedure;

            var sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = com;
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];

        }
        public int User_id_From_Custumer_id(int cid)
        {
            return db.Customers.Include("users").Where(i => i.id == cid).Select(i => i.User.id).FirstOrDefault();
        }
        public int Userid_read_from_Customer(int s)
        {
            return db.Customers.Include("users").Where(i=>i.id  == s).Select(i=> i.User.id).FirstOrDefault();
            //    SqlConnection con = new SqlConnection(DB.ConStr);
            //    SqlCommand com = new SqlCommand("dbo.Userid_read_from_Customer");
            //    com.Parameters.AddWithValue("@Search", s);
            //    com.Connection = con;
            //    com.CommandType = CommandType.StoredProcedure;
            //    var sqladapter = new SqlDataAdapter();
            //    sqladapter.SelectCommand = com;
            //    var commandbuilder = new SqlCommandBuilder(sqladapter);
            //    var ds = new DataSet();
            //    sqladapter.Fill(ds);
            //    return ds.Tables[0];

        }
        public string CustomerPics()
        {
            return db.Customers.Where(i => i.DeletStatus == false).Count().ToString();
        }
        public void CreateBakiye(CUSTOMER c, double b)
        {
            var q = db.Customers.Where(i => i.id == c.id).FirstOrDefault();
            if (q != null)
            {
                q.Bakiye = q.Bakiye + b;
                db.SaveChanges();

            }
        }
        public void Create_payment(CUSTOMER c, double p)
        {
            var q = db.Customers.Where(i => i.id == c.id).FirstOrDefault();
            if (q != null)
            {
                q.Alacak = q.Alacak + p;
                db.SaveChanges();
            }
        }
        public DataTable Read_Bakiye(int id)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.Custumer_Bakiye_Search");
            com.Parameters.AddWithValue("@Search", id);
            com.Connection = con;
            com.CommandType = CommandType.StoredProcedure;
            var sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = com;
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];
        }
        
        public DataTable İnvoice_Customer_Search(string s)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.İnvoice_Customer_Search");
            com.Parameters.AddWithValue("@Search", s);
            com.Connection = con;
            com.CommandType = CommandType.StoredProcedure;

            var sqladapter = new SqlDataAdapter();
            sqladapter.SelectCommand = com;
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];

        }
    }
}
