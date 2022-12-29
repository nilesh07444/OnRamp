namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StandardUserColumnAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUsers", "CustomUserRoleId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUsers", "CustomUserRoleId");
        }
    }
}
