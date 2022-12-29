namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_Conditinoal : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConditionalTables",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CustomDocumentID = c.Guid(nullable: false),
                        TestQuestion = c.String(),
                        TestAnswer = c.String(),
                        ChapterID = c.Guid(nullable: false),
                        documentType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ConditionalTables");
        }
    }
}
