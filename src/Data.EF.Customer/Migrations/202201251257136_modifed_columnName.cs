namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifed_columnName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingManuals", "IsCustomDocument", c => c.Boolean());
            DropColumn("dbo.TrainingManuals", "Show");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TrainingManuals", "Show", c => c.Boolean());
            DropColumn("dbo.TrainingManuals", "IsCustomDocument");
        }
    }
}
