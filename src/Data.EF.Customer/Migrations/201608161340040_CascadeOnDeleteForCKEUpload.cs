namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascadeOnDeleteForCKEUpload : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CKEUploads", "TrainingGuideChapter_Id", "dbo.TraningGuideChapters");
            DropIndex("dbo.CKEUploads", new[] { "TrainingGuideChapter_Id" });
            AlterColumn("dbo.CKEUploads", "TrainingGuideChapter_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.CKEUploads", "TrainingGuideChapter_Id");
            AddForeignKey("dbo.CKEUploads", "TrainingGuideChapter_Id", "dbo.TraningGuideChapters", "TraningGuideChapterId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CKEUploads", "TrainingGuideChapter_Id", "dbo.TraningGuideChapters");
            DropIndex("dbo.CKEUploads", new[] { "TrainingGuideChapter_Id" });
            AlterColumn("dbo.CKEUploads", "TrainingGuideChapter_Id", c => c.Guid());
            CreateIndex("dbo.CKEUploads", "TrainingGuideChapter_Id");
            AddForeignKey("dbo.CKEUploads", "TrainingGuideChapter_Id", "dbo.TraningGuideChapters", "TraningGuideChapterId");
        }
    }
}
