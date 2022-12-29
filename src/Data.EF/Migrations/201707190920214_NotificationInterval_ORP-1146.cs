namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationInterval_ORP1146 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "TestExpiryNotificationInterval", c => c.Int(nullable: false));
            AddColumn("dbo.Companies", "ArbitraryTestExpiryIntervalInDaysBefore", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "ArbitraryTestExpiryIntervalInDaysBefore");
            DropColumn("dbo.Companies", "TestExpiryNotificationInterval");
        }
    }
}
