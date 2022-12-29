namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingManualWebFrameName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentUrls", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentUrls", "Name");
        }
    }
}
