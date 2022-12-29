namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AssociatedDocumentUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssociatedDocuments", "Title", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssociatedDocuments", "Title");
        }
    }
}
