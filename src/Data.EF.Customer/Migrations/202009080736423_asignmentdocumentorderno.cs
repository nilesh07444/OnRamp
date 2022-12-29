namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class asignmentdocumentorderno : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssignedDocuments", "OrderNumber", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssignedDocuments", "OrderNumber");
        }
    }
}
