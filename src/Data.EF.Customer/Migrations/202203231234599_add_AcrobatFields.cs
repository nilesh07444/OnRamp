namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_AcrobatFields : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.StandardUserTrainingGuides", newName: "TrainingGuideStandardUsers");
            DropPrimaryKey("dbo.TrainingGuideStandardUsers");
            CreateTable(
                "dbo.AcrobatFields",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DocumentStatus = c.Int(nullable: false),
                        CreatedOn = c.DateTime(),
                        CreatedBy = c.String(),
                        Printable = c.Boolean(nullable: false),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        Points = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PreviewMode = c.Int(nullable: false),
                        TrainingLabels = c.String(),
                        CoverPictureId = c.String(maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                        PublishStatus = c.Int(),
                        Approver = c.String(),
                        ApproverId = c.Guid(nullable: false),
                        IsCustomDocument = c.Boolean(),
                        CustomDocummentId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.CategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CoverPictureId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.AcrobatFieldContentBoxes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AcrobatFieldId = c.String(maxLength: 128),
                        Title = c.String(),
                        Number = c.Int(nullable: false),
                        Content = c.String(),
                        AttachmentRequired = c.Boolean(nullable: false),
                        IsAttached = c.Boolean(nullable: false),
                        NoteAllow = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        CustomDocument_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AcrobatFields", t => t.AcrobatFieldId)
                .ForeignKey("dbo.CustomDocuments", t => t.CustomDocument_Id)
                .Index(t => t.AcrobatFieldId)
                .Index(t => t.CustomDocument_Id);
            
            CreateTable(
                "dbo.AcrobatFieldChapters",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AcrobatFieldId = c.String(maxLength: 128),
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
                .ForeignKey("dbo.AcrobatFields", t => t.AcrobatFieldId)
                .Index(t => t.AcrobatFieldId);
            
            CreateTable(
                "dbo.AcrobatFieldChapterUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        AcrobatFieldChapterId = c.String(),
                        IsChecked = c.Boolean(nullable: false),
                        IssueDiscription = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        DateCompleted = c.DateTime(nullable: false),
                        ChapterTrackedDate = c.DateTime(),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                        AcrobatFieldContentBox_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AcrobatFieldContentBoxes", t => t.AcrobatFieldContentBox_Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.AcrobatFieldContentBox_Id);
            
            CreateTable(
                "dbo.AcrobatFieldChapterUserUploadResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        AcrobatFieldChapterId = c.String(),
                        UploadId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                        AcrobatFieldContentBox_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AcrobatFieldContentBoxes", t => t.AcrobatFieldContentBox_Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.UploadId)
                .Index(t => t.AcrobatFieldContentBox_Id);
            
            AddColumn("dbo.StandardUsers", "AcrobatField_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "AcrobatFieldContentBox_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "AcrobatFieldContentBox_Id1", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "AcrobatFieldChapter_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "AcrobatFieldChapter_Id1", c => c.String(maxLength: 128));
            AddColumn("dbo.CustomDocuments", "AcrobatFieldId", c => c.Guid());
            AddPrimaryKey("dbo.TrainingGuideStandardUsers", new[] { "TrainingGuide_Id", "StandardUser_Id" });
            CreateIndex("dbo.StandardUsers", "AcrobatField_Id");
            CreateIndex("dbo.Uploads", "AcrobatFieldContentBox_Id");
            CreateIndex("dbo.Uploads", "AcrobatFieldContentBox_Id1");
            CreateIndex("dbo.Uploads", "AcrobatFieldChapter_Id");
            CreateIndex("dbo.Uploads", "AcrobatFieldChapter_Id1");
            AddForeignKey("dbo.StandardUsers", "AcrobatField_Id", "dbo.AcrobatFields", "Id");
            AddForeignKey("dbo.Uploads", "AcrobatFieldContentBox_Id", "dbo.AcrobatFieldContentBoxes", "Id");
            AddForeignKey("dbo.Uploads", "AcrobatFieldContentBox_Id1", "dbo.AcrobatFieldContentBoxes", "Id");
            AddForeignKey("dbo.Uploads", "AcrobatFieldChapter_Id", "dbo.AcrobatFieldChapters", "Id");
            AddForeignKey("dbo.Uploads", "AcrobatFieldChapter_Id1", "dbo.AcrobatFieldChapters", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AcrobatFieldContentBoxes", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.AcrobatFieldChapterUserUploadResults", "UploadId", "dbo.Uploads");
            DropForeignKey("dbo.AcrobatFieldChapterUserUploadResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.AcrobatFieldChapterUserUploadResults", "AcrobatFieldContentBox_Id", "dbo.AcrobatFieldContentBoxes");
            DropForeignKey("dbo.AcrobatFieldChapterUserResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.AcrobatFieldChapterUserResults", "AcrobatFieldContentBox_Id", "dbo.AcrobatFieldContentBoxes");
            DropForeignKey("dbo.Uploads", "AcrobatFieldChapter_Id1", "dbo.AcrobatFieldChapters");
            DropForeignKey("dbo.Uploads", "AcrobatFieldChapter_Id", "dbo.AcrobatFieldChapters");
            DropForeignKey("dbo.AcrobatFieldChapters", "AcrobatFieldId", "dbo.AcrobatFields");
            DropForeignKey("dbo.AcrobatFields", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.Uploads", "AcrobatFieldContentBox_Id1", "dbo.AcrobatFieldContentBoxes");
            DropForeignKey("dbo.Uploads", "AcrobatFieldContentBox_Id", "dbo.AcrobatFieldContentBoxes");
            DropForeignKey("dbo.AcrobatFieldContentBoxes", "AcrobatFieldId", "dbo.AcrobatFields");
            DropForeignKey("dbo.StandardUsers", "AcrobatField_Id", "dbo.AcrobatFields");
            DropForeignKey("dbo.AcrobatFields", "CategoryId", "dbo.DocumentCategories");
            DropIndex("dbo.AcrobatFieldChapterUserUploadResults", new[] { "AcrobatFieldContentBox_Id" });
            DropIndex("dbo.AcrobatFieldChapterUserUploadResults", new[] { "UploadId" });
            DropIndex("dbo.AcrobatFieldChapterUserUploadResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.AcrobatFieldChapterUserResults", new[] { "AcrobatFieldContentBox_Id" });
            DropIndex("dbo.AcrobatFieldChapterUserResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.AcrobatFieldChapters", new[] { "AcrobatFieldId" });
            DropIndex("dbo.Uploads", new[] { "AcrobatFieldChapter_Id1" });
            DropIndex("dbo.Uploads", new[] { "AcrobatFieldChapter_Id" });
            DropIndex("dbo.Uploads", new[] { "AcrobatFieldContentBox_Id1" });
            DropIndex("dbo.Uploads", new[] { "AcrobatFieldContentBox_Id" });
            DropIndex("dbo.AcrobatFieldContentBoxes", new[] { "CustomDocument_Id" });
            DropIndex("dbo.AcrobatFieldContentBoxes", new[] { "AcrobatFieldId" });
            DropIndex("dbo.StandardUsers", new[] { "AcrobatField_Id" });
            DropIndex("dbo.AcrobatFields", new[] { "CategoryId" });
            DropIndex("dbo.AcrobatFields", new[] { "CoverPictureId" });
            DropPrimaryKey("dbo.TrainingGuideStandardUsers");
            DropColumn("dbo.CustomDocuments", "AcrobatFieldId");
            DropColumn("dbo.Uploads", "AcrobatFieldChapter_Id1");
            DropColumn("dbo.Uploads", "AcrobatFieldChapter_Id");
            DropColumn("dbo.Uploads", "AcrobatFieldContentBox_Id1");
            DropColumn("dbo.Uploads", "AcrobatFieldContentBox_Id");
            DropColumn("dbo.StandardUsers", "AcrobatField_Id");
            DropTable("dbo.AcrobatFieldChapterUserUploadResults");
            DropTable("dbo.AcrobatFieldChapterUserResults");
            DropTable("dbo.AcrobatFieldChapters");
            DropTable("dbo.AcrobatFieldContentBoxes");
            DropTable("dbo.AcrobatFields");
            AddPrimaryKey("dbo.TrainingGuideStandardUsers", new[] { "StandardUser_Id", "TrainingGuide_Id" });
            RenameTable(name: "dbo.TrainingGuideStandardUsers", newName: "StandardUserTrainingGuides");
        }
    }
}
