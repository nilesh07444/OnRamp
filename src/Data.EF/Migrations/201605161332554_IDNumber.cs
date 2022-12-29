namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IDNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IDNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IDNumber");
        }
    }
}
