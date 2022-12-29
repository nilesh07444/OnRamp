namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingGuideChapterToTrainingGuideLink : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TraningGuideChapters", "LinkedTrainingGuide_Id", c => c.Guid());
            CreateIndex("dbo.TraningGuideChapters", "LinkedTrainingGuide_Id");
            AddForeignKey("dbo.TraningGuideChapters", "LinkedTrainingGuide_Id", "dbo.TrainingGuides", "TrainingGuideId");
            DropColumn("dbo.TrainingGuides", "LinkedTraningGuideId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TrainingGuides", "LinkedTraningGuideId", c => c.Guid());
            DropForeignKey("dbo.TraningGuideChapters", "LinkedTrainingGuide_Id", "dbo.TrainingGuides");
            DropIndex("dbo.TraningGuideChapters", new[] { "LinkedTrainingGuide_Id" });
            DropColumn("dbo.TraningGuideChapters", "LinkedTrainingGuide_Id");
        }
    }
}
