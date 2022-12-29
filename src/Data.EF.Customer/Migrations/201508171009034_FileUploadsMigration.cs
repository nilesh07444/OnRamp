namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileUploadsMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileUploads",
                c => new
                    {
                        FileId = c.Guid(nullable: false),
                        Type = c.String(),
                        ContentType = c.String(),
                        Data = c.Binary(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.FileId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FileUploads");
        }
    }
}
