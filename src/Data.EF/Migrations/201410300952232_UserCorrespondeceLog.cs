namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserCorrespondeceLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserCorrespondenceLogs", "CorrespondenceType", c => c.Int(nullable: false));
            AddColumn("dbo.UserCorrespondenceLogs", "CorrespondenceDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.UserCorrespondenceLogs", "ActivityType");
            DropColumn("dbo.UserCorrespondenceLogs", "ActivityDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserCorrespondenceLogs", "ActivityDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserCorrespondenceLogs", "ActivityType", c => c.Int(nullable: false));
            DropColumn("dbo.UserCorrespondenceLogs", "CorrespondenceDate");
            DropColumn("dbo.UserCorrespondenceLogs", "CorrespondenceType");
        }
    }
}
