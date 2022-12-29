namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CallaboratorsOnTrainingGuide : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StandardUserTrainingGuides",
                c => new
                    {
                        StandardUser_Id = c.Guid(nullable: false),
                        TrainingGuide_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.StandardUser_Id, t.TrainingGuide_Id })
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.TrainingGuides", t => t.TrainingGuide_Id, cascadeDelete: true)
                .Index(t => t.StandardUser_Id)
                .Index(t => t.TrainingGuide_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StandardUserTrainingGuides", "TrainingGuide_Id", "dbo.TrainingGuides");
            DropForeignKey("dbo.StandardUserTrainingGuides", "StandardUser_Id", "dbo.StandardUsers");
            DropIndex("dbo.StandardUserTrainingGuides", new[] { "TrainingGuide_Id" });
            DropIndex("dbo.StandardUserTrainingGuides", new[] { "StandardUser_Id" });
            DropTable("dbo.StandardUserTrainingGuides");
        }
    }
}
