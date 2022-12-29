namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateed_Conditinoaltable_foreignKey_1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ConditionalTables", "Memo_Id", "dbo.Memos");
            DropIndex("dbo.ConditionalTables", new[] { "Memo_Id" });
            DropColumn("dbo.ConditionalTables", "Memo_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ConditionalTables", "Memo_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.ConditionalTables", "Memo_Id");
            AddForeignKey("dbo.ConditionalTables", "Memo_Id", "dbo.Memos", "Id");
        }
    }
}
