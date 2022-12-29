namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_conditional_logic : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingManualChapters", "IsConditionalLogic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManualChapters", "IsConditionalLogic");
        }
    }
}
