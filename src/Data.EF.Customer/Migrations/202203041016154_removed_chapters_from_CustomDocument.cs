namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removed_chapters_from_CustomDocument : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CustomDocuments", "TrainingMannual");
            DropColumn("dbo.CustomDocuments", "CheckList");
            DropColumn("dbo.CustomDocuments", "Memo");
            DropColumn("dbo.CustomDocuments", "Test");
            DropColumn("dbo.CustomDocuments", "Policy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomDocuments", "Policy", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomDocuments", "Test", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomDocuments", "Memo", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomDocuments", "CheckList", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomDocuments", "TrainingMannual", c => c.Boolean(nullable: false));
        }
    }
}
