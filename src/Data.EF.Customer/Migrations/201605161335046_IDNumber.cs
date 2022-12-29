namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IDNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUsers", "IDNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUsers", "IDNumber");
        }
    }
}
