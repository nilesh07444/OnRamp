namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentWorkflowAuditMessage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentWorkflowAuditMessages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CreatorId = c.Guid(nullable: false),
                        ApproverId = c.Guid(nullable: false),
                        DocumentId = c.String(),
                        Message = c.String(),
                        DateCreated = c.DateTime(),
                        DateEdited = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocumentWorkflowAuditMessages");
        }
    }
}
