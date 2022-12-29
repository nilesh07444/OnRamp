using System.Linq;

namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1256_PackageToBundle : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Companies", "PackageId", "dbo.Packages");
            DropIndex("dbo.Companies", new[] { "PackageId" });
            CreateTable(
                "dbo.Bundles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                        Description = c.String(),
                        MaxNumberOfDocuments = c.Int(nullable: false),
                        IsForSelfProvision = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            Sql("INSERT INTO dbo.Bundles (Id, Title, Description, MaxNumberOfDocuments, IsForSelfProvision) SELECT PackageId, Title, Description, MaxNumberOfGuides, IsForSelfProvision FROM dbo.Packages");

            AddColumn("dbo.Companies", "BundleId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Companies", "BundleId");
            AddForeignKey("dbo.Companies", "BundleId", "dbo.Bundles", "Id");

            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Companies", "BundleId", "dbo.Bundles");
            DropIndex("dbo.Companies", new[] { "BundleId" });
            DropColumn("dbo.Companies", "BundleId");
            DropTable("dbo.Bundles");
            CreateIndex("dbo.Companies", "PackageId");
            AddForeignKey("dbo.Companies", "PackageId", "dbo.Packages", "PackageId");
        }
    }
}
