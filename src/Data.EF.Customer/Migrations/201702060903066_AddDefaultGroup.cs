namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Ramp.Contracts.CommandParameter.Migration;
    
    public partial class AddDefaultGroup : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new AddDefaultGroupToCustomerCompanies_ORP989());
        }
        
        public override void Down()
        {
        }
    }
}
