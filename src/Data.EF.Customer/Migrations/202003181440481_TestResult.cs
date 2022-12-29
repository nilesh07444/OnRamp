namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Test_Result", "IsGloballyAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Test_Result", "IsGloballyAccessed");
        }
    }
}
