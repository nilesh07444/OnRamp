namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_StandardUserAdobeFieldValues : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUserAdobeFieldValues", "AcrobatFieldChapterId", c => c.String());
            AddColumn("dbo.StandardUserAdobeFieldValues", "DocumentId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUserAdobeFieldValues", "DocumentId");
            DropColumn("dbo.StandardUserAdobeFieldValues", "AcrobatFieldChapterId");
        }
    }
}
