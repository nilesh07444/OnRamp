namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Icon_IconSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Icons",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IconType = c.Int(nullable: false),
                        Upload_Id = c.Guid(),
                        IconSet_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileUploads", t => t.Upload_Id)
                .ForeignKey("dbo.IconSets", t => t.IconSet_Id)
                .Index(t => t.Upload_Id)
                .Index(t => t.IconSet_Id);
            
            CreateTable(
                "dbo.IconSets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Deleted = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Icons", "IconSet_Id", "dbo.IconSets");
            DropForeignKey("dbo.Icons", "Upload_Id", "dbo.FileUploads");
            DropIndex("dbo.Icons", new[] { "IconSet_Id" });
            DropIndex("dbo.Icons", new[] { "Upload_Id" });
            DropTable("dbo.IconSets");
            DropTable("dbo.Icons");
        }
    }
}
