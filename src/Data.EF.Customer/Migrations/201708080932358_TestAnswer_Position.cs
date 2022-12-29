namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestAnswer_Position : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestAnswers", "Position", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestAnswers", "Position");
        }
    }
}
