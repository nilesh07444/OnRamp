namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class 
        ViewForGuidesAndTests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TrainingTestUsageStats",
                c => new
                    {
                        TrainingTestUsageStatsId = c.Guid(nullable: false),
                        TrainingTestId = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        DateTaken = c.DateTime(),
                    })
                .PrimaryKey(t => t.TrainingTestUsageStatsId);
            
            AddColumn("dbo.TestAssigneds", "AssignedDate", c => c.DateTime());
            AddColumn("dbo.AssignedTrainingGuides", "AssignedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssignedTrainingGuides", "AssignedDate");
            DropColumn("dbo.TestAssigneds", "AssignedDate");
            DropTable("dbo.TrainingTestUsageStats");
        }
    }
}
