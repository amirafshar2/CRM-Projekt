using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class İNVOİCE
    {
        public İNVOİCE()
        {
            Deletestatus = false;
        }
        public int id { get; set; }

        public string invoiceNumber { get; set; }
        public DateTime RegDate { get; set; }
        public Nullable<DateTime> CeackOutDate { get; set; }
        public bool İsCheckedOut { get; set; }
        public double TotalPrice { get; set; }
        public Nullable<double> ÖdemeTurarı { get; set; }
        public Nullable<DateTime> ÖdemeDate { get; set; }
        public Nullable<PAYMENT_METHOD> ödemeŞekli { set; get; }
        public Nullable<DateTime> VadeDate { get; set; }
        public Nullable<double> Bakiye { get; set; }
        public CUSTOMER Customer { get; set; }
        public bool Deletestatus { get; set; }

        public List<PRODUCT> products { get; set; } = new List<PRODUCT>();
        public USER User { get; set; }
    }
}
public enum PAYMENT_METHOD
{
    Bar,
    Kreditkarte,
    Kreditkarte_Raten,
    Auf_Ziel,
    Scheck
}
