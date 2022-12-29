namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IconSetOnCompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IconSet_Id", c => c.Guid());
            CreateIndex("dbo.Companies", "IconSet_Id");
            AddForeignKey("dbo.Companies", "IconSet_Id", "dbo.IconSets", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Companies", "IconSet_Id", "dbo.IconSets");
            DropIndex("dbo.Companies", new[] { "IconSet_Id" });
            DropColumn("dbo.Companies", "IconSet_Id");
        }
    }
}
