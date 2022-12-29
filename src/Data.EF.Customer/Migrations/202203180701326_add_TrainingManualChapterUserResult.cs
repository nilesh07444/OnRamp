namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_TrainingManualChapterUserResult : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TrainingManualChapterUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        TrainingManualChapterId = c.String(maxLength: 128),
                        IsChecked = c.Boolean(nullable: false),
                        IssueDiscription = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        DateCompleted = c.DateTime(nullable: false),
                        ChapterTrackedDate = c.DateTime(),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.TrainingManualChapters", t => t.TrainingManualChapterId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.TrainingManualChapterId);
            
            CreateTable(
                "dbo.TrainingManualChapterUserUploadResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        TrainingManualChapterId = c.String(maxLength: 128),
                        UploadId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.TrainingManualChapters", t => t.TrainingManualChapterId)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.TrainingManualChapterId)
                .Index(t => t.UploadId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingManualChapterUserUploadResults", "UploadId", "dbo.Uploads");
            DropForeignKey("dbo.TrainingManualChapterUserUploadResults", "TrainingManualChapterId", "dbo.TrainingManualChapters");
            DropForeignKey("dbo.TrainingManualChapterUserUploadResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.TrainingManualChapterUserResults", "TrainingManualChapterId", "dbo.TrainingManualChapters");
            DropForeignKey("dbo.TrainingManualChapterUserResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropIndex("dbo.TrainingManualChapterUserUploadResults", new[] { "UploadId" });
            DropIndex("dbo.TrainingManualChapterUserUploadResults", new[] { "TrainingManualChapterId" });
            DropIndex("dbo.TrainingManualChapterUserUploadResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.TrainingManualChapterUserResults", new[] { "TrainingManualChapterId" });
            DropIndex("dbo.TrainingManualChapterUserResults", new[] { "AssignedDocumentId" });
            DropTable("dbo.TrainingManualChapterUserUploadResults");
            DropTable("dbo.TrainingManualChapterUserResults");
        }
    }
}
