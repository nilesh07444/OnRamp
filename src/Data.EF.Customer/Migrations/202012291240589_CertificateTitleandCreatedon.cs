namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CertificateTitleandCreatedon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Certificates", "Title", c => c.String());
            AddColumn("dbo.Certificates", "CreatedOn", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Certificates", "CreatedOn");
            DropColumn("dbo.Certificates", "Title");
        }
    }
}
