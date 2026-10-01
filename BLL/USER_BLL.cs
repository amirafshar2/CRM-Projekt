using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class USER_BLL
    {
        USER_DAL dAL = new USER_DAL();
        public List<string> Readusername()
        {
            return dAL.Readusername();
        }
        public USER Readuser(string s)
        {
            return dAL.Readuser(s);
        }
        private string encode(string pass)
        {
            byte[] onedata_baye = new byte[pass.Length];
            onedata_baye = System.Text.UTF8Encoding.UTF8.GetBytes(pass);
            string encodedata = Convert.ToBase64String(onedata_baye);
            return encodedata;

        }
        private string Decode(string pass)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder usft8decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(pass);
            int charCount = usft8decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_Char = new char[charCount];
            usft8decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_Char, 0);
            string result = new string(decoded_Char);
            return result;

        }
        public string Create(USER u, USER_GROUP ug, bool reminder)
        {
            if (dAL.Cheack(u))
            {
                u.Password = encode(u.Password);
                return dAL.Create(u, ug, reminder);

            }
            return "Dieser Benutzername ist bereits vergeben.";
        }
        public DataTable ReadAll()
        {
            return dAL.ReadAll();
        }
        public USER Read_By_İd(int id)
        {
            return dAL.Read_By_İd(id);
        }
        public string update(int id, USER u, USER_GROUP ug)
        {
            u.Password = encode(u.Password);
            return dAL.update(id, u, ug);
        }
        public string Delet(int id)
        {
            return dAL.Delet(id);
        }
        public bool İsregestered()
        {
            return dAL.İsregestered();
        }
        public USER Login(string Uname, string pas, bool reminder)
        {
            return dAL.Login(Uname, encode(pas), reminder);
        }
        public int get_usergroupid_by_userid(int userid)
        {
            return dAL.get_usergroupid_by_userid(userid);
            //DataTable dataTable = dAL.get_usergroupid_by_userid(userid); // DAL'dan DataTable al

            //int userId = 0;
            //if (dataTable.Rows.Count > 0 && dataTable.Rows[0]["userGroup_id"] != DBNull.Value)
            //{
            //    userId = Convert.ToInt32(dataTable.Rows[0]["userGroup_id"]);
            //}

            //return userId;
        }
        public bool Access(USER u, string s, can a)
        {
            return dAL.Access(u, s, a);
        }
        public List<string> user_save_login()
        {
            return dAL.user_save_login();
        }
        public string user_pass_login(string username)
        {
            if (dAL.user_pass_login(username) != null)
            {
                return Decode(dAL.user_pass_login(username));
            }
            return "";
        }
        public void userpass_delet()
        {
            dAL.userpass_delet();

        }
        public List<USER> Gwt_All_User()
        {
            return dAL.Gwt_All_User();

        }
    }
}
