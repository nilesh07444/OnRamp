namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrespondenceContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserCorrespondenceLogs", "Content", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserCorrespondenceLogs", "Content");
        }
    }
}
