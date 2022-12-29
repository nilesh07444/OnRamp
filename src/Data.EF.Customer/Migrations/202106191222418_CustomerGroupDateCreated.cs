namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerGroupDateCreated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerGroups", "DateCreated", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerGroups", "DateCreated");
        }
    }
}
