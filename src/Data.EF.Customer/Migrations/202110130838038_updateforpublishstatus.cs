namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateforpublishstatus : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CheckLists", "PublishStatus", c => c.Int());
            AlterColumn("dbo.Memos", "PublishStatus", c => c.Int());
            AlterColumn("dbo.Policies", "PublishStatus", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Policies", "PublishStatus", c => c.Int(nullable: false));
            AlterColumn("dbo.Memos", "PublishStatus", c => c.Int(nullable: false));
            AlterColumn("dbo.CheckLists", "PublishStatus", c => c.Int(nullable: false));
        }
    }
}
