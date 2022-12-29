namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsEmployeeCodeReq : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsEmployeeCodeReq", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsEmployeeCodeReq");
        }
    }
}
