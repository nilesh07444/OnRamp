namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_TestChapterUserResult : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestChapterUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        TestChapterId = c.String(),
                        IsChecked = c.Boolean(nullable: false),
                        IssueDiscription = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        DateCompleted = c.DateTime(nullable: false),
                        ChapterTrackedDate = c.DateTime(),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                        TestQuestion_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.TestQuestions", t => t.TestQuestion_Id)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.TestQuestion_Id);
            
            CreateTable(
                "dbo.TestChapterUserUploadResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        TestChapterId = c.String(),
                        UploadId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                        TestQuestion_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.TestQuestions", t => t.TestQuestion_Id)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.UploadId)
                .Index(t => t.TestQuestion_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestChapterUserUploadResults", "UploadId", "dbo.Uploads");
            DropForeignKey("dbo.TestChapterUserUploadResults", "TestQuestion_Id", "dbo.TestQuestions");
            DropForeignKey("dbo.TestChapterUserUploadResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.TestChapterUserResults", "TestQuestion_Id", "dbo.TestQuestions");
            DropForeignKey("dbo.TestChapterUserResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropIndex("dbo.TestChapterUserUploadResults", new[] { "TestQuestion_Id" });
            DropIndex("dbo.TestChapterUserUploadResults", new[] { "UploadId" });
            DropIndex("dbo.TestChapterUserUploadResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.TestChapterUserResults", new[] { "TestQuestion_Id" });
            DropIndex("dbo.TestChapterUserResults", new[] { "AssignedDocumentId" });
            DropTable("dbo.TestChapterUserUploadResults");
            DropTable("dbo.TestChapterUserResults");
        }
    }
}
