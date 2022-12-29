namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerSurveyDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerSurveyDetails", "Category", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
        }
    }
}
