namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPolicyModelsNEW : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded", c => c.String());
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Yes");
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_No");
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Later");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Later", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_No", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Yes", c => c.Boolean(nullable: false));
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded");
        }
    }
}
