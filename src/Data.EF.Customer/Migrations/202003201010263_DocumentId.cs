namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckListUserResults", "DocumentId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckListUserResults", "DocumentId");
        }
    }
}
