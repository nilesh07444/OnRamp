namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MasterOnIconSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.IconSets", "Master", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.IconSets", "Master");
        }
    }
}
