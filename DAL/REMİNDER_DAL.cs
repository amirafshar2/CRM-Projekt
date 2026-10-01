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
    public class REMİNDER_DAL
    {
        DB db = new DB();
        public string Create(REMİNDER r, USER u)
        {
            try
            {
                r.Users = db.users.Find(u.id);
                db.reminders.Add(r);
                db.SaveChanges();
                return "Erfolgreich gespeichert.";

            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;

            }


        }
        public DataTable Read_all()
        {
            string cmd = "SELECT        dbo.REMİNDER.id, dbo.REMİNDER.Title AS [Hatırlatma Konusu], dbo.REMİNDER.Reminderİnfo AS [Hatırlatıcı Açıklaması], dbo.REMİNDER.ReminDate AS [Hatırlatma Tarihi],               dbo.REMİNDER.İsDone AS [Yapıldı mı?], dbo.USERs.UserName AS Görevli FROM            dbo.REMİNDER INNER JOIN                          dbo.USERs ON dbo.REMİNDER.Users_id = dbo.USERs.id WHERE       (dbo.REMİNDER.İsDone = 0) AND  (dbo.REMİNDER.DeletStatus = 0)  ";
            SqlConnection con = new SqlConnection(DB.ConStr);
            var adaptor = new SqlDataAdapter(cmd, con);
            var bulider = new SqlCommandBuilder(adaptor);
            var ds = new DataSet();
            adaptor.Fill(ds);
            return ds.Tables[0];


        }
        public DataTable ReminderSearch(string s)
        {
            SqlConnection con = new SqlConnection(DB.ConStr);
            SqlCommand com = new SqlCommand("dbo.ReminderSearch");
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

        public string Update(REMİNDER c, int id , USER u)
        {

            try
            {
                var q = db.reminders.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.Users = db.users.Find(u.id);
                    q.Title = c.Title;
                    q.Reminderİnfo = c.Reminderİnfo;                    
                    q.ReminDate = c.ReminDate;
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
        public string İsDone(REMİNDER c, int id)
        {

            try
            {
                var q = db.reminders.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.İsDone = true;

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
                var q = db.reminders.Where(i => i.id == id).FirstOrDefault();
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

        public REMİNDER Readbyid(int id)
        {
            var q = db.reminders.Where(i => i.id == id).FirstOrDefault();
            if (q != null)
            {
                return q;
            }
            return null;
        }

        //public UserClass readUserByReminderId(int idr)
        //{
        //    var reminder = db.reminders.Where(item => item.id == idr);
        //    if (reminder != null)
        //    {
        //        return reminder.Users;
        //    }
        //    return null;
        //}
        public void İsDone_byreminder(REMİNDER r)
        {
            var q = db.reminders.Where(i => i.id == r.id).FirstOrDefault();
            if (q != null)
            {
                q.İsDone = true;
                db.SaveChanges();
            }

        }
        public string TotalReminders()
        {
            return db.reminders.Where(i => i.DeletStatus == false && i.İsDone == false).Count().ToString();
        }
    }
}
