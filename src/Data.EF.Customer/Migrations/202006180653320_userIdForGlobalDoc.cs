namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userIdForGlobalDoc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapterUserResults", "UserId", c => c.String());
            AddColumn("dbo.CheckListChapterUserUploadResults", "UserId", c => c.String());
            AddColumn("dbo.CheckListUserResults", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListUserResults", "UserId");
            DropColumn("dbo.CheckListChapterUserUploadResults", "UserId");
            DropColumn("dbo.CheckListChapterUserResults", "UserId");
        }
    }
}
