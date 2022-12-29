namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_isSignOff1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AcrobatFieldChapterUserUploadResults", "isSignOff", c => c.Boolean(nullable: false));
            AddColumn("dbo.CheckListChapterUserUploadResults", "isSignOff", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoChapterUserUploadResults", "isSignOff", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MemoChapterUserUploadResults", "isSignOff");
            DropColumn("dbo.CheckListChapterUserUploadResults", "isSignOff");
            DropColumn("dbo.AcrobatFieldChapterUserUploadResults", "isSignOff");
        }
    }
}
