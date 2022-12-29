namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1238 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Tests", new[] { "CertificateId" });
            CreateTable(
                "dbo.Test_Result",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false),
                        TestId = c.String(nullable: false, maxLength: 128),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        Percentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Score = c.Int(nullable: false),
                        Total = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        Passed = c.Boolean(nullable: false),
                        CertificateId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Uploads", t => t.CertificateId)
                .ForeignKey("dbo.Tests", t => t.TestId, cascadeDelete: true)
                .Index(t => t.TestId)
                .Index(t => t.CertificateId);
            
            CreateTable(
                "dbo.TestQuestion_Result",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        QuestionId = c.String(maxLength: 128),
                        Deleted = c.Boolean(nullable: false),
                        UnAnswered = c.Boolean(nullable: false),
                        ViewLater = c.Boolean(nullable: false),
                        Correct = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestQuestions", t => t.QuestionId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.TestQuestionAnswer_Result",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AnswerId = c.String(maxLength: 128),
                        Selected = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestQuestionAnswers", t => t.AnswerId)
                .Index(t => t.AnswerId);
            
            CreateTable(
                "dbo.UserFeedbackReads",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UserFeedbackId = c.String(maxLength: 128),
                        UserId = c.String(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserFeedbacks", t => t.UserFeedbackId)
                .Index(t => t.UserFeedbackId);
            
            CreateTable(
                "dbo.UserFeedbacks",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Type = c.Int(nullable: false),
                        ContentType = c.Int(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        SystemLocation = c.String(),
                        DocumentId = c.String(),
                        DocumentType = c.Int(),
                        Content = c.String(),
                        CreatedById = c.String(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestQuestion_Result_Answers_Map",
                c => new
                    {
                        TestQuestion_Result_Id = c.String(nullable: false, maxLength: 128),
                        TestQuestionAnswer_Result_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TestQuestion_Result_Id, t.TestQuestionAnswer_Result_Id })
                .ForeignKey("dbo.TestQuestion_Result", t => t.TestQuestion_Result_Id, cascadeDelete: true)
                .ForeignKey("dbo.TestQuestionAnswer_Result", t => t.TestQuestionAnswer_Result_Id, cascadeDelete: true)
                .Index(t => t.TestQuestion_Result_Id)
                .Index(t => t.TestQuestionAnswer_Result_Id);
            
            CreateTable(
                "dbo.Test_Result_Questions_Map",
                c => new
                    {
                        Test_Result_Id = c.String(nullable: false, maxLength: 128),
                        TestQuestionResult_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Test_Result_Id, t.TestQuestionResult_Id })
                .ForeignKey("dbo.Test_Result", t => t.Test_Result_Id, cascadeDelete: true)
                .ForeignKey("dbo.TestQuestion_Result", t => t.TestQuestionResult_Id, cascadeDelete: true)
                .Index(t => t.Test_Result_Id)
                .Index(t => t.TestQuestionResult_Id);
            
            AlterColumn("dbo.Tests", "CertificateId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Tests", "CertificateId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserFeedbackReads", "UserFeedbackId", "dbo.UserFeedbacks");
            DropForeignKey("dbo.Test_Result", "TestId", "dbo.Tests");
            DropForeignKey("dbo.Test_Result_Questions_Map", "TestQuestionResult_Id", "dbo.TestQuestion_Result");
            DropForeignKey("dbo.Test_Result_Questions_Map", "Test_Result_Id", "dbo.Test_Result");
            DropForeignKey("dbo.TestQuestion_Result", "QuestionId", "dbo.TestQuestions");
            DropForeignKey("dbo.TestQuestion_Result_Answers_Map", "TestQuestionAnswer_Result_Id", "dbo.TestQuestionAnswer_Result");
            DropForeignKey("dbo.TestQuestion_Result_Answers_Map", "TestQuestion_Result_Id", "dbo.TestQuestion_Result");
            DropForeignKey("dbo.TestQuestionAnswer_Result", "AnswerId", "dbo.TestQuestionAnswers");
            DropForeignKey("dbo.Test_Result", "CertificateId", "dbo.Uploads");
            DropIndex("dbo.Test_Result_Questions_Map", new[] { "TestQuestionResult_Id" });
            DropIndex("dbo.Test_Result_Questions_Map", new[] { "Test_Result_Id" });
            DropIndex("dbo.TestQuestion_Result_Answers_Map", new[] { "TestQuestionAnswer_Result_Id" });
            DropIndex("dbo.TestQuestion_Result_Answers_Map", new[] { "TestQuestion_Result_Id" });
            DropIndex("dbo.UserFeedbackReads", new[] { "UserFeedbackId" });
            DropIndex("dbo.Tests", new[] { "CertificateId" });
            DropIndex("dbo.TestQuestionAnswer_Result", new[] { "AnswerId" });
            DropIndex("dbo.TestQuestion_Result", new[] { "QuestionId" });
            DropIndex("dbo.Test_Result", new[] { "CertificateId" });
            DropIndex("dbo.Test_Result", new[] { "TestId" });
            AlterColumn("dbo.Tests", "CertificateId", c => c.String(nullable: false, maxLength: 128));
            DropTable("dbo.Test_Result_Questions_Map");
            DropTable("dbo.TestQuestion_Result_Answers_Map");
            DropTable("dbo.UserFeedbacks");
            DropTable("dbo.UserFeedbackReads");
            DropTable("dbo.TestQuestionAnswer_Result");
            DropTable("dbo.TestQuestion_Result");
            DropTable("dbo.Test_Result");
            CreateIndex("dbo.Tests", "CertificateId");
        }
    }
}
