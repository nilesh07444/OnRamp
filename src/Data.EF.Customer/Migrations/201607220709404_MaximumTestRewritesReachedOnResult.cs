namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MaximumTestRewritesReachedOnResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestResults", "MaximumTestRewritesReached", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestResults", "MaximumTestRewritesReached");
        }
    }
}
