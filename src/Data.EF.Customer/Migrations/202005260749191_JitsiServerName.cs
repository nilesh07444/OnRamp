namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JitsiServerName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VirtualClassRooms", "JitsiServerName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VirtualClassRooms", "JitsiServerName");
        }
    }
}
