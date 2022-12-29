namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestCertificatesOnStandardUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestCertificates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TestId = c.Guid(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        Passed = c.Boolean(nullable: false),
                        Upload_Id = c.Guid(),
                        User_Id = c.Guid(),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileUploads", t => t.Upload_Id)
                .ForeignKey("dbo.StandardUsers", t => t.User_Id)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.Upload_Id)
                .Index(t => t.User_Id)
                .Index(t => t.StandardUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestCertificates", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.TestCertificates", "User_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.TestCertificates", "Upload_Id", "dbo.FileUploads");
            DropIndex("dbo.TestCertificates", new[] { "StandardUser_Id" });
            DropIndex("dbo.TestCertificates", new[] { "User_Id" });
            DropIndex("dbo.TestCertificates", new[] { "Upload_Id" });
            DropTable("dbo.TestCertificates");
        }
    }
}
