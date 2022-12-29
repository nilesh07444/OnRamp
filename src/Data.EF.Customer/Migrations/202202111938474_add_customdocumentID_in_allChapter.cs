namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_customdocumentID_in_allChapter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckLists", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.Policies", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.Tests", "CustomDocummentId", c => c.Guid(nullable: false));
            AddColumn("dbo.TrainingManuals", "CustomDocummentId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManuals", "CustomDocummentId");
            DropColumn("dbo.Tests", "CustomDocummentId");
            DropColumn("dbo.Policies", "CustomDocummentId");
            DropColumn("dbo.CheckLists", "CustomDocummentId");
        }
    }
}
