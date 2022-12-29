namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_column_customDocumentID_memo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Memos", "CustomDocummentId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Memos", "CustomDocummentId");
        }
    }
}
