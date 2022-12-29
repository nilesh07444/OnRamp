namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestResultsTrophyData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestResults", "TrophyData", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestResults", "TrophyData");
        }
    }
}
