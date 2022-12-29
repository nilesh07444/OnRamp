namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentFileToChapterUploads : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingGuides", "CoverPicFileContent", c => c.Binary());
            AddColumn("dbo.ChapterUploads", "DocumentFileContent", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChapterUploads", "DocumentFileContent");
            DropColumn("dbo.TrainingGuides", "CoverPicFileContent");
        }
    }
}
