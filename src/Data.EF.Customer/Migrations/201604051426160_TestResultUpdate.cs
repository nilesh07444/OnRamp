namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestResultUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestResults", "Points", c => c.Int(nullable: false));
            AddColumn("dbo.TestResults", "Total", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestResults", "Total");
            DropColumn("dbo.TestResults", "Points");
        }
    }
}
