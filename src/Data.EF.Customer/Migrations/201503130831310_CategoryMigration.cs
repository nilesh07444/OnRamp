namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CategoryMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "ParentCategoryId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "ParentCategoryId");
        }
    }
}
