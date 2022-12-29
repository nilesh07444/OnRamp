namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FormFieldUserResults : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FormFields",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FormFieldName = c.String(),
                        FormFieldDescription = c.String(),
                        Number = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        FormChapterId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FormChapters", t => t.FormChapterId)
                .Index(t => t.FormChapterId);
            
            CreateTable(
                "dbo.FormFiledUserResults",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                        Number = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        FormFieldId = c.String(maxLength: 128),
                        FormChapterId = c.String(maxLength: 128),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FormChapters", t => t.FormChapterId)
                .ForeignKey("dbo.FormFields", t => t.FormFieldId)
                .Index(t => t.FormFieldId)
                .Index(t => t.FormChapterId);
            
            AddColumn("dbo.FormChapters", "CheckRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.FormChapters", "CustomDocumentOrder", c => c.Int(nullable: false));
            AddColumn("dbo.FormChapters", "Number", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FormFiledUserResults", "FormFieldId", "dbo.FormFields");
            DropForeignKey("dbo.FormFiledUserResults", "FormChapterId", "dbo.FormChapters");
            DropForeignKey("dbo.FormFields", "FormChapterId", "dbo.FormChapters");
            DropIndex("dbo.FormFiledUserResults", new[] { "FormChapterId" });
            DropIndex("dbo.FormFiledUserResults", new[] { "FormFieldId" });
            DropIndex("dbo.FormFields", new[] { "FormChapterId" });
            DropColumn("dbo.FormChapters", "Number");
            DropColumn("dbo.FormChapters", "CustomDocumentOrder");
            DropColumn("dbo.FormChapters", "CheckRequired");
            DropTable("dbo.FormFiledUserResults");
            DropTable("dbo.FormFields");
        }
    }
}
