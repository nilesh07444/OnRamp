namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MaximumRewritesOnTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "MaximumNumberOfRewites", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "MaximumNumberOfRewites");
        }
    }
}
