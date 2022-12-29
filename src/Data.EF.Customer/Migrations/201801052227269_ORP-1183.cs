namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1183 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "HighlightAnswersOnSummary", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "HighlightAnswersOnSummary");
        }
    }
}
