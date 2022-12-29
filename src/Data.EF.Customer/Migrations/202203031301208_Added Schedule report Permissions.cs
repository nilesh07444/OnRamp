namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSchedulereportPermissions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomUserRoles", "ManageReportSchedule", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomUserRoles", "ManageReportSchedule");
        }
    }
}
