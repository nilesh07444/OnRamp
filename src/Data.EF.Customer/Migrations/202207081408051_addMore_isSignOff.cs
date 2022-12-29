namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMore_isSignOff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestChapterUserUploadResults", "isSignOff", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestChapterUserUploadResults", "isSignOff");
        }
    }
}
