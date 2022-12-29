namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomDocumentUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocuments", "TrainingMannualId", c => c.Guid());
            AddColumn("dbo.CustomDocuments", "CheckListId", c => c.Guid());
            AddColumn("dbo.CustomDocuments", "MemoId", c => c.Guid());
            AddColumn("dbo.CustomDocuments", "TestId", c => c.Guid());
            AddColumn("dbo.CustomDocuments", "PolicyId", c => c.Guid());
            AddColumn("dbo.TrainingManuals", "Show", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManuals", "Show");
            DropColumn("dbo.CustomDocuments", "PolicyId");
            DropColumn("dbo.CustomDocuments", "TestId");
            DropColumn("dbo.CustomDocuments", "MemoId");
            DropColumn("dbo.CustomDocuments", "CheckListId");
            DropColumn("dbo.CustomDocuments", "TrainingMannualId");
        }
    }
}
