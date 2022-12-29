using System.Linq;

namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1256_CopyPackageIdToBundleId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Companies", "BundleId", "dbo.Bundles");

            Sql("UPDATE dbo.Companies SET BundleId = PackageId ");

            AddForeignKey("dbo.Companies", "BundleId", "dbo.Bundles", "Id");
        }
        
        public override void Down()
        {
        }
    }
}
