namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnassignedOnUsageStats : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingGuideusageStats", "Unassigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingTestUsageStats", "Unassigned", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTestUsageStats", "Unassigned");
            DropColumn("dbo.TrainingGuideusageStats", "Unassigned");
        }
    }
}
