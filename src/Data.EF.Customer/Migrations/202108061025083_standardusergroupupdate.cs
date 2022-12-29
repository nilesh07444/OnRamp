namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class standardusergroupupdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StandardUserGroups", "GroupId", c => c.Guid());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StandardUserGroups", "GroupId", c => c.Guid(nullable: false));
        }
    }
}
