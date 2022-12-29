namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_database_field : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ScheduleReportParameters", "ParameterID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ScheduleReportParameters", "ParameterID", c => c.Guid(nullable: false));
        }
    }
}
