namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class documenturl : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentUrls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.String(),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocumentUrls");
        }
    }
}
