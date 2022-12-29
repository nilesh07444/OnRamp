namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RestoreTracks", "DocumentStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RestoreTracks", "DocumentStatus");
        }
    }
}
