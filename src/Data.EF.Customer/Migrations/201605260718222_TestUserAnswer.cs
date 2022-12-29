namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestUserAnswer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestUserAnswers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Answer_Id = c.Guid(),
                        Result_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestAnswers", t => t.Answer_Id)
                .ForeignKey("dbo.TestResults", t => t.Result_Id)
                .Index(t => t.Answer_Id)
                .Index(t => t.Result_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestUserAnswers", "Result_Id", "dbo.TestResults");
            DropForeignKey("dbo.TestUserAnswers", "Answer_Id", "dbo.TestAnswers");
            DropIndex("dbo.TestUserAnswers", new[] { "Result_Id" });
            DropIndex("dbo.TestUserAnswers", new[] { "Answer_Id" });
            DropTable("dbo.TestUserAnswers");
        }
    }
}
