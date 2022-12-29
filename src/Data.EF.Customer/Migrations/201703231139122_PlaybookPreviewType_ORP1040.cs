namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlaybookPreviewType_ORP1040 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingGuides", "PlaybookPreviewMode", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingGuides", "PlaybookPreviewMode");
        }
    }
}
