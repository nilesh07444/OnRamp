namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PolicyMemoTestChecklist : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckLists", "PublishStatus", c => c.Int(nullable: false));
            AddColumn("dbo.CheckLists", "Approver", c => c.String());
            AddColumn("dbo.CheckLists", "ApproverId", c => c.Guid(nullable: false));
            AddColumn("dbo.Memos", "PublishStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Memos", "Approver", c => c.String());
            AddColumn("dbo.Memos", "ApproverId", c => c.Guid(nullable: false));
            AddColumn("dbo.Policies", "PublishStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Policies", "Approver", c => c.String());
            AddColumn("dbo.Policies", "ApproverId", c => c.Guid(nullable: false));
            AddColumn("dbo.Tests", "PublishStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Tests", "Approver", c => c.String());
            AddColumn("dbo.Tests", "ApproverId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tests", "ApproverId");
            DropColumn("dbo.Tests", "Approver");
            DropColumn("dbo.Tests", "PublishStatus");
            DropColumn("dbo.Policies", "ApproverId");
            DropColumn("dbo.Policies", "Approver");
            DropColumn("dbo.Policies", "PublishStatus");
            DropColumn("dbo.Memos", "ApproverId");
            DropColumn("dbo.Memos", "Approver");
            DropColumn("dbo.Memos", "PublishStatus");
            DropColumn("dbo.CheckLists", "ApproverId");
            DropColumn("dbo.CheckLists", "Approver");
            DropColumn("dbo.CheckLists", "PublishStatus");
        }
    }
}
