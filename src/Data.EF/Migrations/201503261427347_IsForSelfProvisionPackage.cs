namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsForSelfProvisionPackage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "IsForSelfProvision", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Packages", "IsForSelfProvision");
        }
    }
}
