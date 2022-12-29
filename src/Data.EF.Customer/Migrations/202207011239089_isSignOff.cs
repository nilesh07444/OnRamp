namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isSignOff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingManualChapterUserUploadResults", "isSignOff", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManualChapterUserUploadResults", "isSignOff");
        }
    }
}
