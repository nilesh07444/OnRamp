namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1245_RenameCustomerRolesDescription : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Manage Documents and Categories' WHERE RoleName = 'ContentAdmin'");
        }
        
        public override void Down()
        {
            Sql("UPDATE dbo.CustomerRoles SET Description = 'Create Documents' WHERE RoleName = 'ContentAdmin'");
        }
    }
}
