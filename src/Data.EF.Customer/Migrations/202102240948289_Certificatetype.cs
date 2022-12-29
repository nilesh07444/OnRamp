namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Certificatetype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Certificates", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Certificates", "Type");
        }
    }
}
