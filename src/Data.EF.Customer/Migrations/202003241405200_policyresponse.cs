namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class policyresponse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PolicyResponses", "IsGlobalAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PolicyResponses", "IsGlobalAccessed");
        }
    }
}
