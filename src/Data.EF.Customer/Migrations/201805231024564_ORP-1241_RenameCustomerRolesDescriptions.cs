namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1241_RenameCustomerRolesDescriptions : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Manage Document Categories' WHERE RoleName = 'CategoryAdmin'");
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Create Documents' WHERE RoleName = 'ContentAdmin'");
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Standard User' WHERE RoleName = 'StandardUser'");
        }

        public override void Down()
        {
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Manage Playbook Categories' WHERE RoleName = 'CategoryAdmin'");
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Create Playbooks and Tests' WHERE RoleName = 'ContentAdmin'");
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Standard user' WHERE RoleName = 'StandardUser'");
        }
    }
}
