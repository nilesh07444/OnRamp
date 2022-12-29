namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class datecompleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapterUserResults", "DateCompleted", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListChapterUserResults", "DateCompleted");
        }
    }
}
