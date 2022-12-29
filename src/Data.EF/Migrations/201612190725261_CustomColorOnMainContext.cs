namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomColorOnMainContext : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomColours",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ButtonColour = c.String(),
                        FeedbackColour = c.String(),
                        FooterColour = c.String(),
                        HeaderColour = c.String(),
                        LoginColour = c.String(),
                        NavigationColour = c.String(),
                        SearchColour = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomColours", "Id", "dbo.Companies");
            DropIndex("dbo.CustomColours", new[] { "Id" });
            DropTable("dbo.CustomColours");
        }
    }
}
