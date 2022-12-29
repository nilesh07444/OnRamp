namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1243_StandardUserDisclaimerActivityLog_Deleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUserDisclaimerActivityLogs", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUserDisclaimerActivityLogs", "Deleted");
        }
    }
}
