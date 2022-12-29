namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionUploads : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionUploads",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DocumentType = c.Int(nullable: false),
                        DocumentName = c.String(),
                        DocumentFileContent = c.Binary(),
                        TrainingQuestion_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrainingQuestions", t => t.TrainingQuestion_Id, cascadeDelete: true)
                .Index(t => t.TrainingQuestion_Id);
            
            AddColumn("dbo.TrainingQuestions", "Image_Id", c => c.Guid());
            AddColumn("dbo.TrainingQuestions", "Video_Id", c => c.Guid());
            AddColumn("dbo.TestAnswers", "Correct", c => c.Boolean(nullable: false));
            CreateIndex("dbo.TrainingQuestions", "Image_Id");
            CreateIndex("dbo.TrainingQuestions", "Video_Id");
            AddForeignKey("dbo.TrainingQuestions", "Image_Id", "dbo.QuestionUploads", "Id");
            AddForeignKey("dbo.TrainingQuestions", "Video_Id", "dbo.QuestionUploads", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingQuestions", "Video_Id", "dbo.QuestionUploads");
            DropForeignKey("dbo.TrainingQuestions", "Image_Id", "dbo.QuestionUploads");
            DropForeignKey("dbo.QuestionUploads", "TrainingQuestion_Id", "dbo.TrainingQuestions");
            DropIndex("dbo.QuestionUploads", new[] { "TrainingQuestion_Id" });
            DropIndex("dbo.TrainingQuestions", new[] { "Video_Id" });
            DropIndex("dbo.TrainingQuestions", new[] { "Image_Id" });
            DropColumn("dbo.TestAnswers", "Correct");
            DropColumn("dbo.TrainingQuestions", "Video_Id");
            DropColumn("dbo.TrainingQuestions", "Image_Id");
            DropTable("dbo.QuestionUploads");
        }
    }
}
