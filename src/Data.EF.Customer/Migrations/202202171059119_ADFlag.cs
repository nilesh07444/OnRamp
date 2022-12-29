namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ADFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUsers", "AdUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUsers", "AdUser");
        }
    }
}
