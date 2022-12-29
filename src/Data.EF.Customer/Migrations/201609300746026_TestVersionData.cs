namespace Data.EF.Customer.Migrations
{
    using Common.Command;
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;

    public partial class TestVersionData : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new MigrateCompanyPlaybooksAndTestToVersionsCommand());
        }
        
        public override void Down()
        {
        }
    }
}
