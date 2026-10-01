using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class USER_GROUP_DAL
    {
        DB db = new DB();
        public string Create(USER_GROUP u)
        {
            try
            {
                db.usergrups.Add(u);
                db.SaveChanges();
                return "Erfolgreich gespeichert.";
            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;

            }

        }
        public DataTable GetAll()
        {
            string cmd = "SELECT        id, Title AS [Yetki Ünvanı] FROM            dbo.USER_GROUP  WHERE        (DeleteStatus = 0)";
            SqlConnection con = new SqlConnection(DB.ConStr);
            var adaptor = new SqlDataAdapter(cmd, con);
            var bulider = new SqlCommandBuilder(adaptor);
            var ds = new DataSet();
            adaptor.Fill(ds);
            return ds.Tables[0];
        }
        public USER_GROUP Get(int id)
        {
            return db.usergrups.SingleOrDefault(u => u.id == id);
        }
        public USER_GROUP getug_bytitle(string s)
        {
            return db.usergrups.FirstOrDefault(ı => ı.Title == s && ı.DeleteStatus == false);
        }
        public List<string> GetTitle()
        {
            return db.usergrups.Where(i => i.DeleteStatus == false).Select(i => i.Title).ToList();
        }
        public string delete(int id)
        {

            try
            {
                var q = db.usergrups.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                }
                return "Erfolgreich gelöscht.";
            }
            catch (Exception e)
            {
                return "Beim Löschen ist ein Fehler aufgetreten:\n" + e.Message;

            }
        }
    }
}
