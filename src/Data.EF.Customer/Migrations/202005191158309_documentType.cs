namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class documentType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentAuditTracks", "DocumentType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentAuditTracks", "DocumentType");
        }
    }
}
