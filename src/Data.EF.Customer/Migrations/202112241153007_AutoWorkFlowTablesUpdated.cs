namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoWorkFlowTablesUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AutoAssignDocuments", "Title", c => c.String());
            AddColumn("dbo.AutoAssignWorkflows", "SendNotiEnabled", c => c.Boolean(nullable: false));
            AlterColumn("dbo.AutoAssignWorkflows", "DateCreated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AutoAssignWorkflows", "DateCreated", c => c.DateTime());
            DropColumn("dbo.AutoAssignWorkflows", "SendNotiEnabled");
            DropColumn("dbo.AutoAssignDocuments", "Title");
        }
    }
}
