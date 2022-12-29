namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExpiryNotificationSentOn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "ExpiryNotificationSentOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "ExpiryNotificationSentOn");
        }
    }
}
