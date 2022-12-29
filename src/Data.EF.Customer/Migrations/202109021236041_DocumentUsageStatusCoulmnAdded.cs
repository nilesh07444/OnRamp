namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentUsageStatusCoulmnAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentUsages", "Status", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentUsages", "Status");
        }
    }
}
