namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerSplit : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustomerSurveyDetails", "UserId", "dbo.Users");
            DropIndex("dbo.CustomerSurveyDetails", new[] { "UserId" });
            DropTable("dbo.CustomerSurveyDetails");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CustomerSurveyDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(),
                        RatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.CustomerSurveyDetails", "UserId");
            AddForeignKey("dbo.CustomerSurveyDetails", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
    }
}
