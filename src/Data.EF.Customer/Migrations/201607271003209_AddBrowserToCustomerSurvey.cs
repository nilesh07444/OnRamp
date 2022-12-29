namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBrowserToCustomerSurvey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerSurveyDetails", "Browser", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerSurveyDetails", "Browser");
        }
    }
}
