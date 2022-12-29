namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VirtaulClassroom : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VirtualClassRooms",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        VirtualClassRoomName = c.String(),
                        Description = c.String(),
                        IsPasswordProtection = c.Boolean(nullable: false),
                        Password = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        LastEditDate = c.DateTime(),
                        LastEditedBy = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VirtualClassRooms");
        }
    }
}
