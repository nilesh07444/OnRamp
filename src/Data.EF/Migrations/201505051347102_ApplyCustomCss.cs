namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplyCustomCss : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "ApplyCustomCss", c => c.Boolean(nullable: false,defaultValue:false));
            AddColumn("dbo.Companies", "customCssFile", c => c.Binary(nullable :true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "customCssFile");
            DropColumn("dbo.Companies", "ApplyCustomCss");
        }
    }
}
