namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsforSelfSignUpGroup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "DefaultUserExpireDays", c => c.Int());
            AddColumn("dbo.Groups", "IsforSelfSignUpGroup", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "IsforSelfSignUpGroup");
            DropColumn("dbo.Companies", "DefaultUserExpireDays");
        }
    }
}
