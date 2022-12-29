namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsForSelfProvision : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsForSelfProvision", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsForSelfProvision");
        }
    }
}
