namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CutomerConfigurationDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerConfigurations", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerConfigurations", "Description");
        }
    }
}
