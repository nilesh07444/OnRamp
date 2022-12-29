namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnableRaceCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "EnableRaceCode", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "EnableRaceCode");
        }
    }
}
