namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectMerge : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomDocumentAnswerSubmissions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CustomDocumentID = c.Guid(nullable: false),
                        TestQuestionID = c.Guid(nullable: false),
                        TestSelectedAnswer = c.String(),
                        StandarduserID = c.Guid(nullable: false),
                        documentType = c.Int(nullable: false),
                        CreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AcrobatFieldContentBoxes", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.CheckListChapters", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.PolicyContentBoxes", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Policies", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.TestQuestions", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "CustomDocumentOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManualChapters", "CustomDocumentOrder");
            DropColumn("dbo.TestQuestions", "CustomDocumentOrder");
            DropColumn("dbo.Policies", "CustomDocumentOrder");
            DropColumn("dbo.PolicyContentBoxes", "CustomDocumentOrder");
            DropColumn("dbo.MemoContentBoxes", "CustomDocumentOrder");
            DropColumn("dbo.CheckListChapters", "CustomDocumentOrder");
            DropColumn("dbo.AcrobatFieldContentBoxes", "CustomDocumentOrder");
            DropTable("dbo.CustomDocumentAnswerSubmissions");
        }
    }
}
