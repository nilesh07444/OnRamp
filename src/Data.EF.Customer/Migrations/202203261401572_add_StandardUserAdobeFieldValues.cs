namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_StandardUserAdobeFieldValues : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StandardUserAdobeFieldValues",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Field_Name = c.String(),
                        Field_Value = c.String(),
                        User_ID = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StandardUserAdobeFieldValues");
        }
    }
}
