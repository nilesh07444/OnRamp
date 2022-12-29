namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomDocumentFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocuments", "TrainingMannual", c => c.String());
            AddColumn("dbo.CustomDocuments", "CheckList", c => c.String());
            AddColumn("dbo.CustomDocuments", "Memo", c => c.String());
            AddColumn("dbo.CustomDocuments", "Test", c => c.String());
            AddColumn("dbo.CustomDocuments", "Policy", c => c.String());

            AddColumn("dbo.TrainingManuals", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.Policies", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.Memos", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.Checklists", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.Tests", "CustomDocummentId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
        }
    }
}
