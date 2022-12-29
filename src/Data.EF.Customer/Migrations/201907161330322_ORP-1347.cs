namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1347 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tests", "EnableTimer", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tests", "EnableTimer");
        }
    }
}
