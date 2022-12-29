namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Legal_Disclaimer_ORP_912 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "LegalDisclaimer", c => c.String());
            AddColumn("dbo.Companies", "ShowLegalDisclaimerOnLoginOnlyOnce", c => c.Boolean(nullable: false));
            AddColumn("dbo.Companies", "ShowLegalDisclaimerOnLogin", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "ShowLegalDisclaimerOnLogin");
            DropColumn("dbo.Companies", "ShowLegalDisclaimerOnLoginOnlyOnce");
            DropColumn("dbo.Companies", "LegalDisclaimer");
        }
    }
}
