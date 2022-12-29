namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Policy_Model : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PolicyContentBoxes", "AttachmentRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxes", "IsAttached", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxes", "NoteAllow", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxes", "IsSignOff", c => c.Boolean(nullable: false));
            AddColumn("dbo.PolicyContentBoxes", "IsConditionalLogic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PolicyContentBoxes", "IsConditionalLogic");
            DropColumn("dbo.PolicyContentBoxes", "IsSignOff");
            DropColumn("dbo.PolicyContentBoxes", "NoteAllow");
            DropColumn("dbo.PolicyContentBoxes", "IsAttached");
            DropColumn("dbo.PolicyContentBoxes", "AttachmentRequired");
        }
    }
}
