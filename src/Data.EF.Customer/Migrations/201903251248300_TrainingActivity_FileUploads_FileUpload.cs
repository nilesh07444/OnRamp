namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingActivity_FileUploads_FileUpload : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.FileUploads", new[] { "BursaryTrainingActivityDetail_Id" });
            DropIndex("dbo.FileUploads", new[] { "ExternalTrainingActivityDetail_Id" });
            DropIndex("dbo.FileUploads", new[] { "StandardUserTrainingActivityLog_Id" });
            AddColumn("dbo.Uploads", "BursaryTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.Uploads", "ExternalTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.Uploads", "StandardUserTrainingActivityLog_Id", c => c.Guid());
            CreateIndex("dbo.Uploads", "BursaryTrainingActivityDetail_Id");
            CreateIndex("dbo.Uploads", "ExternalTrainingActivityDetail_Id");
            CreateIndex("dbo.Uploads", "StandardUserTrainingActivityLog_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Uploads", new[] { "StandardUserTrainingActivityLog_Id" });
            DropIndex("dbo.Uploads", new[] { "ExternalTrainingActivityDetail_Id" });
            DropIndex("dbo.Uploads", new[] { "BursaryTrainingActivityDetail_Id" });
            DropColumn("dbo.Uploads", "StandardUserTrainingActivityLog_Id");
            DropColumn("dbo.Uploads", "ExternalTrainingActivityDetail_Id");
            DropColumn("dbo.Uploads", "BursaryTrainingActivityDetail_Id");
            CreateIndex("dbo.FileUploads", "StandardUserTrainingActivityLog_Id");
            CreateIndex("dbo.FileUploads", "ExternalTrainingActivityDetail_Id");
            CreateIndex("dbo.FileUploads", "BursaryTrainingActivityDetail_Id");
        }
    }
}
