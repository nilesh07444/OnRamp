namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_createdOn_Statndarduser : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StandardUsers", "CreatedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StandardUsers", "CreatedOn", c => c.DateTime(nullable: false));
        }
    }
}
