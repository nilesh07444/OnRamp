namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoAssignDocumentsAutoAssignGroups : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AutoAssignDocuments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        WorkFlowId = c.Guid(nullable: false),
                        DocumentId = c.Guid(nullable: false),
                        Type = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AutoAssignGroups",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        WorkFlowId = c.Guid(nullable: false),
                        GroupId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AutoAssignWorkflows", "DocumentListID");
            DropColumn("dbo.AutoAssignWorkflows", "GroupId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AutoAssignWorkflows", "GroupId", c => c.String());
            AddColumn("dbo.AutoAssignWorkflows", "DocumentListID", c => c.String());
            DropTable("dbo.AutoAssignGroups");
            DropTable("dbo.AutoAssignDocuments");
        }
    }
}
