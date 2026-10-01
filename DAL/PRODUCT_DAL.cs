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
    public class PRODUCT_DAL
    {

        DB db = new DB();



        public Random Random = new Random(1000000);
        public string Create(PRODUCT c)
        {

            try
            {
                string s = Random.Next(1000000).ToString();
                var q = db.products.Where(z => z.Product_cod == s);
                while (q.Count() > 0)
                {
                    s = Random.Next(1000000).ToString();
                }
                c.Product_cod = s;
                db.products.Add(c);
                db.SaveChanges();
                return "Erfolgreich gespeichert.";
            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;
            }
        }
        public DataTable ReadAll()
        {
            string cmd = "SELECT        id, Category AS Ürün, Name AS [Ürün Adı], Cap AS Çap, Boy, Quality AS Kalite, Kaplama, DINnumber AS DIN, Stock AS Stok, Price AS Fiyat, BrandName AS Marka, Packing AS Paket, picture AS Görsel,                            Feature AS Özellik  FROM            dbo.PRODUCTs WHERE        (DeletStatus = 0) AND (SaledPices = 0) ";
            SqlConnection con = new SqlConnection(DB.ConStr);
            var sqladapter = new SqlDataAdapter(cmd, con);
            var commandbuilder = new SqlCommandBuilder(sqladapter);
            var ds = new DataSet();
            sqladapter.Fill(ds);
            return ds.Tables[0];

        }
        public List<PRODUCT> Readall()
        {
            return db.products.Where(i=>i.DeletStatus == false && i.SaledPices==0 ).ToList();
        }
        public PRODUCT Readbyid(int id)
        {
            var q = db.products.Where(i => i.id == id && i.DeletStatus == false && i.SaledPices== 0).FirstOrDefault();
            if (q != null)
            {
                return q;
            }
            return null;
        }

        public DataTable Search(String s)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.SearchProduct");
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

        public string update(PRODUCT p, int id)
        {
            try
            {
                var q = db.products.Where(x => x.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.Category = p.Category;
                    q.Name = p.Name;
                    q.DINnumber = p.DINnumber;
                    q.Cap = p.Cap;
                    q.Boy = p.Boy;
                    q.Quality = p.Quality;
                    q.Kaplama = p.Kaplama;
                    q.Price = p.Price;
                    q.Stock = p.Stock;
                    q.Packing = p.Packing;
                    q.Feature = p.Feature;
                    q.BrandName = p.BrandName;
                    q.DeletStatus = p.DeletStatus;
                    q.picture = p.picture;
                    db.SaveChanges();
                    return "Änderungen gespeichert.";
                }
                return "Produkt nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Ändern ist ein Fehler aufgetreten:\n" + e.Message;
            }


        }
        public string Delet(int id)
        {
            try
            {
                var q = db.products.Where(x => x.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeletStatus = true;
                    db.SaveChanges();
                    return "Erfolgreich gelöscht.";
                }
                return "Produkt nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Löschen ist ein Fehler aufgetreten:\n" + e.Message;
            }
        }

        public PRODUCT Chenge_price(int id, double price)
        {
            var q = db.products.Where(i => i.id == id).FirstOrDefault();
            if (q != null)
            {
                q.Price = price;
                db.SaveChanges();
            }
            return null;
        }

        public string Productpics()
        {
            return db.products.Where(i => i.DeletStatus == false && i.SaledPices ==0).Count().ToString();
        }
        public string check_pic(string c , string n , string k  )
        {
            var q = db.products.Where(i => i.Category == c && i.Name == n && i.Kaplama == k ).FirstOrDefault();
            if (q != null)
            {
                return q.picture;
            }
            return null;
        }
    }
}
