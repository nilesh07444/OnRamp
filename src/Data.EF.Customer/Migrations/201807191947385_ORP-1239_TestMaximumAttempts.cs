namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1239_TestMaximumAttempts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tests", "MaximumAttempts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tests", "MaximumAttempts");
        }
    }
}
