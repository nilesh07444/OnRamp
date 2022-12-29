namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapterUserResults", "ChapterTrackedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListChapterUserResults", "ChapterTrackedDate");
        }
    }
}
