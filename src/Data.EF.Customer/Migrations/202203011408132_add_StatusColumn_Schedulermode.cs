namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_StatusColumn_Schedulermode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleReportModels", "Status", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScheduleReportModels", "Status");
        }
    }
}
