namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1281_DocumentUsage_Duration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentUsages", "Duration", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentUsages", "Duration");
        }
    }
}
