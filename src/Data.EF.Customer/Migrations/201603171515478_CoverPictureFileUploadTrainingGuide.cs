namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CoverPictureFileUploadTrainingGuide : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingGuides", "CoverPicture_Id", c => c.Guid());
            CreateIndex("dbo.TrainingGuides", "CoverPicture_Id");
            AddForeignKey("dbo.TrainingGuides", "CoverPicture_Id", "dbo.FileUploads", "FileId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingGuides", "CoverPicture_Id", "dbo.FileUploads");
            DropIndex("dbo.TrainingGuides", new[] { "CoverPicture_Id" });
            DropColumn("dbo.TrainingGuides", "CoverPicture_Id");
        }
    }
}
