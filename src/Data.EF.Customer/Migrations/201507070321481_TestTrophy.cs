namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestTrophy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "TestTrophy", c => c.Binary(nullable: true));
            AddColumn("dbo.TrainingTests", "TrophyName", c => c.String(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "TrophyName");
            DropColumn("dbo.TrainingTests", "TestTrophy");
        }
    }
}
