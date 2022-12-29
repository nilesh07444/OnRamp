namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Form_Section_Table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Forms",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ReferenceId = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        CreatedOn = c.DateTime(),
                        CreatedBy = c.String(),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                        CustomDocummentId = c.Guid(nullable: false),
                        CategoryId = c.String(maxLength: 128),
                        Printable = c.Boolean(nullable: false),
                        Points = c.Int(nullable: false),
                        DocumentStatus = c.Int(nullable: false),
                        CoverPictureId = c.String(maxLength: 128),
                        Deleted = c.Boolean(nullable: false),
                        TrainingLabels = c.String(),
                        CheckRequired = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentCategories", t => t.CategoryId)
                .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                .Index(t => t.CategoryId)
                .Index(t => t.CoverPictureId);
            
            CreateTable(
                "dbo.FormChapters",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FormId = c.String(maxLength: 128),
                        Title = c.String(),
                        Content = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        FormFieldTitle = c.String(),
                        FormFieldValue = c.String(),
                        IsConditionalLogic = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Forms", t => t.FormId)
                .Index(t => t.FormId);
            
            CreateTable(
                "dbo.FormChapterUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AssignedDocumentId = c.String(maxLength: 128),
                        FormChapterId = c.String(maxLength: 128),
                        UserId = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        CompletedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.FormChapters", t => t.FormChapterId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.FormChapterId);
            
            AddColumn("dbo.StandardUsers", "Form_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "FormChapter_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Uploads", "FormChapter_Id1", c => c.String(maxLength: 128));
            CreateIndex("dbo.StandardUsers", "Form_Id");
            CreateIndex("dbo.Uploads", "FormChapter_Id");
            CreateIndex("dbo.Uploads", "FormChapter_Id1");
            AddForeignKey("dbo.StandardUsers", "Form_Id", "dbo.Forms", "Id");
            AddForeignKey("dbo.Uploads", "FormChapter_Id", "dbo.FormChapters", "Id");
            AddForeignKey("dbo.Uploads", "FormChapter_Id1", "dbo.FormChapters", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FormChapterUserResults", "FormChapterId", "dbo.FormChapters");
            DropForeignKey("dbo.FormChapterUserResults", "AssignedDocumentId", "dbo.AssignedDocuments");
            DropForeignKey("dbo.Uploads", "FormChapter_Id1", "dbo.FormChapters");
            DropForeignKey("dbo.FormChapters", "FormId", "dbo.Forms");
            DropForeignKey("dbo.Uploads", "FormChapter_Id", "dbo.FormChapters");
            DropForeignKey("dbo.Forms", "CoverPictureId", "dbo.Uploads");
            DropForeignKey("dbo.StandardUsers", "Form_Id", "dbo.Forms");
            DropForeignKey("dbo.Forms", "CategoryId", "dbo.DocumentCategories");
            DropIndex("dbo.FormChapterUserResults", new[] { "FormChapterId" });
            DropIndex("dbo.FormChapterUserResults", new[] { "AssignedDocumentId" });
            DropIndex("dbo.FormChapters", new[] { "FormId" });
            DropIndex("dbo.Forms", new[] { "CoverPictureId" });
            DropIndex("dbo.Forms", new[] { "CategoryId" });
            DropIndex("dbo.Uploads", new[] { "FormChapter_Id1" });
            DropIndex("dbo.Uploads", new[] { "FormChapter_Id" });
            DropIndex("dbo.StandardUsers", new[] { "Form_Id" });
            DropColumn("dbo.Uploads", "FormChapter_Id1");
            DropColumn("dbo.Uploads", "FormChapter_Id");
            DropColumn("dbo.StandardUsers", "Form_Id");
            DropTable("dbo.FormChapterUserResults");
            DropTable("dbo.FormChapters");
            DropTable("dbo.Forms");
        }
    }
}
