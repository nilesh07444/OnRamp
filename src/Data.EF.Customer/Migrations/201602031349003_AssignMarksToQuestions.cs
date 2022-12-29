namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AssignMarksToQuestions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "AssignMarksToQuestions", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "AssignMarksToQuestions");
        }
    }
}
