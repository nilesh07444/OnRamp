namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPassPointsToTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "PassPoints", c => c.Int(nullable: false, defaultValue: 1 ));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "PassPoints");
        }
    }
}
