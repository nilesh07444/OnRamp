namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class trackedEnable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckLists", "IsChecklistTracked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckLists", "IsChecklistTracked");
        }
    }
}
