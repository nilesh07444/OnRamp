namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompanyIsLock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsLock", c => c.Boolean(nullable: false, defaultValue:false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsLock");
        }
    }
}
