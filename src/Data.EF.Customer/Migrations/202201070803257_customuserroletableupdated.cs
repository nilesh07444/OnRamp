namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customuserroletableupdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomUserRoles", "ManageAutoWorkflow", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomUserRoles", "ManageAutoWorkflow");
        }
    }
}
