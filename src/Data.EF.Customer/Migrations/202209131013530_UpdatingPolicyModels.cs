namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPolicyModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Yes", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_No", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Later", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Later");
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_No");
            DropColumn("dbo.PolicyContentBoxUserResults", "IsActionNeeded_Yes");
        }
    }
}
