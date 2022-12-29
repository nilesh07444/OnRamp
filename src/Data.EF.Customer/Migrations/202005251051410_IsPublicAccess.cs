namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsPublicAccess : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VirtualClassRooms", "IsPublicAccess", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.VirtualClassRooms", "IsPublicAccess");
        }
    }
}
