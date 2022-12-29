namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_isSignOff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AcrobatFieldContentBoxes", "IsSignOff", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "IsSignOff", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "IsSignOff", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManualChapters", "IsSignOff");
            DropColumn("dbo.MemoContentBoxes", "IsSignOff");
            DropColumn("dbo.AcrobatFieldContentBoxes", "IsSignOff");
        }
    }
}
