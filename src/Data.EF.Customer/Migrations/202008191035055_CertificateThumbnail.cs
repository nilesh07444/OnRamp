namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CertificateThumbnail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Test_Result", "CertificateThumbnailId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Test_Result", "CertificateThumbnailId");
        }
    }
}
