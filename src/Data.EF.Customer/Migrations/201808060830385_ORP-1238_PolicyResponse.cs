namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1238_PolicyResponse : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PolicyResponses",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PolicyId = c.String(),
                        UserId = c.String(),
                        Created = c.DateTime(nullable: false),
                        Response = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PolicyResponses");
        }
    }
}
