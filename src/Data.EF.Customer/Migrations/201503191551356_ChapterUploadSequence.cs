namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChapterUploadSequence : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChapterUploads", "ChapterUploadSequence", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChapterUploads", "ChapterUploadSequence");
        }
    }
}
