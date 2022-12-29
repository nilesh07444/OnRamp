namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChapterLinksDependantOnChapters : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChapterLinks", "TraningGuideChapter_Id", "dbo.TraningGuideChapters");
            DropIndex("dbo.ChapterLinks", new[] { "TraningGuideChapter_Id" });
            AlterColumn("dbo.ChapterLinks", "TraningGuideChapter_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.ChapterLinks", "TraningGuideChapter_Id");
            AddForeignKey("dbo.ChapterLinks", "TraningGuideChapter_Id", "dbo.TraningGuideChapters", "TraningGuideChapterId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChapterLinks", "TraningGuideChapter_Id", "dbo.TraningGuideChapters");
            DropIndex("dbo.ChapterLinks", new[] { "TraningGuideChapter_Id" });
            AlterColumn("dbo.ChapterLinks", "TraningGuideChapter_Id", c => c.Guid());
            CreateIndex("dbo.ChapterLinks", "TraningGuideChapter_Id");
            AddForeignKey("dbo.ChapterLinks", "TraningGuideChapter_Id", "dbo.TraningGuideChapters", "TraningGuideChapterId");
        }
    }
}
