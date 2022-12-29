namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScheduleReportParams : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ScheduleReportParams",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ReportId = c.Guid(),
                        ParameterName = c.String(),
                        ParameterValuess = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ScheduleReportParams");
        }
    }
}
