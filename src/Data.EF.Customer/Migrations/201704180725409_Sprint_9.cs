namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sprint_9 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TrainingLabels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        TrainingGuide_Id = c.Guid(),
                        StandardUserTrainingActivityLog_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrainingGuides", t => t.TrainingGuide_Id)
                .ForeignKey("dbo.StandardUserTrainingActivityLogs", t => t.StandardUserTrainingActivityLog_Id)
                .Index(t => t.TrainingGuide_Id)
                .Index(t => t.StandardUserTrainingActivityLog_Id);
            
            CreateTable(
                "dbo.BEECertificates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Year = c.Int(),
                        Upload_Id = c.Guid(),
                        ExternalTrainingProvider_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileUploads", t => t.Upload_Id)
                .ForeignKey("dbo.ExternalTrainingProviders", t => t.ExternalTrainingProvider_Id)
                .Index(t => t.Upload_Id)
                .Index(t => t.ExternalTrainingProvider_Id);
            
            CreateTable(
                "dbo.BursaryTrainingActivityDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BursaryType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExternalTrainingActivityDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExternalTrainingProviders",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompanyName = c.String(),
                        Address = c.String(),
                        ContactNumber = c.String(),
                        ContactPerson = c.String(),
                        EmialAddress = c.String(),
                        MobileNumber = c.String(),
                        BEEStatusLevel = c.String(),
                        ExternalTrainingActivityDetail_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExternalTrainingActivityDetails", t => t.ExternalTrainingActivityDetail_Id)
                .Index(t => t.ExternalTrainingActivityDetail_Id);
            
            CreateTable(
                "dbo.InternalTrainingActivityDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MentoringAndCoachingTrainingActivityDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StandardUserTrainingActivityLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        From = c.DateTime(nullable: false),
                        To = c.DateTime(nullable: false),
                        Time = c.DateTime(nullable: false),
                        TrainingActivityType = c.Int(),
                        RewardPoints = c.Int(),
                        Venue = c.String(),
                        AdditionalInfo = c.String(),
                        CostImplication = c.Decimal(precision: 18, scale: 2),
                        LastEditDate = c.DateTime(),
                        Created = c.DateTime(nullable: false),
                        BursaryTrainingActivityDetail_Id = c.Guid(),
                        CreatedBy_Id = c.Guid(),
                        EditedBy_Id = c.Guid(),
                        ExternalTrainingActivityDetail_Id = c.Guid(),
                        InternalTrainingActivityDetail_Id = c.Guid(),
                        MentoringAndCoachingTrainingActivityDetail_Id = c.Guid(),
                        ToolboxTalkTrainingActivityDetail_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BursaryTrainingActivityDetails", t => t.BursaryTrainingActivityDetail_Id)
                .ForeignKey("dbo.StandardUsers", t => t.CreatedBy_Id)
                .ForeignKey("dbo.StandardUsers", t => t.EditedBy_Id)
                .ForeignKey("dbo.ExternalTrainingActivityDetails", t => t.ExternalTrainingActivityDetail_Id)
                .ForeignKey("dbo.InternalTrainingActivityDetails", t => t.InternalTrainingActivityDetail_Id)
                .ForeignKey("dbo.MentoringAndCoachingTrainingActivityDetails", t => t.MentoringAndCoachingTrainingActivityDetail_Id)
                .ForeignKey("dbo.ToolboxTalkTrainingActivityDetails", t => t.ToolboxTalkTrainingActivityDetail_Id)
                .Index(t => t.BursaryTrainingActivityDetail_Id)
                .Index(t => t.CreatedBy_Id)
                .Index(t => t.EditedBy_Id)
                .Index(t => t.ExternalTrainingActivityDetail_Id)
                .Index(t => t.InternalTrainingActivityDetail_Id)
                .Index(t => t.MentoringAndCoachingTrainingActivityDetail_Id)
                .Index(t => t.ToolboxTalkTrainingActivityDetail_Id);
            
            CreateTable(
                "dbo.ToolboxTalkTrainingActivityDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FileUploads", "BursaryTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.FileUploads", "ExternalTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.FileUploads", "StandardUserTrainingActivityLog_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "InternalTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id", c => c.Guid());
            CreateIndex("dbo.FileUploads", "BursaryTrainingActivityDetail_Id");
            CreateIndex("dbo.FileUploads", "ExternalTrainingActivityDetail_Id");
            CreateIndex("dbo.FileUploads", "StandardUserTrainingActivityLog_Id");
            CreateIndex("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "InternalTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id");
            AddForeignKey("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails", "Id");
            AddForeignKey("dbo.FileUploads", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails", "Id");
            AddForeignKey("dbo.FileUploads", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails", "Id");
            AddForeignKey("dbo.StandardUsers", "InternalTrainingActivityDetail_Id", "dbo.InternalTrainingActivityDetails", "Id");
            AddForeignKey("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id", "dbo.MentoringAndCoachingTrainingActivityDetails", "Id");
            AddForeignKey("dbo.FileUploads", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs", "Id");
            AddForeignKey("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id", "dbo.ToolboxTalkTrainingActivityDetails", "Id");
            AddForeignKey("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs");
            DropForeignKey("dbo.TrainingLabels", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "ToolboxTalkTrainingActivityDetail_Id", "dbo.ToolboxTalkTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id", "dbo.ToolboxTalkTrainingActivityDetails");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "MentoringAndCoachingTrainingActivityDetail_Id", "dbo.MentoringAndCoachingTrainingActivityDetails");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "InternalTrainingActivityDetail_Id", "dbo.InternalTrainingActivityDetails");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "EditedBy_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.FileUploads", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "CreatedBy_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUserTrainingActivityLogs", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id", "dbo.MentoringAndCoachingTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "InternalTrainingActivityDetail_Id", "dbo.InternalTrainingActivityDetails");
            DropForeignKey("dbo.FileUploads", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails");
            DropForeignKey("dbo.ExternalTrainingProviders", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails");
            DropForeignKey("dbo.BEECertificates", "ExternalTrainingProvider_Id", "dbo.ExternalTrainingProviders");
            DropForeignKey("dbo.FileUploads", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails");
            DropForeignKey("dbo.BEECertificates", "Upload_Id", "dbo.FileUploads");
            DropForeignKey("dbo.TrainingLabels", "TrainingGuide_Id", "dbo.TrainingGuides");
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "ToolboxTalkTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "MentoringAndCoachingTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "InternalTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "ExternalTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "EditedBy_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "CreatedBy_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLogs", new[] { "BursaryTrainingActivityDetail_Id" });
            DropIndex("dbo.ExternalTrainingProviders", new[] { "ExternalTrainingActivityDetail_Id" });
            DropIndex("dbo.BEECertificates", new[] { "ExternalTrainingProvider_Id" });
            DropIndex("dbo.BEECertificates", new[] { "Upload_Id" });
            DropIndex("dbo.TrainingLabels", new[] { "StandardUserTrainingActivityLog_Id" });
            DropIndex("dbo.TrainingLabels", new[] { "TrainingGuide_Id" });
            DropIndex("dbo.StandardUsers", new[] { "StandardUserTrainingActivityLog_Id" });
            DropIndex("dbo.StandardUsers", new[] { "ToolboxTalkTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "MentoringAndCoachingTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "InternalTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "BursaryTrainingActivityDetail_Id" });
            DropIndex("dbo.FileUploads", new[] { "StandardUserTrainingActivityLog_Id" });
            DropIndex("dbo.FileUploads", new[] { "ExternalTrainingActivityDetail_Id" });
            DropIndex("dbo.FileUploads", new[] { "BursaryTrainingActivityDetail_Id" });
            DropColumn("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id");
            DropColumn("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "InternalTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id");
            DropColumn("dbo.FileUploads", "StandardUserTrainingActivityLog_Id");
            DropColumn("dbo.FileUploads", "ExternalTrainingActivityDetail_Id");
            DropColumn("dbo.FileUploads", "BursaryTrainingActivityDetail_Id");
            DropTable("dbo.ToolboxTalkTrainingActivityDetails");
            DropTable("dbo.StandardUserTrainingActivityLogs");
            DropTable("dbo.MentoringAndCoachingTrainingActivityDetails");
            DropTable("dbo.InternalTrainingActivityDetails");
            DropTable("dbo.ExternalTrainingProviders");
            DropTable("dbo.ExternalTrainingActivityDetails");
            DropTable("dbo.BursaryTrainingActivityDetails");
            DropTable("dbo.BEECertificates");
            DropTable("dbo.TrainingLabels");
        }
    }
}
