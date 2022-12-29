namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReferenceId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VirtualClassRooms", "ReferenceId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VirtualClassRooms", "ReferenceId");
        }
    }
}
