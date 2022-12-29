namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommandAuditTrail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CommandAuditTrails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CommandType = c.String(),
                        Command = c.String(),
                        Executed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CommandAuditTrails");
        }
    }
}
