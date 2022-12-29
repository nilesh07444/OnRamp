namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class virtualclass : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "EnableVirtualClassRoom", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "EnableVirtualClassRoom");
        }
    }
}
