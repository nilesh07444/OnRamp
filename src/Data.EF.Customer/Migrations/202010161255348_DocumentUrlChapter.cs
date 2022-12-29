namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentUrlChapter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentUrls", "ChapterId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentUrls", "ChapterId");
        }
    }
}
