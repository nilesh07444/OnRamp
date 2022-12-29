namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShowCompanyNameOnDashboard_Company : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "ShowCompanyNameOnDashboard", c => c.Boolean(nullable: false));
            Sql("UPDATE Companies SET ShowCompanyNameOnDashboard = '1';");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "ShowCompanyNameOnDashboard");
        }
    }
}
