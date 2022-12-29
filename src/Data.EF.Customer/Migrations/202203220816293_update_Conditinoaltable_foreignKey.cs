namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_Conditinoaltable_foreignKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConditionalTables", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.ConditionalTables", "Memo_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.ConditionalTables", "Memo_Id");
            AddForeignKey("dbo.ConditionalTables", "Memo_Id", "dbo.Memos", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ConditionalTables", "Memo_Id", "dbo.Memos");
            DropIndex("dbo.ConditionalTables", new[] { "Memo_Id" });
            DropColumn("dbo.ConditionalTables", "Memo_Id");
            DropColumn("dbo.ConditionalTables", "CreatedOn");
        }
    }
}
