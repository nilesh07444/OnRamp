namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_IsGlobalAccessField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckLists", "IsGlobalAccessed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Memos", "IsGlobalAccessed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Policies", "IsGlobalAccessed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tests", "IsGlobalAccessed", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManuals", "IsGlobalAccessed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManuals", "IsGlobalAccessed");
            DropColumn("dbo.Tests", "IsGlobalAccessed");
            DropColumn("dbo.Policies", "IsGlobalAccessed");
            DropColumn("dbo.Memos", "IsGlobalAccessed");
            DropColumn("dbo.CheckLists", "IsGlobalAccessed");
        }
    }
}
