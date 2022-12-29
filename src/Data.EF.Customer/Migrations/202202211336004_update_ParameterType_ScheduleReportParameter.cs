namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_ParameterType_ScheduleReportParameter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleReportParameters", "ParameterType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScheduleReportParameters", "ParameterType");
        }
    }
}
