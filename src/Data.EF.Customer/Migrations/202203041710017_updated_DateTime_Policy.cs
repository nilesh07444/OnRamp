namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updated_DateTime_Policy : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CheckLists", "CreatedOn", c => c.DateTime());
            AlterColumn("dbo.CustomDocuments", "CreatedOn", c => c.DateTime());
            AlterColumn("dbo.Memos", "CreatedOn", c => c.DateTime());
            AlterColumn("dbo.Policies", "CreatedOn", c => c.DateTime());
            AlterColumn("dbo.Tests", "CreatedOn", c => c.DateTime());
            AlterColumn("dbo.TrainingManuals", "CreatedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TrainingManuals", "CreatedOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Tests", "CreatedOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Policies", "CreatedOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Memos", "CreatedOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CustomDocuments", "CreatedOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CheckLists", "CreatedOn", c => c.DateTime(nullable: false));
        }
    }
}
