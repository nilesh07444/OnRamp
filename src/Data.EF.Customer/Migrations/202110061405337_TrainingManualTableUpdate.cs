namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingManualTableUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingManuals", "PublishStatus", c => c.Int(nullable: false));
            AddColumn("dbo.TrainingManuals", "Approver", c => c.String());
            AddColumn("dbo.TrainingManuals", "ApproverId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManuals", "ApproverId");
            DropColumn("dbo.TrainingManuals", "Approver");
            DropColumn("dbo.TrainingManuals", "PublishStatus");
        }
    }
}
