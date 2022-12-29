namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SetTestPointsDefaultToZero : DbMigration
    {
        public override void Up()
        {
            Sql(Resources.GetString("Up"));
        }
        
        public override void Down()
        {
        }
    }
}
