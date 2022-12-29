namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestVersion : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TrainingTests", "TrainingGuideId", "dbo.TrainingGuides");
            DropIndex("dbo.TrainingTests", new[] { "TrainingGuideId" });
            CreateTable(
                "dbo.TestVersions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CurrentVersion_Id = c.Guid(),
                        LastPublishedVersion_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrainingTests", t => t.CurrentVersion_Id)
                .ForeignKey("dbo.TrainingTests", t => t.LastPublishedVersion_Id)
                .ForeignKey("dbo.TrainingGuides", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.CurrentVersion_Id)
                .Index(t => t.LastPublishedVersion_Id);
            
            AddColumn("dbo.TrainingTests", "Deleted", c => c.Boolean());
            AddColumn("dbo.TrainingTests", "TestVersion_Id", c => c.Guid());
            AlterColumn("dbo.TrainingTests", "TrainingGuideId", c => c.Guid());
            CreateIndex("dbo.TrainingTests", "TrainingGuideId");
            CreateIndex("dbo.TrainingTests", "TestVersion_Id");
            AddForeignKey("dbo.TrainingTests", "TestVersion_Id", "dbo.TestVersions", "Id");
            AddForeignKey("dbo.TrainingTests", "TrainingGuideId", "dbo.TrainingGuides", "TrainingGuideId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingTests", "TrainingGuideId", "dbo.TrainingGuides");
            DropForeignKey("dbo.TrainingTests", "TestVersion_Id", "dbo.TestVersions");
            DropForeignKey("dbo.TestVersions", "Id", "dbo.TrainingGuides");
            DropForeignKey("dbo.TestVersions", "LastPublishedVersion_Id", "dbo.TrainingTests");
            DropForeignKey("dbo.TestVersions", "CurrentVersion_Id", "dbo.TrainingTests");
            DropIndex("dbo.TestVersions", new[] { "LastPublishedVersion_Id" });
            DropIndex("dbo.TestVersions", new[] { "CurrentVersion_Id" });
            DropIndex("dbo.TestVersions", new[] { "Id" });
            DropIndex("dbo.TrainingTests", new[] { "TestVersion_Id" });
            DropIndex("dbo.TrainingTests", new[] { "TrainingGuideId" });
            AlterColumn("dbo.TrainingTests", "TrainingGuideId", c => c.Guid(nullable: false));
            DropColumn("dbo.TrainingTests", "TestVersion_Id");
            DropColumn("dbo.TrainingTests", "Deleted");
            DropTable("dbo.TestVersions");
            CreateIndex("dbo.TrainingTests", "TrainingGuideId");
            AddForeignKey("dbo.TrainingTests", "TrainingGuideId", "dbo.TrainingGuides", "TrainingGuideId", cascadeDelete: true);
        }
    }
}
