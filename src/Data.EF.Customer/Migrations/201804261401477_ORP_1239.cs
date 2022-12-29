namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP_1239 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentCategories",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                        ParentId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Labels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Deleted = c.Boolean(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MemoContentBoxes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        MemoId = c.String(maxLength: 128),
                        Title = c.String(),
                        Number = c.Int(nullable: false),
                        Content = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Memos", t => t.MemoId)
                .Index(t => t.MemoId);
            
            CreateTable(
                "dbo.Uploads",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Type = c.String(),
                        ContentType = c.String(),
                        Data = c.Binary(),
                        Name = c.String(),
                        Description = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        Content = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Memos",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DocumentStatus = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Printable = c.Boolean(nullable: false),
                        Points = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PreviewMode = c.Int(nullable: false),
                        CoverPictureId = c.String(maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.CategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CoverPictureId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Policies",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DocumentStatus = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Printable = c.Boolean(nullable: false),
                        CallToAction = c.Boolean(nullable: false),
                        CallToActionMessage = c.String(),
                        Points = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PreviewMode = c.Int(nullable: false),
                        CoverPictureId = c.String(maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.CategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CoverPictureId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.PolicyContentBoxes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PolicyId = c.String(maxLength: 128),
                        Title = c.String(),
                        Number = c.Int(nullable: false),
                        Content = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Policies", t => t.PolicyId)
                .Index(t => t.PolicyId);
            
            CreateTable(
                "dbo.TestQuestionAnswers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Option = c.String(),
                        Number = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        TestQuestionId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestQuestions", t => t.TestQuestionId, cascadeDelete: true)
                .Index(t => t.TestQuestionId);
            
            CreateTable(
                "dbo.TestQuestions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        TestId = c.String(maxLength: 128),
                        Number = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        AnswerWeightage = c.Int(nullable: false),
                        Question = c.String(),
                        CorrectAnswerId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tests", t => t.TestId)
                .Index(t => t.TestId);
            
            CreateTable(
                "dbo.Tests",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DocumentStatus = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Printable = c.Boolean(nullable: false),
                        Points = c.Int(nullable: false),
                        PassMarks = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Duration = c.Int(nullable: false),
                        IntroductionContent = c.String(),
                        TestExpiresNumberDaysFromAssignment = c.Int(),
                        NotificationInteval = c.Int(nullable: false),
                        NotificationIntevalDaysBeforeExpiry = c.Int(),
                        ExpiryNotificationSentOn = c.DateTime(),
                        AssignMarksToQuestions = c.Boolean(nullable: false),
                        TestReview = c.Boolean(nullable: false),
                        RandomizeQuestions = c.Boolean(nullable: false),
                        EmailSummary = c.Boolean(nullable: false),
                        HighlightAnswersOnSummary = c.Boolean(),
                        Deleted = c.Boolean(nullable: false),
                        PreviewMode = c.Int(nullable: false),
                        CoverPictureId = c.String(maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.CategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CoverPictureId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.TrainingManualChapters",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        TrainingManualId = c.String(maxLength: 128),
                        Title = c.String(),
                        Number = c.Int(nullable: false),
                        Content = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrainingManuals", t => t.TrainingManualId)
                .Index(t => t.TrainingManualId);
            
            CreateTable(
                "dbo.TrainingManuals",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DocumentStatus = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Printable = c.Boolean(nullable: false),
                        Points = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PreviewMode = c.Int(nullable: false),
                        CoverPictureId = c.String(maxLength: 128),
                        DocumentCategoryId = c.String(maxLength: 128),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.DocumentCategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CoverPictureId)
                .Index(t => t.DocumentCategoryId);
            
            CreateTable(
                "dbo.MemoContentBoxContentToolsUploads",
                c => new
                    {
                        MemoContentBox_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.MemoContentBox_Id, t.Upload_Id })
                .ForeignKey("dbo.MemoContentBoxes", t => t.MemoContentBox_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.MemoContentBox_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.MemoLabels",
                c => new
                    {
                        Memo_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Memo_Id, t.Label_Id })
                .ForeignKey("dbo.Memos", t => t.Memo_Id, cascadeDelete: true)
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .Index(t => t.Memo_Id)
                .Index(t => t.Label_Id);
            
            CreateTable(
                "dbo.MemoContentBoxUploads",
                c => new
                    {
                        MemoContentBox_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.MemoContentBox_Id, t.Upload_Id })
                .ForeignKey("dbo.MemoContentBoxes", t => t.MemoContentBox_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.MemoContentBox_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.PolicyContentBoxContentToolsUploads",
                c => new
                    {
                        PolicyContentBox_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PolicyContentBox_Id, t.Upload_Id })
                .ForeignKey("dbo.PolicyContentBoxes", t => t.PolicyContentBox_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.PolicyContentBox_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.PolicyContentBoxUploads",
                c => new
                    {
                        PolicyContentBox_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PolicyContentBox_Id, t.Upload_Id })
                .ForeignKey("dbo.PolicyContentBoxes", t => t.PolicyContentBox_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.PolicyContentBox_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.PolicyLabels",
                c => new
                    {
                        Policy_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Policy_Id, t.Label_Id })
                .ForeignKey("dbo.Policies", t => t.Policy_Id, cascadeDelete: true)
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .Index(t => t.Policy_Id)
                .Index(t => t.Label_Id);
            
            CreateTable(
                "dbo.TestQuestionContentToolsUploads",
                c => new
                    {
                        TestQuestion_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TestQuestion_Id, t.Upload_Id })
                .ForeignKey("dbo.TestQuestions", t => t.TestQuestion_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.TestQuestion_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.TestLabels",
                c => new
                    {
                        Test_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Test_Id, t.Label_Id })
                .ForeignKey("dbo.Tests", t => t.Test_Id, cascadeDelete: true)
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .Index(t => t.Test_Id)
                .Index(t => t.Label_Id);
            
            CreateTable(
                "dbo.TestQuestionUploads",
                c => new
                    {
                        TestQuestion_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TestQuestion_Id, t.Upload_Id })
                .ForeignKey("dbo.TestQuestions", t => t.TestQuestion_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.TestQuestion_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.TrainingManualChapterContentToolsUploads",
                c => new
                    {
                        TrainingManualChapter_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TrainingManualChapter_Id, t.Upload_Id })
                .ForeignKey("dbo.TrainingManualChapters", t => t.TrainingManualChapter_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.TrainingManualChapter_Id)
                .Index(t => t.Upload_Id);
            
            CreateTable(
                "dbo.TrainingManualLabels",
                c => new
                    {
                        TrainingManual_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TrainingManual_Id, t.Label_Id })
                .ForeignKey("dbo.TrainingManuals", t => t.TrainingManual_Id, cascadeDelete: true)
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .Index(t => t.TrainingManual_Id)
                .Index(t => t.Label_Id);
            
            CreateTable(
                "dbo.TrainingManualChapterUploads",
                c => new
                    {
                        TrainingManualChapter_Id = c.String(nullable: false, maxLength: 128),
                        Upload_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TrainingManualChapter_Id, t.Upload_Id })
                .ForeignKey("dbo.TrainingManualChapters", t => t.TrainingManualChapter_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.TrainingManualChapter_Id)
                .Index(t => t.Upload_Id);
            
            AddColumn("dbo.FileUploads", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingManualChapterUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.TrainingManualChapterUploads", "TrainingManualChapter_Id", "dbo.TrainingManualChapters");
            DropForeignKey("dbo.TrainingManualLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.TrainingManualLabels", "TrainingManual_Id", "dbo.TrainingManuals");
            DropForeignKey("dbo.TrainingManuals", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.TrainingManualChapters", "TrainingManualId", "dbo.TrainingManuals");
            DropForeignKey("dbo.TrainingManuals", "DocumentCategoryId", "dbo.DocumentCategories");
            DropForeignKey("dbo.TrainingManualChapterContentToolsUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.TrainingManualChapterContentToolsUploads", "TrainingManualChapter_Id", "dbo.TrainingManualChapters");
            DropForeignKey("dbo.TestQuestionUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.TestQuestionUploads", "TestQuestion_Id", "dbo.TestQuestions");
            DropForeignKey("dbo.TestLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.TestLabels", "Test_Id", "dbo.Tests");
            DropForeignKey("dbo.TestQuestions", "TestId", "dbo.Tests");
            DropForeignKey("dbo.Tests", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.Tests", "CategoryId", "dbo.DocumentCategories");
            DropForeignKey("dbo.TestQuestionContentToolsUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.TestQuestionContentToolsUploads", "TestQuestion_Id", "dbo.TestQuestions");
            DropForeignKey("dbo.TestQuestionAnswers", "TestQuestionId", "dbo.TestQuestions");
            DropForeignKey("dbo.PolicyLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.PolicyLabels", "Policy_Id", "dbo.Policies");
            DropForeignKey("dbo.Policies", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.PolicyContentBoxUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.PolicyContentBoxUploads", "PolicyContentBox_Id", "dbo.PolicyContentBoxes");
            DropForeignKey("dbo.PolicyContentBoxes", "PolicyId", "dbo.Policies");
            DropForeignKey("dbo.PolicyContentBoxContentToolsUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.PolicyContentBoxContentToolsUploads", "PolicyContentBox_Id", "dbo.PolicyContentBoxes");
            DropForeignKey("dbo.Policies", "CategoryId", "dbo.DocumentCategories");
            DropForeignKey("dbo.MemoContentBoxUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.MemoContentBoxUploads", "MemoContentBox_Id", "dbo.MemoContentBoxes");
            DropForeignKey("dbo.MemoLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.MemoLabels", "Memo_Id", "dbo.Memos");
            DropForeignKey("dbo.Memos", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.MemoContentBoxes", "MemoId", "dbo.Memos");
            DropForeignKey("dbo.Memos", "CategoryId", "dbo.DocumentCategories");
            DropForeignKey("dbo.MemoContentBoxContentToolsUploads", "Upload_Id", "dbo.Uploads");
            DropForeignKey("dbo.MemoContentBoxContentToolsUploads", "MemoContentBox_Id", "dbo.MemoContentBoxes");
            DropIndex("dbo.TrainingManualChapterUploads", new[] { "Upload_Id" });
            DropIndex("dbo.TrainingManualChapterUploads", new[] { "TrainingManualChapter_Id" });
            DropIndex("dbo.TrainingManualLabels", new[] { "Label_Id" });
            DropIndex("dbo.TrainingManualLabels", new[] { "TrainingManual_Id" });
            DropIndex("dbo.TrainingManualChapterContentToolsUploads", new[] { "Upload_Id" });
            DropIndex("dbo.TrainingManualChapterContentToolsUploads", new[] { "TrainingManualChapter_Id" });
            DropIndex("dbo.TestQuestionUploads", new[] { "Upload_Id" });
            DropIndex("dbo.TestQuestionUploads", new[] { "TestQuestion_Id" });
            DropIndex("dbo.TestLabels", new[] { "Label_Id" });
            DropIndex("dbo.TestLabels", new[] { "Test_Id" });
            DropIndex("dbo.TestQuestionContentToolsUploads", new[] { "Upload_Id" });
            DropIndex("dbo.TestQuestionContentToolsUploads", new[] { "TestQuestion_Id" });
            DropIndex("dbo.PolicyLabels", new[] { "Label_Id" });
            DropIndex("dbo.PolicyLabels", new[] { "Policy_Id" });
            DropIndex("dbo.PolicyContentBoxUploads", new[] { "Upload_Id" });
            DropIndex("dbo.PolicyContentBoxUploads", new[] { "PolicyContentBox_Id" });
            DropIndex("dbo.PolicyContentBoxContentToolsUploads", new[] { "Upload_Id" });
            DropIndex("dbo.PolicyContentBoxContentToolsUploads", new[] { "PolicyContentBox_Id" });
            DropIndex("dbo.MemoContentBoxUploads", new[] { "Upload_Id" });
            DropIndex("dbo.MemoContentBoxUploads", new[] { "MemoContentBox_Id" });
            DropIndex("dbo.MemoLabels", new[] { "Label_Id" });
            DropIndex("dbo.MemoLabels", new[] { "Memo_Id" });
            DropIndex("dbo.MemoContentBoxContentToolsUploads", new[] { "Upload_Id" });
            DropIndex("dbo.MemoContentBoxContentToolsUploads", new[] { "MemoContentBox_Id" });
            DropIndex("dbo.TrainingManuals", new[] { "DocumentCategoryId" });
            DropIndex("dbo.TrainingManuals", new[] { "CoverPictureId" });
            DropIndex("dbo.TrainingManualChapters", new[] { "TrainingManualId" });
            DropIndex("dbo.Tests", new[] { "CategoryId" });
            DropIndex("dbo.Tests", new[] { "CoverPictureId" });
            DropIndex("dbo.TestQuestions", new[] { "TestId" });
            DropIndex("dbo.TestQuestionAnswers", new[] { "TestQuestionId" });
            DropIndex("dbo.PolicyContentBoxes", new[] { "PolicyId" });
            DropIndex("dbo.Policies", new[] { "CategoryId" });
            DropIndex("dbo.Policies", new[] { "CoverPictureId" });
            DropIndex("dbo.Memos", new[] { "CategoryId" });
            DropIndex("dbo.Memos", new[] { "CoverPictureId" });
            DropIndex("dbo.MemoContentBoxes", new[] { "MemoId" });
            DropColumn("dbo.FileUploads", "Deleted");
            DropTable("dbo.TrainingManualChapterUploads");
            DropTable("dbo.TrainingManualLabels");
            DropTable("dbo.TrainingManualChapterContentToolsUploads");
            DropTable("dbo.TestQuestionUploads");
            DropTable("dbo.TestLabels");
            DropTable("dbo.TestQuestionContentToolsUploads");
            DropTable("dbo.PolicyLabels");
            DropTable("dbo.PolicyContentBoxUploads");
            DropTable("dbo.PolicyContentBoxContentToolsUploads");
            DropTable("dbo.MemoContentBoxUploads");
            DropTable("dbo.MemoLabels");
            DropTable("dbo.MemoContentBoxContentToolsUploads");
            DropTable("dbo.TrainingManuals");
            DropTable("dbo.TrainingManualChapters");
            DropTable("dbo.Tests");
            DropTable("dbo.TestQuestions");
            DropTable("dbo.TestQuestionAnswers");
            DropTable("dbo.PolicyContentBoxes");
            DropTable("dbo.Policies");
            DropTable("dbo.Memos");
            DropTable("dbo.Uploads");
            DropTable("dbo.MemoContentBoxes");
            DropTable("dbo.Labels");
            DropTable("dbo.DocumentCategories");
        }
    }
}
