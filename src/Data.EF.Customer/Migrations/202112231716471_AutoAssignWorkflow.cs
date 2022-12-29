namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoAssignWorkflow : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AutoAssignWorkflows",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        WorkflowName = c.String(),
                        DocumentListID = c.String(),
                        GroupId = c.String(),
                        DateCreated = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        SendNotiEnabled = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);


            CreateTable(
               "dbo.AutoAssignDocuments",
               c => new
               {
                   Id = c.Guid(nullable: false),
                   Title = c.String(nullable: false),
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
            AlterColumn("dbo.AutoAssignWorkflows", "DateCreated", c => c.DateTime(nullable: false));



        }
        
        public override void Down()
        {
           
        }
    }
}
