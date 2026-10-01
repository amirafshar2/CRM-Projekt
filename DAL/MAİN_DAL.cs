using BE;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class MAİN_DAL
    {
        DB DB = new DB();

        REMİNDER reminder = new REMİNDER();





        // Abrechnungsmonat: jeweils vom 10. bis zum 10. des Folgemonats
        // (vorher Absturz im Januar, weil Monat 0 berechnet wurde)
        static void Period(out DateTime first, out DateTime last)
        {
            DateTime today = DateTime.Today;
            DateTime start = today.Day <= 10 ? today.AddMonths(-1) : today;
            first = new DateTime(start.Year, start.Month, 10);
            last = first.AddMonths(1);
        }

        public string TotalReminders(USER u)
        {
            return DB.reminders.Where(i => i.Users.id == u.id && i.DeletStatus == false && i.İsDone == false).Count().ToString();
        }
        public string Totalmonthlisales(USER u)
        {
            DateTime FirstDayOfTheThisMonth;
            DateTime LastDayOfTheThisMonth;
            Period(out FirstDayOfTheThisMonth, out LastDayOfTheThisMonth);

            var q = DB.invoices.Where(i => i.User.id == u.id && i.Deletestatus == false && i.RegDate >= FirstDayOfTheThisMonth && i.RegDate < LastDayOfTheThisMonth).Sum(i => (double?)i.TotalPrice);
            if (q.HasValue)
            {
                return q.Value.ToString("N2");
            }
            return "0,00";

        }
        public string Total_monthli_Payment(USER u)
        {
            DateTime FirstDayOfTheThisMonth;
            DateTime LastDayOfTheThisMonth;
            Period(out FirstDayOfTheThisMonth, out LastDayOfTheThisMonth);

            // Summe der eingegangenen Zahlungen (auch Teilzahlungen) im Zeitraum
            var q = DB.invoices.Where(i => i.User.id == u.id && i.Deletestatus == false && i.ÖdemeDate >= FirstDayOfTheThisMonth && i.ÖdemeDate < LastDayOfTheThisMonth).Sum(i => i.ÖdemeTurarı);
            if (q.HasValue)
            {
                return q.Value.ToString("N2");
            }
            return "0,00";

        }
        public string Total_custumer(USER u)
        {
            return DB.Customers.Where(i => i.User.id == u.id && i.DeletStatus == false).Count().ToString();

        }
        public string NewCustumerİnmonth(USER u)
        {
            DateTime FirstDayOfTheThisMonth;
            DateTime LastDayOfTheThisMonth;
            Period(out FirstDayOfTheThisMonth, out LastDayOfTheThisMonth);

            var q = DB.Customers.Where(i => i.User.id == u.id && i.DeletStatus == false && i.Regdate >= FirstDayOfTheThisMonth && i.Regdate < LastDayOfTheThisMonth).Count().ToString();
            if (q != null)
            {
                return q;
            }
            return "0";
        }

        public string TotalStock()
        {
            double? s = DB.products.Where(i => i.DeletStatus == false && i.SaledPices == 0).Sum(i => (double?)i.Stock);
            return (s ?? 0).ToString("N0");
        }
        public List<REMİNDER> Getuserreminder(USER u)
        {
            List<REMİNDER> re = new List<REMİNDER>();
           List<REMİNDER> r = DB.reminders.Where(i => i.Users.id == u.id && i.İsDone == false && i.DeletStatus == false ).ToList();
            foreach (var item in r)
            {
                if (item.ReminDate == DateTime.Now.Date)
                {
                    re .Add(item);
                } 
            }
            return re;
        }
        public REMİNDER GetReminder_bayinfoandtitle(string title, string info)
        {
            return DB.reminders.Where(i => i.Title == title && i.Reminderİnfo == info && i.DeletStatus == false).FirstOrDefault();
        }
        public bool Reminder_ihtar( USER u)
        {
            List<REMİNDER> r = DB.reminders.Where(i => i.Users.id == u.id && i.İsDone == false && i.DeletStatus == false).ToList();
            // Warnsymbol, sobald irgendeine Erinnerung überfällig ist
            return r.Any(item => item.ReminDate < DateTime.Now.Date);
        }
        public string Product_sale_pices()
        {
            // verkaufte Stück im aktuellen Abrechnungsmonat (aus nicht gelöschten Rechnungen)
            DateTime first, last;
            Period(out first, out last);
            int? sum = DB.products.Where(i => i.SaledPices > 0 && i.invoices.Any(inv => inv.Deletestatus == false && inv.RegDate >= first && inv.RegDate < last))
                                  .Sum(i => (int?)i.SaledPices);
            return (sum ?? 0).ToString("N0") + " Stück";
        }
    }
}
