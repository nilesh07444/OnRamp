namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VersionNumberOnTests : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "Version", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "Version");
        }
    }
}
