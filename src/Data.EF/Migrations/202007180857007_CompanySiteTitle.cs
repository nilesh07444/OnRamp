namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompanySiteTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "CompanySiteTitle", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "CompanySiteTitle");
        }
    }
}
