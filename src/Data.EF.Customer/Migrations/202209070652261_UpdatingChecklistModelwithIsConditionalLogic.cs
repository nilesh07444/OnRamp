namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingChecklistModelwithIsConditionalLogic : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListChapters", "IsConditionalLogic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListChapters", "IsConditionalLogic");
        }
    }
}
