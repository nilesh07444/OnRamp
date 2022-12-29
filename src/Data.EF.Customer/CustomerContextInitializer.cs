using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF.Customer
{
    internal class CustomerContextInitializer : MigrateDatabaseToLatestVersion<CustomerContext, Migrations.Configuration>
    {
        public CustomerContextInitializer() : base(true)
        {
        }
        public override void InitializeDatabase(CustomerContext context)
        {
            base.InitializeDatabase(context);
        }
    }
}
