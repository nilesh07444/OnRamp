namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtensionCustomColours : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomColours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ButtonColour = c.String(),
                        FeedbackColour = c.String(),
                        FooterColour = c.String(),
                        HeaderColour = c.String(),
                        LoginColour = c.String(),
                        NavigationColour = c.String(),
                        SearchColour = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CustomColours");
        }
    }
}
