namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RestoreDatabase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocuments", "IsResourceCentre", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomDocuments", "IsResourceCentre");
        }
    }
}
