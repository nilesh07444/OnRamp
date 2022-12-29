namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_noteAllow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapters", "NoteAllow", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
    
        }
    }
}
