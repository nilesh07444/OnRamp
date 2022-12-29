namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class publishstatusupdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TrainingManuals", "PublishStatus", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TrainingManuals", "PublishStatus", c => c.Int(nullable: false));
        }
    }
}
