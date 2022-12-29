namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1085 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "HideDashboardLogo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "HideDashboardLogo");
        }
    }
}
