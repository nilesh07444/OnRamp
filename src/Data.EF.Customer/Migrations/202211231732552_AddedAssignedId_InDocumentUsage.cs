namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAssignedId_InDocumentUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocumentMessageCenters", "AssignedDocumentId", c => c.String());
            AddColumn("dbo.DocumentUsages", "AssignedDocumentId", c => c.String());
            DropColumn("dbo.CustomDocumentMessageCenters", "AssignedId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomDocumentMessageCenters", "AssignedId", c => c.String());
            DropColumn("dbo.DocumentUsages", "AssignedDocumentId");
            DropColumn("dbo.CustomDocumentMessageCenters", "AssignedDocumentId");
        }
    }
}
