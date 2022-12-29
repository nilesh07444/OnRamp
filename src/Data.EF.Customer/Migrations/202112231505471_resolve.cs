namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class resolve : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Courses", "GlobalAccessEnabled", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Courses", "ExpiryEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Courses", "ExpiryEnabled", c => c.Boolean());
            AlterColumn("dbo.Courses", "GlobalAccessEnabled", c => c.Boolean());
        } 
    }
}
