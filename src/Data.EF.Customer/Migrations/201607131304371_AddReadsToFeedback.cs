namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReadsToFeedback : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FeedbackReads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        ReadOn = c.DateTime(nullable: false),
                        Feedback_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Feedbacks", t => t.Feedback_Id, cascadeDelete: true)
                .Index(t => t.Feedback_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FeedbackReads", "Feedback_Id", "dbo.Feedbacks");
            DropIndex("dbo.FeedbackReads", new[] { "Feedback_Id" });
            DropTable("dbo.FeedbackReads");
        }
    }
}
