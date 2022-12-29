namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomDocument : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomDocuments",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DocumentStatus = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Printable = c.Boolean(nullable: false),
                        IsGlobalAccessed = c.Boolean(nullable: false),
                        Points = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PreviewMode = c.Int(nullable: false),
                        CoverPictureId = c.String(maxLength: 128),
                        DocumentCategoryId = c.String(maxLength: 128),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                        TrainingLabels = c.String(),
                        PublishStatus = c.Int(),
                        Approver = c.String(),
                        ApproverId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.DocumentCategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CoverPictureId)
                .Index(t => t.DocumentCategoryId);
            
            AddColumn("dbo.StandardUsers", "CustomDocument_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.CheckListChapters", "CustomDocument_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.MemoContentBoxes", "CustomDocument_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.PolicyContentBoxes", "CustomDocument_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.TestQuestions", "CustomDocument_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.TrainingManualChapters", "CustomDocument_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.StandardUsers", "CustomDocument_Id");
            CreateIndex("dbo.CheckListChapters", "CustomDocument_Id");
            CreateIndex("dbo.MemoContentBoxes", "CustomDocument_Id");
            CreateIndex("dbo.PolicyContentBoxes", "CustomDocument_Id");
            CreateIndex("dbo.TestQuestions", "CustomDocument_Id");
            CreateIndex("dbo.TrainingManualChapters", "CustomDocument_Id");
            AddForeignKey("dbo.CheckListChapters", "CustomDocument_Id", "dbo.CustomDocuments", "Id");
            AddForeignKey("dbo.StandardUsers", "CustomDocument_Id", "dbo.CustomDocuments", "Id");
            AddForeignKey("dbo.MemoContentBoxes", "CustomDocument_Id", "dbo.CustomDocuments", "Id");
            AddForeignKey("dbo.PolicyContentBoxes", "CustomDocument_Id", "dbo.CustomDocuments", "Id");
            AddForeignKey("dbo.TestQuestions", "CustomDocument_Id", "dbo.CustomDocuments", "Id");
            AddForeignKey("dbo.TrainingManualChapters", "CustomDocument_Id", "dbo.CustomDocuments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingManualChapters", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.TestQuestions", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.PolicyContentBoxes", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.MemoContentBoxes", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.CustomDocuments", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.StandardUsers", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.CheckListChapters", "CustomDocument_Id", "dbo.CustomDocuments");
            DropForeignKey("dbo.CustomDocuments", "DocumentCategoryId", "dbo.DocumentCategories");
            DropIndex("dbo.TrainingManualChapters", new[] { "CustomDocument_Id" });
            DropIndex("dbo.TestQuestions", new[] { "CustomDocument_Id" });
            DropIndex("dbo.PolicyContentBoxes", new[] { "CustomDocument_Id" });
            DropIndex("dbo.MemoContentBoxes", new[] { "CustomDocument_Id" });
            DropIndex("dbo.CustomDocuments", new[] { "DocumentCategoryId" });
            DropIndex("dbo.CustomDocuments", new[] { "CoverPictureId" });
            DropIndex("dbo.CheckListChapters", new[] { "CustomDocument_Id" });
            DropIndex("dbo.StandardUsers", new[] { "CustomDocument_Id" });
            DropColumn("dbo.TrainingManualChapters", "CustomDocument_Id");
            DropColumn("dbo.TestQuestions", "CustomDocument_Id");
            DropColumn("dbo.PolicyContentBoxes", "CustomDocument_Id");
            DropColumn("dbo.MemoContentBoxes", "CustomDocument_Id");
            DropColumn("dbo.CheckListChapters", "CustomDocument_Id");
            DropColumn("dbo.StandardUsers", "CustomDocument_Id");
            DropTable("dbo.CustomDocuments");
        }
    }
}
