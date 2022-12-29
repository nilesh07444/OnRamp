namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomUserRoleTableUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomUserRoles", "ContentCreator", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "ContentApprover", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "ContentAdmin", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "PortalAdmin", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "Publisher", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "Reporter", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "UserAdmin", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "ManageTags", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomUserRoles", "ManageVirtualMeetings", c => c.Boolean(nullable: false));
            DropColumn("dbo.CustomUserRoles", "Permissions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomUserRoles", "Permissions", c => c.String());
            DropColumn("dbo.CustomUserRoles", "ManageVirtualMeetings");
            DropColumn("dbo.CustomUserRoles", "ManageTags");
            DropColumn("dbo.CustomUserRoles", "UserAdmin");
            DropColumn("dbo.CustomUserRoles", "Reporter");
            DropColumn("dbo.CustomUserRoles", "Publisher");
            DropColumn("dbo.CustomUserRoles", "PortalAdmin");
            DropColumn("dbo.CustomUserRoles", "ContentAdmin");
            DropColumn("dbo.CustomUserRoles", "ContentApprover");
            DropColumn("dbo.CustomUserRoles", "ContentCreator");
        }
    }
}
