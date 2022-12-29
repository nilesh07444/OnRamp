namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeNo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "EmployeeNo", c => c.String(nullable:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "EmployeeNo");
        }
    }
}
