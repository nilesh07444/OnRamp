namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class checklistchapterresult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapterUserResults", "DocumentId", c => c.String());
            AddColumn("dbo.CheckListChapterUserResults", "IsGlobalAccessed", c => c.Boolean(nullable: false));
            AddColumn("dbo.CheckListChapterUserUploadResults", "DocumentId", c => c.String());
            AddColumn("dbo.CheckListChapterUserUploadResults", "IsGlobalAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListChapterUserUploadResults", "IsGlobalAccessed");
            DropColumn("dbo.CheckListChapterUserUploadResults", "DocumentId");
            DropColumn("dbo.CheckListChapterUserResults", "IsGlobalAccessed");
            DropColumn("dbo.CheckListChapterUserResults", "DocumentId");
        }
    }
}
