using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class DB :DbContext
    {
        // Zentrale Verbindungszeichenfolge aus App.config ("constr") – wird von allen DAL-Klassen genutzt
        public static readonly string ConStr = System.Configuration.ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

        public DB() : base("constr")
        {

        }
        public DbSet<ACTİVİTY_CATEGORY> ActivityCategories { get; set; }
        public DbSet<ACTİVİTY> Activitys { get; set; }
        public DbSet<CUSTOMER> Customers { get; set; }

        public DbSet<İNVOİCE> invoices { get; set; }
        public DbSet<PRODUCT> products { get; set; }
        public DbSet<REMİNDER> reminders { get; set; }
        public DbSet<USER> users { get; set; }

        public DbSet<USER_GROUP> usergrups { get; set; }
        public DbSet<USER_ACCESS_ROLE> userrols { get; set; }
        public DbSet<USER_PAS_SAVE> userpass_Saves { get; set; }
    }
}
