namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPolicyModelwithToggleCheckRequired : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PolicyContentBoxes", "CheckRequired", c => c.Boolean(nullable: false));
            DropColumn("dbo.PolicyContentBoxes", "SectionRequired");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PolicyContentBoxes", "SectionRequired", c => c.Boolean(nullable: false));
            DropColumn("dbo.PolicyContentBoxes", "CheckRequired");
        }
    }
}
