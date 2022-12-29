namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schedulereports : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ScheduleReportModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ScheduleName = c.String(),
                        RecipientsList = c.String(),
                        ReportAssignedId = c.String(),
                        Occurences = c.String(),
                        ScheduleTime = c.DateTime(),
                        DateCreated = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ScheduleReportParameters",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ReportID = c.Guid(nullable: false),
                        ParameterID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SelectedScheduleReportParameters",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        ParameterID = c.Guid(nullable: false),
                        Selectedparameter = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SelectedScheduleReportParameters");
            DropTable("dbo.ScheduleReportParameters");
            DropTable("dbo.ScheduleReportModels");
        }
    }
}
