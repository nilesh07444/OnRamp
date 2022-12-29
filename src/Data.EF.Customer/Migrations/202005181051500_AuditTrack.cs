namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuditTrack : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentAuditTracks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                        DocumentStatus = c.String(),
                        DocumentId = c.String(),
                        Roles_Id = c.Guid(),
                        User_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerRoles", t => t.Roles_Id)
                .ForeignKey("dbo.StandardUsers", t => t.User_Id)
                .Index(t => t.Roles_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DocumentAuditTracks", "User_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.DocumentAuditTracks", "Roles_Id", "dbo.CustomerRoles");
            DropIndex("dbo.DocumentAuditTracks", new[] { "User_Id" });
            DropIndex("dbo.DocumentAuditTracks", new[] { "Roles_Id" });
            DropTable("dbo.DocumentAuditTracks");
        }
    }
}
