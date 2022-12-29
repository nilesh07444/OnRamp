namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateed_Conditinoaltable_dateTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ConditionalTables", "CreatedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ConditionalTables", "CreatedOn", c => c.DateTime(nullable: false));
        }
    }
}
