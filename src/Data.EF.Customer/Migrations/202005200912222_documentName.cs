namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class documentName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentAuditTracks", "DocumentName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentAuditTracks", "DocumentName");
        }
    }
}
