namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_MemoChapterUserResult_add : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MemoChapterUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        MemoChapterId = c.String(maxLength: 128),
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
                .ForeignKey("dbo.MemoChapters", t => t.MemoChapterId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.MemoChapterId);
            
            CreateTable(
                "dbo.MemoChapters",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        MemoId = c.String(maxLength: 128),
                        Title = c.String(),
                        Number = c.Int(nullable: false),
                        Content = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        AttachmentRequired = c.Boolean(nullable: false),
                        CheckRequired = c.Boolean(nullable: false),
                        IsChecked = c.Boolean(nullable: false),
                        NoteAllow = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Memos", t => t.MemoId)
                .Index(t => t.MemoId);
            
            CreateTable(
                "dbo.MemoChapterUserUploadResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        MemoChapterId = c.String(maxLength: 128),
                        UploadId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.MemoChapters", t => t.MemoChapterId)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.MemoChapterId)
                .Index(t => t.UploadId);
            
            AddColumn("dbo.Uploads", "MemoChapter_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "MemoChapter_Id1", c => c.String(maxLength: 128));
            CreateIndex("dbo.Uploads", "MemoChapter_Id");
            CreateIndex("dbo.Uploads", "MemoChapter_Id1");
            AddForeignKey("dbo.Uploads", "MemoChapter_Id", "dbo.MemoChapters", "Id");
            AddForeignKey("dbo.Uploads", "MemoChapter_Id1", "dbo.MemoChapters", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemoChapterUserUploadResults", "UploadId", "dbo.Uploads");
            DropForeignKey("dbo.MemoChapterUserUploadResults", "MemoChapterId", "dbo.MemoChapters");
            DropForeignKey("dbo.MemoChapterUserUploadResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.MemoChapterUserResults", "MemoChapterId", "dbo.MemoChapters");
            DropForeignKey("dbo.Uploads", "MemoChapter_Id1", "dbo.MemoChapters");
            DropForeignKey("dbo.MemoChapters", "MemoId", "dbo.Memos");
            DropForeignKey("dbo.Uploads", "MemoChapter_Id", "dbo.MemoChapters");
            DropForeignKey("dbo.MemoChapterUserResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropIndex("dbo.MemoChapterUserUploadResults", new[] { "UploadId" });
            DropIndex("dbo.MemoChapterUserUploadResults", new[] { "MemoChapterId" });
            DropIndex("dbo.MemoChapterUserUploadResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.MemoChapters", new[] { "MemoId" });
            DropIndex("dbo.MemoChapterUserResults", new[] { "MemoChapterId" });
            DropIndex("dbo.MemoChapterUserResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.Uploads", new[] { "MemoChapter_Id1" });
            DropIndex("dbo.Uploads", new[] { "MemoChapter_Id" });
            DropColumn("dbo.Uploads", "MemoChapter_Id1");
            DropColumn("dbo.Uploads", "MemoChapter_Id");
            DropTable("dbo.MemoChapterUserUploadResults");
            DropTable("dbo.MemoChapters");
            DropTable("dbo.MemoChapterUserResults");
        }
    }
}
