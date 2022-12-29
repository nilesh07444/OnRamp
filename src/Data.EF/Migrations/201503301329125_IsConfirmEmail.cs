namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsConfirmEmail : DbMigration
    {
        public override void Up()
        {
           AddColumn("dbo.Users", "IsConfirmEmail", c => c.Boolean(nullable: false, defaultValue:true));
        }
        
        public override void Down()
        {
           DropColumn("dbo.Users", "IsConfirmEmail");
        }
    }
}
