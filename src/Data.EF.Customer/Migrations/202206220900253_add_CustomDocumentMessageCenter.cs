namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_CustomDocumentMessageCenter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomDocumentMessageCenters",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.String(),
                        DocumentId = c.String(),
                        DocumentType = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        Status = c.Int(),
                        Messages = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CustomDocumentMessageCenters");
        }
    }
}
