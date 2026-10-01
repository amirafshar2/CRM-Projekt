using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using System.Dynamic;

namespace DAL
{
    public class İNVOİCE_DAL
    {
        DB DB = new DB();
        Random rnd = new Random();

        public string Create(İNVOİCE i, CUSTOMER c, List<PRODUCT> p, USER u)
        {
            try
            {

                i.User = DB.users.Find(u.id);
                i.Customer = DB.Customers.Find(c.id);
                //i.Customer.Bakiye =  i.Customer.Bakiye + i.TotalPrice;
                string s = rnd.Next(1000000).ToString();
                var q = DB.invoices.Where(f => f.invoiceNumber == s);

                while (q.Count() > 0)
                {
                    s = rnd.Next(1000000).ToString();
                }
                i.invoiceNumber = s;
                // Verkaufte Positionen werden als eigene Zeilen gespeichert (SaledPices > 0).
                // Vorher merken, von welchem Katalogprodukt wie viel verkauft wurde.
                var verkauft = new List<KeyValuePair<int, int>>();
                foreach (var item in p)
                {
                    verkauft.Add(new KeyValuePair<int, int>(item.id, item.SaledPices));
                    item.id = 0;
                    item.Stock = 0;
                    i.products.Add(item);
                }

                DB.invoices.Add(i);
                DB.SaveChanges();

                // Lagerbestand der Katalogprodukte reduzieren
                foreach (var v in verkauft)
                {
                    DB.Database.ExecuteSqlCommand("UPDATE dbo.PRODUCTs SET Stock = CASE WHEN Stock > @p1 THEN Stock - @p1 ELSE 0 END WHERE id = @p0 AND SaledPices = 0",
                        new SqlParameter("@p0", v.Key), new SqlParameter("@p1", v.Value));
                }
                return "Bestellung angelegt.";
            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;
            }
        }
        public DataTable readall()
        {
            string cmd = "SELECT dbo.İNVOİCE.id, dbo.İNVOİCE.invoiceNumber AS [Fiş numarası], dbo.İNVOİCE.RegDate AS [Kayit tarihi], dbo.CUSTOMERs.Name AS [Müşteri Adı], dbo.CUSTOMERs.Company AS Firma, dbo.İNVOİCE.TotalPrice AS [Toplam Tutar], ISNULL(dbo.İNVOİCE.Bakiye, dbo.İNVOİCE.TotalPrice) AS [Offen], dbo.USERs.Name AS [Satiş Temsilcisi] FROM dbo.İNVOİCE INNER JOIN dbo.CUSTOMERs ON dbo.İNVOİCE.Customer_id = dbo.CUSTOMERs.id INNER JOIN dbo.USERs ON dbo.İNVOİCE.User_id = dbo.USERs.id WHERE (dbo.İNVOİCE.Deletestatus = 0) ORDER BY dbo.İNVOİCE.id DESC";
            SqlConnection con = new SqlConnection(DB.ConStr);
            var adptor = new SqlDataAdapter(cmd, con);
            var bulid = new SqlCommandBuilder(adptor);
            var ds = new DataSet();
            adptor.Fill(ds);
            return ds.Tables[0];
        }
        /// <summary>Rechnung mit Positionen, Kunde und Betreuer (für Druck und Zahlung)</summary>
        public İNVOİCE read_full(int id)
        {
            return DB.invoices.Include("products").Include("Customer").Include("User").Where(i => i.id == id && i.Deletestatus == false).FirstOrDefault();
        }

        /// <summary>Zahlung zu einer Rechnung buchen (Teil- oder Restzahlung)</summary>
        public string AddPayment(int id, double betrag, PAYMENT_METHOD art, DateTime datum)
        {
            try
            {
                var q = DB.invoices.Where(e => e.id == id && e.Deletestatus == false).FirstOrDefault();
                if (q == null) return "Rechnung nicht gefunden.";
                q.ÖdemeTurarı = (q.ÖdemeTurarı ?? 0) + betrag;
                q.Bakiye = q.TotalPrice - q.ÖdemeTurarı;
                q.ödemeŞekli = art;
                q.ÖdemeDate = datum.Date;
                if (q.Bakiye <= 0.005)
                {
                    q.İsCheckedOut = true;
                    q.CeackOutDate = datum.Date;
                }
                DB.SaveChanges();
                return "Zahlung gespeichert.";
            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;
            }
        }

        public İNVOİCE read_by_id(int id)
        {
            var q = DB.invoices.Where(i => i.id == id).FirstOrDefault();
            if (q != null)
            {
                return q;
            }
            return null;
        }
        public string update(İNVOİCE i, CUSTOMER c, List<PRODUCT> p, USER u, int id)
        {
            try
            {
                var q = DB.invoices.Where(e => e.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.TotalPrice = i.TotalPrice;
                    q.İsCheckedOut = i.İsCheckedOut;
                    if (i.CeackOutDate != null)
                    {
                        q.CeackOutDate = i.CeackOutDate;
                    }
                    q.Customer = DB.Customers.Find(c.id);
                    q.User = DB.users.Find(u.id);
                    foreach (var item in p)
                    {
                        q.products.Add(item);
                    }
                    DB.SaveChanges();
                    return "Änderungen gespeichert.";
                }
                return "Rechnung nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Ändern ist ein Fehler aufgetreten:\n" + e.Message; ;

            }
        }
        public string Delete(int id)
        {
            try
            {
                var q = DB.invoices.Include("products").Include("Customer").Where(e => e.id == id).FirstOrDefault();
                if (q != null && !q.Deletestatus)
                {
                    q.Deletestatus = true;
                    // Kundensaldo zurückbuchen
                    if (q.Customer != null)
                    {
                        q.Customer.Bakiye -= q.TotalPrice;
                        q.Customer.Alacak -= q.ÖdemeTurarı ?? 0;
                    }
                    DB.SaveChanges();
                    // Bestand zurückbuchen (Katalogprodukt mit gleichen Merkmalen)
                    foreach (var item in q.products)
                    {
                        DB.Database.ExecuteSqlCommand("UPDATE TOP (1) dbo.PRODUCTs SET Stock = Stock + @n WHERE DeletStatus = 0 AND SaledPices = 0 AND Category = @c AND Name = @na AND ISNULL(Cap,'') = @cap AND ISNULL(Boy,'') = @boy AND ISNULL(Kaplama,'') = @k",
                            new SqlParameter("@n", item.SaledPices), new SqlParameter("@c", item.Category ?? ""), new SqlParameter("@na", item.Name ?? ""),
                            new SqlParameter("@cap", item.Cap ?? ""), new SqlParameter("@boy", item.Boy ?? ""), new SqlParameter("@k", item.Kaplama ?? ""));
                    }
                    return "Erfolgreich gelöscht.";
                }
                return "Rechnung nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Löschen ist ein Fehler aufgetreten:\n" + e.Message; ;

            }
        }
        public string Checkouute_Payment(int id)
        {
            var q = DB.invoices.Where(i => i.id == id).FirstOrDefault();
            if (q != null && q.İsCheckedOut != true)
            {
                q.İsCheckedOut = true;
                q.CeackOutDate = DateTime.Now.Date;
                DB.SaveChanges();
                return "Als bezahlt markiert.";
            }
            return "Bestellung nicht gefunden.";

        }
        public DataTable SearchCustomer(string s)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.Searchinvoice");
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
        public string Readinvnum()
        {
            var q = DB.invoices.OrderByDescending(i => i.id).FirstOrDefault();
            return q.invoiceNumber;
        }
        public string Payed(int id)
        {
            try
            {
                var q = DB.invoices.Where(e => e.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.İsCheckedOut = true;
                    q.CeackOutDate = DateTime.Now.Date;

                    DB.SaveChanges();
                    return "Zahlung gespeichert.";
                }
                return "Rechnung nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Ändern ist ein Fehler aufgetreten:\n" + e.Message; ;

            }
        }
        public string invoicepics()
        {
            return DB.invoices.Where(i => i.Deletestatus == false).Count().ToString();
        }
        public string İnvoicenum()
        {
           var q = DB.invoices.OrderByDescending ( i => i.id).FirstOrDefault();
            return q.invoiceNumber;

        }
        //public string update_sale_pics( List <PRODUCT> p , int s) 
        //{
        //  foreach
        //}
        public CUSTOMER Get_customer_Bay_invoice_İd(int id)
        {
            return DB.invoices.Where(i=>i.id == id).Select(i=>i.Customer).FirstOrDefault();
        }
        public DataTable Get_İnvoice_Product(int id)
        {

            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.GetİnvoiceProduct");
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
    }
}
