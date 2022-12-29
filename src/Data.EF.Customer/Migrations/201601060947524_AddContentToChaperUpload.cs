namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddContentToChaperUpload : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChapterUploads", "Content", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChapterUploads", "Content");
        }
    }
}
