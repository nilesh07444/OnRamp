namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1393_TestTimedByDefault : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE [dbo].[Tests] SET EnableTimer = 1");
        }
        
        public override void Down()
        {
        }
    }
}
