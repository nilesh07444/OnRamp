namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAssignedId_InCDMessageCenter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomDocumentMessageCenters", "AssignedId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomDocumentMessageCenters", "AssignedId");
        }
    }
}
