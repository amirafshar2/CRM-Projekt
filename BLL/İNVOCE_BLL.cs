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
    public class İNVOCE_BLL
    {
        İNVOİCE_DAL DAL = new İNVOİCE_DAL();
        public string Create(İNVOİCE i, CUSTOMER c, List<PRODUCT> p, USER u)
        {
            return DAL.Create(i, c, p, u);
        }
        public DataTable readall()
        {
            return DAL.readall();
        }
        public string update(İNVOİCE i, CUSTOMER c, List<PRODUCT> p, USER u, int id)
        {
            return DAL.update(i, c, p, u, id);
        }
        public string Delete(int id)
        {
            return DAL.Delete(id);
        }
        public İNVOİCE read_full(int id)
        {
            return DAL.read_full(id);
        }
        public string AddPayment(int id, double betrag, PAYMENT_METHOD art, DateTime datum)
        {
            return DAL.AddPayment(id, betrag, art, datum);
        }
        public İNVOİCE read_by_id(int id)
        {
            return DAL.read_by_id(id);
        }
        public DataTable SearchCustomer(string s)
        {
            return DAL.SearchCustomer(s);
        }
        public string Readinvnum()
        {
            return DAL.Readinvnum();
        }
        public string Payed(int id)
        {
            return DAL.Payed(id);
        }
        public string invoicepics()
        {
            return DAL.invoicepics();
        }
        public string İnvoicenum()
        {
            
            return DAL.İnvoicenum();

        }
        public CUSTOMER Get_customer_Bay_invoice_İd(int id)
        {
            return DAL.Get_customer_Bay_invoice_İd(id);
        }

        public  DataTable Get_İnvoice_Product(int id)
        {
           
            return DAL.Get_İnvoice_Product(id);
        }
    }
}
