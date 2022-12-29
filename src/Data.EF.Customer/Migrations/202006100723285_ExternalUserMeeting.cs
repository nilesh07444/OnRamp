namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExternalUserMeeting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalMeetingUsers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MeetingId = c.String(),
                        EmailAddress = c.String(),
                        UserId = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExternalMeetingUsers");
        }
    }
}
