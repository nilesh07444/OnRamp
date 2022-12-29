namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1289_TestSession : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestSessions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(),
                        CurrentTestId = c.String(),
                        StartTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TestSessions");
        }
    }
}
