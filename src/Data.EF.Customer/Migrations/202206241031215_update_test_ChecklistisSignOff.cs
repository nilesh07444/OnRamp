namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_test_ChecklistisSignOff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapters", "IsSignOff", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestQuestions", "IsSignOff", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestQuestions", "IsSignOff");
            DropColumn("dbo.CheckListChapters", "IsSignOff");
        }
    }
}
