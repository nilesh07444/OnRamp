namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP_1255 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Certificates",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UploadId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.UploadId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Certificates", "UploadId", "dbo.Uploads");
            DropIndex("dbo.Certificates", new[] { "UploadId" });
            DropTable("dbo.Certificates");
        }
    }
}
