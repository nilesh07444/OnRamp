namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_AdditionalMsg_Field : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssignedDocuments", "AdditionalMsg", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssignedDocuments", "AdditionalMsg");
        }
    }
}
