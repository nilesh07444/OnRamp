namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestExpiryDateNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TrainingTests", "TestExpiryDate", c => c.DateTime(nullable: true));
        }

        public override void Down()
        {
        }
    }
}
