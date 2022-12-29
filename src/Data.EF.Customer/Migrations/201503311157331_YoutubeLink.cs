namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YoutubeLink : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChapterLinks", "ChapterUploadSequence", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChapterLinks", "ChapterUploadSequence");
        }
    }
}
