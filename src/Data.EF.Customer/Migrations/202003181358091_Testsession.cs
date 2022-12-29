namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Testsession : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestSessions", "IsGlobalAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestSessions", "IsGlobalAccessed");
        }
    }
}
