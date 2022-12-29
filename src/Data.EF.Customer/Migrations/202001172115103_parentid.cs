namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class parentid : DbMigration
    {
        public override void Up()
        {
           // AddColumn("dbo.CustomerGroups", "ParentId", c => c.String(nullable: true));
        }
        
        public override void Down()
        {
        }
    }
}
