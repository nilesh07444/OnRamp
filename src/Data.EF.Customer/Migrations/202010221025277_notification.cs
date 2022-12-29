namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentNotifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.String(),
                        UserId = c.String(),
                        AssignedDate = c.DateTime(nullable: false),
                        IsViewed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocumentNotifications");
        }
    }
}
