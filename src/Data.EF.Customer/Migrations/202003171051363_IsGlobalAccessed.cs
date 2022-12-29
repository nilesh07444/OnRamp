namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsGlobalAccessed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentUsages", "IsGlobalAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentUsages", "IsGlobalAccessed");
        }
    }
}
