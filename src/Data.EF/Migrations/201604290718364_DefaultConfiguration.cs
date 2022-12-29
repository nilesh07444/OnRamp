namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultConfiguration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DefaultConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Certificate_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileUploads", t => t.Certificate_Id)
                .Index(t => t.Certificate_Id);
            
            CreateTable(
                "dbo.FileUploads",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Data = c.Binary(),
                        Name = c.String(),
                        MIMEType = c.String(),
                        FileType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DefaultConfigurationIdTrophyId",
                c => new
                    {
                        DefaultConfigurationId = c.Guid(nullable: false),
                        Upload_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.DefaultConfigurationId, t.Upload_Id })
                .ForeignKey("dbo.DefaultConfigurations", t => t.DefaultConfigurationId, cascadeDelete: true)
                .ForeignKey("dbo.FileUploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.DefaultConfigurationId)
                .Index(t => t.Upload_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DefaultConfigurationIdTrophyId", "Upload_Id", "dbo.FileUploads");
            DropForeignKey("dbo.DefaultConfigurationIdTrophyId", "DefaultConfigurationId", "dbo.DefaultConfigurations");
            DropForeignKey("dbo.DefaultConfigurations", "Certificate_Id", "dbo.FileUploads");
            DropIndex("dbo.DefaultConfigurationIdTrophyId", new[] { "Upload_Id" });
            DropIndex("dbo.DefaultConfigurationIdTrophyId", new[] { "DefaultConfigurationId" });
            DropIndex("dbo.DefaultConfigurations", new[] { "Certificate_Id" });
            DropTable("dbo.DefaultConfigurationIdTrophyId");
            DropTable("dbo.FileUploads");
            DropTable("dbo.DefaultConfigurations");
        }
    }
}
