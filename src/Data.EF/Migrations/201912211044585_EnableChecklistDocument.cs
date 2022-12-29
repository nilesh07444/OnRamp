namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnableChecklistDocument : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "EnableChecklistDocument", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "EnableChecklistDocument");
        }
    }
}
