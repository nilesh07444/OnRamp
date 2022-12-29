namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsForSelfSignUp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsForSelfSignUp", c => c.Boolean(nullable: false, defaultValue:false));
            AddColumn("dbo.Companies", "IsSelfSignUpApprove", c => c.Boolean(nullable: false, defaultValue:true));
            AddColumn("dbo.Users", "IsFromSelfSignUp", c => c.Boolean(nullable: false, defaultValue:false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsFromSelfSignUp");
            DropColumn("dbo.Companies", "IsSelfSignUpApprove");
            DropColumn("dbo.Companies", "IsForSelfSignUp");
        }
    }
}
