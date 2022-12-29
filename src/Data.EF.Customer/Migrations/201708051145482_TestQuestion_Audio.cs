namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestQuestion_Audio : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingQuestions", "Audio_Id", c => c.Guid());
            CreateIndex("dbo.TrainingQuestions", "Audio_Id");
            AddForeignKey("dbo.TrainingQuestions", "Audio_Id", "dbo.QuestionUploads", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingQuestions", "Audio_Id", "dbo.QuestionUploads");
            DropIndex("dbo.TrainingQuestions", new[] { "Audio_Id" });
            DropColumn("dbo.TrainingQuestions", "Audio_Id");
        }
    }
}
