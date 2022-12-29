namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CutomerConfiguration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Deleted = c.Boolean(),
                        Version = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Company_Id = c.Guid(),
                        Upload_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .ForeignKey("dbo.FileUploads", t => t.Upload_Id)
                .Index(t => t.Company_Id)
                .Index(t => t.Upload_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomerConfigurations", "Upload_Id", "dbo.FileUploads");
            DropForeignKey("dbo.CustomerConfigurations", "Company_Id", "dbo.Companies");
            DropIndex("dbo.CustomerConfigurations", new[] { "Upload_Id" });
            DropIndex("dbo.CustomerConfigurations", new[] { "Company_Id" });
            DropTable("dbo.CustomerConfigurations");
        }
    }
}
