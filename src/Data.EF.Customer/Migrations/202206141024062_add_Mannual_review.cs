namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_Mannual_review : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocuments", "IsMannualReview", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomDocuments", "IsMannualReview");
        }
    }
}
