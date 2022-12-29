namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RaceCodes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RaceCodes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RaceCodes");
        }
    }
}
