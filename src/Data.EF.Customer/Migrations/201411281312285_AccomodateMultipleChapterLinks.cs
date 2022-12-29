namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccomodateMultipleChapterLinks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChapterLinks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Url = c.String(),
                        Type = c.Int(nullable: false),
                        TraningGuideChapter_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TraningGuideChapters", t => t.TraningGuideChapter_Id)
                .Index(t => t.TraningGuideChapter_Id);

            Sql("INSERT INTO ChapterLinks (Id,TraningGuideChapter_Id,Url,Type) SELECT NEWID(), TraningGuideChapterId, YoutubeUrl, 1 FROM TraningGuideChapters WHERE Youtubeurl is not null");
            Sql("INSERT INTO ChapterLinks (Id,TraningGuideChapter_Id,Url,Type) SELECT NEWID(), TraningGuideChapterId, ViemoUrl, 0 FROM TraningGuideChapters WHERE ViemoUrl is not null");            
            
            DropColumn("dbo.TraningGuideChapters", "YouTubeUrl");
            DropColumn("dbo.TraningGuideChapters", "ViemoUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TraningGuideChapters", "ViemoUrl", c => c.String());
            AddColumn("dbo.TraningGuideChapters", "YouTubeUrl", c => c.String());
            DropForeignKey("dbo.ChapterLinks", "TraningGuideChapter_Id", "dbo.TraningGuideChapters");
            DropIndex("dbo.ChapterLinks", new[] { "TraningGuideChapter_Id" });
            DropTable("dbo.ChapterLinks");
        }
    }
}
