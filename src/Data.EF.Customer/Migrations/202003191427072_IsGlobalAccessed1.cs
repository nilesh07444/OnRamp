namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsGlobalAccessed1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListUserResults", "IsGlobalAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListUserResults", "IsGlobalAccessed");
        }
    }
}
