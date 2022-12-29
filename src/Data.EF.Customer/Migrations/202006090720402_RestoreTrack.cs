namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RestoreTrack : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RestoreTracks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UpdatedDate = c.DateTime(),
                        LastEditedBy = c.String(),
                        DocumentName = c.String(),
                        DocumentId = c.String(),
                        UserName = c.String(),
                        DocumentType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RestoreTracks");
        }
    }
}
