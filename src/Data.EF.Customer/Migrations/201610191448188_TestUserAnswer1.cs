namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestUserAnswer1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TestUserAnswers", "Answer_Id", "dbo.TestAnswers");
            DropForeignKey("dbo.TestUserAnswers", "Result_Id", "dbo.TestResults");
            DropIndex("dbo.TestUserAnswers", new[] { "Answer_Id" });
            DropIndex("dbo.TestUserAnswers", new[] { "Result_Id" });
            AlterColumn("dbo.TestUserAnswers", "Answer_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.TestUserAnswers", "Result_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.TestUserAnswers", "Answer_Id");
            CreateIndex("dbo.TestUserAnswers", "Result_Id");
            AddForeignKey("dbo.TestUserAnswers", "Answer_Id", "dbo.TestAnswers", "TestAnswerId", cascadeDelete: true);
            AddForeignKey("dbo.TestUserAnswers", "Result_Id", "dbo.TestResults", "TestResultId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestUserAnswers", "Result_Id", "dbo.TestResults");
            DropForeignKey("dbo.TestUserAnswers", "Answer_Id", "dbo.TestAnswers");
            DropIndex("dbo.TestUserAnswers", new[] { "Result_Id" });
            DropIndex("dbo.TestUserAnswers", new[] { "Answer_Id" });
            AlterColumn("dbo.TestUserAnswers", "Result_Id", c => c.Guid());
            AlterColumn("dbo.TestUserAnswers", "Answer_Id", c => c.Guid());
            CreateIndex("dbo.TestUserAnswers", "Result_Id");
            CreateIndex("dbo.TestUserAnswers", "Answer_Id");
            AddForeignKey("dbo.TestUserAnswers", "Result_Id", "dbo.TestResults", "TestResultId");
            AddForeignKey("dbo.TestUserAnswers", "Answer_Id", "dbo.TestAnswers", "TestAnswerId");
        }
    }
}
