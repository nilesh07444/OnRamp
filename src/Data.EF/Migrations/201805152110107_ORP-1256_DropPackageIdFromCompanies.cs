namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1256_DropPackageIdFromCompanies : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Companies", "PackageId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "PackageId", c => c.Guid());
        }
    }
}
