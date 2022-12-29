namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_column_IsCustomDocument : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckLists", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Memos", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Policies", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Tests", "IsCustomDocument", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tests", "IsCustomDocument");
            DropColumn("dbo.Policies", "IsCustomDocument");
            DropColumn("dbo.Memos", "IsCustomDocument");
            DropColumn("dbo.CheckLists", "IsCustomDocument");
        }
    }
}
