namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CKEUploadsOnTrainingGuideChapter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CKEUploads",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        TrainingGuideChapter_Id = c.Guid(),
                        Upload_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TraningGuideChapters", t => t.TrainingGuideChapter_Id)
                .ForeignKey("dbo.FileUploads", t => t.Upload_Id)
                .Index(t => t.TrainingGuideChapter_Id)
                .Index(t => t.Upload_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CKEUploads", "Upload_Id", "dbo.FileUploads");
            DropForeignKey("dbo.CKEUploads", "TrainingGuideChapter_Id", "dbo.TraningGuideChapters");
            DropIndex("dbo.CKEUploads", new[] { "Upload_Id" });
            DropIndex("dbo.CKEUploads", new[] { "TrainingGuideChapter_Id" });
            DropTable("dbo.CKEUploads");
        }
    }
}
