namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsUserExpire : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CreatedOn", c => c.DateTime(nullable: false,defaultValue:DateTime.UtcNow));
            AddColumn("dbo.Users", "IsUserExpire", c => c.Boolean(nullable: false, defaultValue:false ));
            AddColumn("dbo.Users", "ExpireDays", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ExpireDays");
            DropColumn("dbo.Users", "IsUserExpire");
            DropColumn("dbo.Users", "CreatedOn");
        }
    }
}
