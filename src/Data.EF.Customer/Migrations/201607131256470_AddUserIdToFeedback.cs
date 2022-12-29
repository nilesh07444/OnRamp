namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserIdToFeedback : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Feedbacks", "UserId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Feedbacks", "UserId");
        }
    }
}
