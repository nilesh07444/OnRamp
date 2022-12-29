namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ManyUsersToManyTrainingActivities_ORP1119 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails");
            DropForeignKey("dbo.ExternalTrainingProviders", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "InternalTrainingActivityDetail_Id", "dbo.InternalTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id", "dbo.MentoringAndCoachingTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id", "dbo.ToolboxTalkTrainingActivityDetails");
            DropForeignKey("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs");
            DropIndex("dbo.StandardUsers", new[] { "BursaryTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "InternalTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "MentoringAndCoachingTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "ToolboxTalkTrainingActivityDetail_Id" });
            DropIndex("dbo.StandardUsers", new[] { "StandardUserTrainingActivityLog_Id" });
            DropIndex("dbo.ExternalTrainingProviders", new[] { "ExternalTrainingActivityDetail_Id" });
            CreateTable(
                "dbo.BursaryTrainingActivityDetail_StandardUser",
                c => new
                    {
                        BursaryTrainingActivityDetail_Id = c.Guid(nullable: false),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.BursaryTrainingActivityDetail_Id, t.StandardUser_Id })
                .ForeignKey("dbo.BursaryTrainingActivityDetails", t => t.BursaryTrainingActivityDetail_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.BursaryTrainingActivityDetail_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.ExternalTrainingActivityDetail_ExternalTrainingProvider",
                c => new
                    {
                        ExternalTrainingActivityDetail_Id = c.Guid(nullable: false),
                        ExternalTrainingProvider_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.ExternalTrainingActivityDetail_Id, t.ExternalTrainingProvider_Id })
                .ForeignKey("dbo.ExternalTrainingActivityDetails", t => t.ExternalTrainingActivityDetail_Id, cascadeDelete: true)
                .ForeignKey("dbo.ExternalTrainingProviders", t => t.ExternalTrainingProvider_Id, cascadeDelete: true)
                .Index(t => t.ExternalTrainingActivityDetail_Id)
                .Index(t => t.ExternalTrainingProvider_Id);
            
            CreateTable(
                "dbo.InternalTrainingActivityDetail_StandardUser",
                c => new
                    {
                        InternalTrainingActivityDetail_Id = c.Guid(nullable: false),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.InternalTrainingActivityDetail_Id, t.StandardUser_Id })
                .ForeignKey("dbo.InternalTrainingActivityDetails", t => t.InternalTrainingActivityDetail_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.InternalTrainingActivityDetail_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.MentoringAndCoachingTrainingActivityDetail_StandardUser",
                c => new
                    {
                        MentoringAndCoachingTrainingActivityDetail_Id = c.Guid(nullable: false),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MentoringAndCoachingTrainingActivityDetail_Id, t.StandardUser_Id })
                .ForeignKey("dbo.MentoringAndCoachingTrainingActivityDetails", t => t.MentoringAndCoachingTrainingActivityDetail_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.MentoringAndCoachingTrainingActivityDetail_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.ToolboxTalkTrainingActivityDetail_StandardUser",
                c => new
                    {
                        ToolboxTalkTrainingActivityDetail_Id = c.Guid(nullable: false),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.ToolboxTalkTrainingActivityDetail_Id, t.StandardUser_Id })
                .ForeignKey("dbo.ToolboxTalkTrainingActivityDetails", t => t.ToolboxTalkTrainingActivityDetail_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.ToolboxTalkTrainingActivityDetail_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.StandardUserTrainingActivityLog_StandardUser",
                c => new
                    {
                        StandardUserTrainingActivityLog_Id = c.Guid(nullable: false),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.StandardUserTrainingActivityLog_Id, t.StandardUser_Id })
                .ForeignKey("dbo.StandardUserTrainingActivityLogs", t => t.StandardUserTrainingActivityLog_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.StandardUserTrainingActivityLog_Id)
                .Index(t => t.StandardUser_Id);
            
            DropColumn("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "InternalTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id");
            DropColumn("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id");
            DropColumn("dbo.ExternalTrainingProviders", "ExternalTrainingActivityDetail_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalTrainingProviders", "ExternalTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "InternalTrainingActivityDetail_Id", c => c.Guid());
            AddColumn("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id", c => c.Guid());
            DropForeignKey("dbo.StandardUserTrainingActivityLog_StandardUser", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUserTrainingActivityLog_StandardUser", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs");
            DropForeignKey("dbo.ToolboxTalkTrainingActivityDetail_StandardUser", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.ToolboxTalkTrainingActivityDetail_StandardUser", "ToolboxTalkTrainingActivityDetail_Id", "dbo.ToolboxTalkTrainingActivityDetails");
            DropForeignKey("dbo.MentoringAndCoachingTrainingActivityDetail_StandardUser", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.MentoringAndCoachingTrainingActivityDetail_StandardUser", "MentoringAndCoachingTrainingActivityDetail_Id", "dbo.MentoringAndCoachingTrainingActivityDetails");
            DropForeignKey("dbo.InternalTrainingActivityDetail_StandardUser", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.InternalTrainingActivityDetail_StandardUser", "InternalTrainingActivityDetail_Id", "dbo.InternalTrainingActivityDetails");
            DropForeignKey("dbo.ExternalTrainingActivityDetail_ExternalTrainingProvider", "ExternalTrainingProvider_Id", "dbo.ExternalTrainingProviders");
            DropForeignKey("dbo.ExternalTrainingActivityDetail_ExternalTrainingProvider", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails");
            DropForeignKey("dbo.BursaryTrainingActivityDetail_StandardUser", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.BursaryTrainingActivityDetail_StandardUser", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails");
            DropIndex("dbo.StandardUserTrainingActivityLog_StandardUser", new[] { "StandardUser_Id" });
            DropIndex("dbo.StandardUserTrainingActivityLog_StandardUser", new[] { "StandardUserTrainingActivityLog_Id" });
            DropIndex("dbo.ToolboxTalkTrainingActivityDetail_StandardUser", new[] { "StandardUser_Id" });
            DropIndex("dbo.ToolboxTalkTrainingActivityDetail_StandardUser", new[] { "ToolboxTalkTrainingActivityDetail_Id" });
            DropIndex("dbo.MentoringAndCoachingTrainingActivityDetail_StandardUser", new[] { "StandardUser_Id" });
            DropIndex("dbo.MentoringAndCoachingTrainingActivityDetail_StandardUser", new[] { "MentoringAndCoachingTrainingActivityDetail_Id" });
            DropIndex("dbo.InternalTrainingActivityDetail_StandardUser", new[] { "StandardUser_Id" });
            DropIndex("dbo.InternalTrainingActivityDetail_StandardUser", new[] { "InternalTrainingActivityDetail_Id" });
            DropIndex("dbo.ExternalTrainingActivityDetail_ExternalTrainingProvider", new[] { "ExternalTrainingProvider_Id" });
            DropIndex("dbo.ExternalTrainingActivityDetail_ExternalTrainingProvider", new[] { "ExternalTrainingActivityDetail_Id" });
            DropIndex("dbo.BursaryTrainingActivityDetail_StandardUser", new[] { "StandardUser_Id" });
            DropIndex("dbo.BursaryTrainingActivityDetail_StandardUser", new[] { "BursaryTrainingActivityDetail_Id" });
            DropTable("dbo.StandardUserTrainingActivityLog_StandardUser");
            DropTable("dbo.ToolboxTalkTrainingActivityDetail_StandardUser");
            DropTable("dbo.MentoringAndCoachingTrainingActivityDetail_StandardUser");
            DropTable("dbo.InternalTrainingActivityDetail_StandardUser");
            DropTable("dbo.ExternalTrainingActivityDetail_ExternalTrainingProvider");
            DropTable("dbo.BursaryTrainingActivityDetail_StandardUser");
            CreateIndex("dbo.ExternalTrainingProviders", "ExternalTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id");
            CreateIndex("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "InternalTrainingActivityDetail_Id");
            CreateIndex("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id");
            AddForeignKey("dbo.StandardUsers", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs", "Id");
            AddForeignKey("dbo.StandardUsers", "ToolboxTalkTrainingActivityDetail_Id", "dbo.ToolboxTalkTrainingActivityDetails", "Id");
            AddForeignKey("dbo.StandardUsers", "MentoringAndCoachingTrainingActivityDetail_Id", "dbo.MentoringAndCoachingTrainingActivityDetails", "Id");
            AddForeignKey("dbo.StandardUsers", "InternalTrainingActivityDetail_Id", "dbo.InternalTrainingActivityDetails", "Id");
            AddForeignKey("dbo.ExternalTrainingProviders", "ExternalTrainingActivityDetail_Id", "dbo.ExternalTrainingActivityDetails", "Id");
            AddForeignKey("dbo.StandardUsers", "BursaryTrainingActivityDetail_Id", "dbo.BursaryTrainingActivityDetails", "Id");
        }
    }
}
