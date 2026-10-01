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
    public class USER_DAL
    {
        DB DB = new DB();
        public string Create(USER u, USER_GROUP ug, bool reminder)
        {
            try
            {
                u.userGroup = DB.usergrups.Find(ug.id);
                DB.users.Add(u);
                if (reminder)
                {
                    USER_PAS_SAVE ups = new USER_PAS_SAVE();
                    ups.username = u.UserName;
                    ups.password = u.Password;
                    DB.userpass_Saves.Add(ups);
                }

                DB.SaveChanges();
                return "Erfolgreich gespeichert.";

            }
            catch (Exception e)
            {
                return "Beim Speichern ist ein Fehler aufgetreten:\n" + e.Message;

            }

        }
        public bool Cheack(USER u)
        {
            // true = Benutzername ist noch frei
            return !DB.users.Any(i => i.UserName == u.UserName && i.DeletStatus == false);
        }
        public DataTable ReadAll()
        {
            string cmd = "SELECT        id, Name, UserName, Password, Pic, Status, Regtime FROM            dbo.USERs WHERE        (DeletStatus = 0)";
            SqlConnection con = new SqlConnection(DB.ConStr);
            var adaptor = new SqlDataAdapter(cmd, con);
            var bulider = new SqlCommandBuilder(adaptor);
            var ds = new DataSet();
            adaptor.Fill(ds);
            return ds.Tables[0];
        }
        public USER Read_By_İd(int id)
        {
            var q = DB.users.Where(i => i.id == id).FirstOrDefault();
            if (q != null)
            {
                return q;
            }
            return null;
        }
        public string update(int id, USER u, USER_GROUP ug)
        {
            try
            {

                var q = DB.users.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.userGroup = DB.usergrups.Find(ug.id); // Rolle wird jetzt wirklich geändert
                    q.Name = u.Name;
                    q.UserName = u.UserName;
                    q.Password = u.Password;
                    q.Status = u.Status;
                    q.Adress = u.Adress;
                    q.TC = u.TC;
                    q.E_mail= u.E_mail;
                    q.Pic = u.Pic;
                    q.PhoneNumber= u.PhoneNumber;
                    DB.SaveChanges();
                    return "Änderungen gespeichert.";
                }
                return "Benutzer nicht gefunden.";
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
                var q = DB.users.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeletStatus = true;
                    DB.SaveChanges();
                    return "Erfolgreich gelöscht.";
                }
                return "Benutzer nicht gefunden.";
            }
            catch (Exception e)
            {
                return "Beim Löschen ist ein Fehler aufgetreten:\n" + e.Message;

            }

        }
        public USER Readuser(string s)
        {
            return DB.users.Where(i => i.UserName == s && i.DeletStatus == false).FirstOrDefault();
        }
        public List<string> Readusername()
        {
            return DB.users.Where(i => i.DeletStatus == false).Select(i => i.UserName).ToList();
        }
        public bool İsregestered()
        {
            var q = DB.users.Count() > 0;
            if (q)
            {
                return true;
            }
            return false;
        }
        public USER Login(string Uname, string pas, bool reminder)
        {
            var q = DB.users.Include("UserGroup").Where(i => i.UserName == Uname && i.Password == pas && i.DeletStatus == false).FirstOrDefault();
            if (q != null)
            {
                if (reminder)
                {
                    var s = DB.userpass_Saves.Where(e => e.username == Uname && e.password == pas).FirstOrDefault();
                    if (s == null)
                    {
                        USER_PAS_SAVE ups = new USER_PAS_SAVE();
                        ups.username = Uname;
                        ups.password = pas;
                        DB.userpass_Saves.Add(ups);
                        DB.SaveChanges();
                    }
                }
                return q;
            }
            return null;
        }
        public int get_usergroupid_by_userid(int userid)
        {
            return DB.users.Include("USER_GROUP").Where(i=> i.id == userid).Select(i=>i.userGroup.id).FirstOrDefault();
            
        }

        public bool Access(USER u, string s, can a)
        {
            USER_GROUP ug = DB.usergrups.Include("Roles").Where(i => i.id == u.userGroup.id).FirstOrDefault();
            if (ug == null) return false;
            USER_ACCESS_ROLE ura = ug.Roles.Where(z => z.Section == s).FirstOrDefault();
            if (ura == null) return false; // Bereich nicht vorhanden -> kein Zugriff
            if (a == can.Read)
            {
                return ura.CanRead;
            }
            else if (a == can.Create)
            {
                return ura.CanCreate;
            }

            else if (a == can.Update)
            {
                return ura.CanUpdate;
            }
            else
            {
                return ura.CamDelete;
            }
        }
        public List<string> user_save_login()
        {
            return DB.userpass_Saves.Select(i => i.username).ToList();
        }
        public string user_pass_login(string username)
        {
            return DB.userpass_Saves.Where(A => A.username == username).Select(A => A.password).FirstOrDefault();
        }
        public void userpass_delet()
        {
            var ud = DB.userpass_Saves.ToList();
            foreach (var item in ud)
            {
                DB.userpass_Saves.Remove(item);
                DB.SaveChanges();
            }

        }
        public List<USER> Gwt_All_User()
        {
            return  DB.users.Where(i=>i.DeletStatus==false).ToList();
          
        }

       
    }
   
}