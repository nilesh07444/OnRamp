namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomUserRole : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomUserRoles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        Title = c.String(),
                        Permissions = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(),
                        DateEdited = c.DateTime(),
                        DateDeleted = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CustomUserRoles");
        }
    }
}
