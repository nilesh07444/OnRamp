namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RestoreDB : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocuments", "IsResourceCentre", c => c.Boolean(nullable: false));
            AddColumn("dbo.CustomDocuments", "CertificateId", c => c.String(maxLength: 128));
            AddColumn("dbo.CustomDocuments", "TestExpiresNumberDaysFromAssignment", c => c.Int());
            AddColumn("dbo.CustomDocuments", "NotificationInteval", c => c.Int(nullable: false));
            AddColumn("dbo.CustomDocuments", "NotificationIntevalDaysBeforeExpiry", c => c.Int());
            AddColumn("dbo.TestQuestions", "Title", c => c.String());
            AddColumn("dbo.Test_Result", "Title", c => c.String());
            CreateIndex("dbo.CustomDocuments", "CertificateId");
            AddForeignKey("dbo.CustomDocuments", "CertificateId", "dbo.Certificates", "Id");
        }
        
        public override void Down()
        {
        }
    }
}
