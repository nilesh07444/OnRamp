namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class docnotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentNotifications", "NotificationType", c => c.String());
            AddColumn("dbo.DocumentNotifications", "Message", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentNotifications", "Message");
            DropColumn("dbo.DocumentNotifications", "NotificationType");
        }
    }
}
