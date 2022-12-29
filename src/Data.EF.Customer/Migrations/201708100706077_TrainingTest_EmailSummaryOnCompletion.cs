namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingTest_EmailSummaryOnCompletion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "EmailSummaryOnCompletion", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "EmailSummaryOnCompletion");
        }
    }
}
