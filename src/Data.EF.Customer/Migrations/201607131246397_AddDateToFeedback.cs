namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDateToFeedback : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Feedbacks", "FeedbackDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Feedbacks", "FeedbackDate");
        }
    }
}
