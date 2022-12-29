namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomUserRoleTableUpdate1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomUserRoles", "StandardUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomUserRoles", "StandardUser");
        }
    }
}
