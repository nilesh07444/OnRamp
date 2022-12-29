namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1281_Test_Result_TimeTaken : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Test_Result", "TimeTaken", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Test_Result", "TimeTaken");
        }
    }
}
