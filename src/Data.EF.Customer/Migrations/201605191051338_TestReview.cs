namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestReview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "TestReview", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "TestReview");
        }
    }
}
