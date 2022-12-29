namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCustomFields : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Fields",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FieldName = c.String(),
                        UserId = c.String(),
                        FieldType = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateEdited = c.DateTime(),
                        DateDeleted = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Fields");
        }
    }
}
