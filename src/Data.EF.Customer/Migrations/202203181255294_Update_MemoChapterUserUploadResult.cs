namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_MemoChapterUserUploadResult : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MemoChapterUserUploadResults", "MemoChapterId", "dbo.MemoChapters");
            DropIndex("dbo.MemoChapterUserUploadResults", new[] { "MemoChapterId" });
            AddColumn("dbo.MemoChapterUserUploadResults", "MemoContentBox_Id", c => c.String(maxLength: 128));
            AlterColumn("dbo.MemoChapterUserUploadResults", "MemoChapterId", c => c.String());
            CreateIndex("dbo.MemoChapterUserUploadResults", "MemoContentBox_Id");
            AddForeignKey("dbo.MemoChapterUserUploadResults", "MemoContentBox_Id", "dbo.MemoContentBoxes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemoChapterUserUploadResults", "MemoContentBox_Id", "dbo.MemoContentBoxes");
            DropIndex("dbo.MemoChapterUserUploadResults", new[] { "MemoContentBox_Id" });
            AlterColumn("dbo.MemoChapterUserUploadResults", "MemoChapterId", c => c.String(maxLength: 128));
            DropColumn("dbo.MemoChapterUserUploadResults", "MemoContentBox_Id");
            CreateIndex("dbo.MemoChapterUserUploadResults", "MemoChapterId");
            AddForeignKey("dbo.MemoChapterUserUploadResults", "MemoChapterId", "dbo.MemoChapters", "Id");
        }
    }
}
