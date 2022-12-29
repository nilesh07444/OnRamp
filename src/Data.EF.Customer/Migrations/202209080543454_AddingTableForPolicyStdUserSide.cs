namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingTableForPolicyStdUserSide : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PolicyContentBoxUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        PolicyContentBoxId = c.String(maxLength: 128),
                        IsChecked = c.Boolean(nullable: false),
                        IssueDiscription = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        DateCompleted = c.DateTime(nullable: false),
                        ChapterTrackedDate = c.DateTime(),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.PolicyContentBoxes", t => t.PolicyContentBoxId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.PolicyContentBoxId);
            
            CreateTable(
                "dbo.PolicyContentBoxUserUploadResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        PolicyContentBoxId = c.String(maxLength: 128),
                        UploadId = c.String(maxLength: 128),
                        isSignOff = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        DocumentId = c.String(),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.PolicyContentBoxes", t => t.PolicyContentBoxId)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.PolicyContentBoxId)
                .Index(t => t.UploadId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PolicyContentBoxUserUploadResults", "UploadId", "dbo.Uploads");
            DropForeignKey("dbo.PolicyContentBoxUserUploadResults", "PolicyContentBoxId", "dbo.PolicyContentBoxes");
            DropForeignKey("dbo.PolicyContentBoxUserUploadResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.PolicyContentBoxUserResults", "PolicyContentBoxId", "dbo.PolicyContentBoxes");
            DropForeignKey("dbo.PolicyContentBoxUserResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropIndex("dbo.PolicyContentBoxUserUploadResults", new[] { "UploadId" });
            DropIndex("dbo.PolicyContentBoxUserUploadResults", new[] { "PolicyContentBoxId" });
            DropIndex("dbo.PolicyContentBoxUserUploadResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.PolicyContentBoxUserResults", new[] { "PolicyContentBoxId" });
            DropIndex("dbo.PolicyContentBoxUserResults", new[] { "AssignedDocumentId" });
            DropTable("dbo.PolicyContentBoxUserUploadResults");
            DropTable("dbo.PolicyContentBoxUserResults");
        }
    }
}
