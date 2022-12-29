namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StandardUserDisclaimerActivityLog_ORP_912 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StandardUserDisclaimerActivityLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Stamp = c.DateTime(nullable: false),
                        Accepted = c.Boolean(nullable: false),
                        IPAddress = c.String(),
                        User_Id = c.Guid(nullable: false),
                        StandardUser_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StandardUsers", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id)
                .Index(t => t.User_Id)
                .Index(t => t.StandardUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StandardUserDisclaimerActivityLogs", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUserDisclaimerActivityLogs", "User_Id", "dbo.StandardUsers");
            DropIndex("dbo.StandardUserDisclaimerActivityLogs", new[] { "StandardUser_Id" });
            DropIndex("dbo.StandardUserDisclaimerActivityLogs", new[] { "User_Id" });
            DropTable("dbo.StandardUserDisclaimerActivityLogs");
        }
    }
}
