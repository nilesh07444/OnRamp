namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP_1239_ORP_1255 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tests", "CertificateId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Tests", "CertificateId");
            AddForeignKey("dbo.Tests", "CertificateId", "dbo.Certificates", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tests", "CertificateId", "dbo.Certificates");
            DropIndex("dbo.Tests", new[] { "CertificateId" });
            DropColumn("dbo.Tests", "CertificateId");
        }
    }
}
