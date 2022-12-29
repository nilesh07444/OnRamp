namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAssignedIdInFormFieldUserResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormFiledUserResults", "AssignedId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FormFiledUserResults", "AssignedId");
        }
    }
}
