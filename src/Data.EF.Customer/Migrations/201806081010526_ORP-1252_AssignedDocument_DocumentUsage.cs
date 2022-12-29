namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1252_AssignedDocument_DocumentUsage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AssignedDocuments",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(),
                        DocumentId = c.String(),
                        DocumentType = c.Int(nullable: false),
                        AssignedBy = c.String(),
                        AssignedDate = c.DateTime(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentUsages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(),
                        DocumentId = c.String(),
                        DocumentType = c.Int(nullable: false),
                        ViewDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocumentUsages");
            DropTable("dbo.AssignedDocuments");
        }
    }
}
