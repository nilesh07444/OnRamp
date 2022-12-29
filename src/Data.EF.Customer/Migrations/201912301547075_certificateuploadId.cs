namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class certificateuploadId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalTrainingProviders", "CertificateUploadId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExternalTrainingProviders", "CertificateUploadId");
        }
    }
}
