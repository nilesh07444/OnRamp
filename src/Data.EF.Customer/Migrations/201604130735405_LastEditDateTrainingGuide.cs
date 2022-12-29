namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastEditDateTrainingGuide : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingGuides", "LastEditDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingGuides", "LastEditDate");
        }
    }
}
