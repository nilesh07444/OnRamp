namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerAccountType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "ExpiryDate", c => c.DateTime(nullable:true));
            AddColumn("dbo.Companies", "YearlySubscription", c => c.Boolean(nullable: true));
            AddColumn("dbo.Companies", "AutoExpire", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "AutoExpire");
            DropColumn("dbo.Companies", "YearlySubscription");
            DropColumn("dbo.Companies", "ExpiryDate");
        }
    }
}
