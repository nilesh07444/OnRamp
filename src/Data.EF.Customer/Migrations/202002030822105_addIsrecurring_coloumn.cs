namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsrecurring_coloumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssignedDocuments", "IsRecurring", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssignedDocuments", "IsRecurring");
        }
    }
}
