namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Standard_User_Department : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUsers", "Department", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUsers", "Department");
        }
    }
}
