namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingGuide_Printable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingGuides", "Printable", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingGuides", "Printable");
        }
    }
}
