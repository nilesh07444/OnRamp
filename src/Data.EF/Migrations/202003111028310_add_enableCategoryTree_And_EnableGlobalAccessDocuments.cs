namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_enableCategoryTree_And_EnableGlobalAccessDocuments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "EnableCategoryTree", c => c.Boolean(nullable: false));
            AddColumn("dbo.Companies", "EnableGlobalAccessDocuments", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "EnableGlobalAccessDocuments");
            DropColumn("dbo.Companies", "EnableCategoryTree");
        }
    }
}
