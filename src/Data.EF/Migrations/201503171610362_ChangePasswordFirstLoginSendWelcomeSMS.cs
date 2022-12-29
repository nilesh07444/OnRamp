namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangePasswordFirstLoginSendWelcomeSMS : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsChangePasswordFirstLogin", c => c.Boolean(nullable: false));
            AddColumn("dbo.Companies", "IsSendWelcomeSMS", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsSendWelcomeSMS");
            DropColumn("dbo.Companies", "IsChangePasswordFirstLogin");
        }
    }
}
