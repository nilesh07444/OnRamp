namespace Data.EF.Customer.Migrations
{
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;

    public partial class CustomerConfiguationData : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new MigrateCustomConfigurationDataCommand());
        }
        
        public override void Down()
        {
        }
    }
}
