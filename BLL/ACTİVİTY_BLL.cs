using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using BE;

namespace BLL
{
    public class ACTİVİTY_BLL
    {
         ACTİVİTY_DAL dal = new  DAL.ACTİVİTY_DAL();
        public string Create(ACTİVİTY a, USER u, CUSTOMER c, ACTİVİTY_CATEGORY ac)
        {
            return dal.Create(a, u, c, ac);
        }

        public DataTable Read_All()
        {
            return dal.Read_All();
        }
        public string Delete(int id)
        {
            return dal.Delete(id);
        }


        public string Update(int id, string title, string info)
        {
            return dal.Update(id, title, info);
        }
        public ACTİVİTY Read_byid(int id)
        {
            return dal.Read_byid(id);
        }
        public string Activitypics()
        {
            return dal.Activitypics();

        }

        public DataTable Search(string s)
        {
            return dal.Search(s);
        }
    }
}
