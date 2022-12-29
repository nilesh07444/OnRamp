namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestResultExpansion : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TestResults", "TrainingGuideId", "dbo.TrainingGuides");
            DropForeignKey("dbo.TestResults", "TrainingTestId", "dbo.TrainingTests");
            DropIndex("dbo.TestResults", new[] { "TrainingGuideId" });
            DropIndex("dbo.TestResults", new[] { "TrainingTestId" });
            AddColumn("dbo.TestResults", "Version", c => c.Int(nullable: false));
            AddColumn("dbo.TestResults", "TestTitle", c => c.String());
            AddColumn("dbo.TestResults", "TrainingGuideTitle", c => c.String());
            AddColumn("dbo.TestResults", "TrainingGuideCategory", c => c.String());
            AddColumn("dbo.TestResults", "TrainingGuideCategoryId", c => c.Guid());
            AddColumn("dbo.TestResults", "TrophyName", c => c.String());
            AlterColumn("dbo.TestResults", "TrainingTestId", c => c.Guid());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TestResults", "TrainingTestId", c => c.Guid(nullable: false));
            DropColumn("dbo.TestResults", "TrophyName");
            DropColumn("dbo.TestResults", "TrainingGuideCategoryId");
            DropColumn("dbo.TestResults", "TrainingGuideCategory");
            DropColumn("dbo.TestResults", "TrainingGuideTitle");
            DropColumn("dbo.TestResults", "TestTitle");
            DropColumn("dbo.TestResults", "Version");
            CreateIndex("dbo.TestResults", "TrainingTestId");
            CreateIndex("dbo.TestResults", "TrainingGuideId");
            AddForeignKey("dbo.TestResults", "TrainingTestId", "dbo.TrainingTests", "TrainingTestId", cascadeDelete: true);
            AddForeignKey("dbo.TestResults", "TrainingGuideId", "dbo.TrainingGuides", "TrainingGuideId");
        }
    }
}
