namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileUploadInQuestionAndChapterUpload : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionUploads", "Upload_Id", c => c.Guid());
            AddColumn("dbo.ChapterUploads", "Upload_Id", c => c.Guid());
            CreateIndex("dbo.QuestionUploads", "Upload_Id");
            CreateIndex("dbo.ChapterUploads", "Upload_Id");
            AddForeignKey("dbo.QuestionUploads", "Upload_Id", "dbo.FileUploads", "FileId");
            AddForeignKey("dbo.ChapterUploads", "Upload_Id", "dbo.FileUploads", "FileId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChapterUploads", "Upload_Id", "dbo.FileUploads");
            DropForeignKey("dbo.QuestionUploads", "Upload_Id", "dbo.FileUploads");
            DropIndex("dbo.ChapterUploads", new[] { "Upload_Id" });
            DropIndex("dbo.QuestionUploads", new[] { "Upload_Id" });
            DropColumn("dbo.ChapterUploads", "Upload_Id");
            DropColumn("dbo.QuestionUploads", "Upload_Id");
        }
    }
}
