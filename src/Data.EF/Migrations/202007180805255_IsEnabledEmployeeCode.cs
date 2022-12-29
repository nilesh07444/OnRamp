namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsEnabledEmployeeCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsEnabledEmployeeCode", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsEnabledEmployeeCode");
        }
    }
}
