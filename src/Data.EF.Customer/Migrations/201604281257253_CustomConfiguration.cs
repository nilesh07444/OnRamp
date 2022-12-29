namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomConfiguration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Certificate_Id = c.Guid(),
                        CSS_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileUploads", t => t.Certificate_Id)
                .ForeignKey("dbo.FileUploads", t => t.CSS_Id)
                .Index(t => t.Certificate_Id)
                .Index(t => t.CSS_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomConfigurations", "CSS_Id", "dbo.FileUploads");
            DropForeignKey("dbo.CustomConfigurations", "Certificate_Id", "dbo.FileUploads");
            DropIndex("dbo.CustomConfigurations", new[] { "CSS_Id" });
            DropIndex("dbo.CustomConfigurations", new[] { "Certificate_Id" });
            DropTable("dbo.CustomConfigurations");
        }
    }
}
