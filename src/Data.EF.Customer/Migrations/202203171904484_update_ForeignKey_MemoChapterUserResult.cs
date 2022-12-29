namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_ForeignKey_MemoChapterUserResult : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MemoChapterUserResults", "MemoChapterId", "dbo.MemoChapters");
            DropIndex("dbo.MemoChapterUserResults", new[] { "MemoChapterId" });
            AddColumn("dbo.MemoChapterUserResults", "MemoContentBox_Id", c => c.String(maxLength: 128));
            AlterColumn("dbo.MemoChapterUserResults", "MemoChapterId", c => c.String());
            CreateIndex("dbo.MemoChapterUserResults", "MemoContentBox_Id");
            AddForeignKey("dbo.MemoChapterUserResults", "MemoContentBox_Id", "dbo.MemoContentBoxes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemoChapterUserResults", "MemoContentBox_Id", "dbo.MemoContentBoxes");
            DropIndex("dbo.MemoChapterUserResults", new[] { "MemoContentBox_Id" });
            AlterColumn("dbo.MemoChapterUserResults", "MemoChapterId", c => c.String(maxLength: 128));
            DropColumn("dbo.MemoChapterUserResults", "MemoContentBox_Id");
            CreateIndex("dbo.MemoChapterUserResults", "MemoChapterId");
            AddForeignKey("dbo.MemoChapterUserResults", "MemoChapterId", "dbo.MemoChapters", "Id");
        }
    }
}
