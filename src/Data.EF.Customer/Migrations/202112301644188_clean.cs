namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class clean : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Courses", "WorkflowEnabled", c => c.Boolean(nullable: false));
            DropColumn("dbo.Courses", "OrderWorkflowEnabled");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Courses", "OrderWorkflowEnabled", c => c.Boolean(nullable: false));
            DropColumn("dbo.Courses", "WorkflowEnabled");
        }
    }
}
