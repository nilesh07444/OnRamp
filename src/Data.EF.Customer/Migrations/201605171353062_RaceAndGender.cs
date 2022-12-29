namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RaceAndGender : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUsers", "Gender", c => c.Int());
            AddColumn("dbo.StandardUsers", "RaceCodeId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StandardUsers", "RaceCodeId");
            DropColumn("dbo.StandardUsers", "Gender");
        }
    }
}
