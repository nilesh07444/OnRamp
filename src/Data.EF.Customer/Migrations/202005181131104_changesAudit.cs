namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesAudit : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DocumentAuditTracks", "Roles_Id", "dbo.CustomerRoles");
            DropIndex("dbo.DocumentAuditTracks", new[] { "Roles_Id" });
            AddColumn("dbo.DocumentAuditTracks", "UserName", c => c.String());
            DropColumn("dbo.DocumentAuditTracks", "Roles_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DocumentAuditTracks", "Roles_Id", c => c.Guid());
            DropColumn("dbo.DocumentAuditTracks", "UserName");
            CreateIndex("dbo.DocumentAuditTracks", "Roles_Id");
            AddForeignKey("dbo.DocumentAuditTracks", "Roles_Id", "dbo.CustomerRoles", "Id");
        }
    }
}
