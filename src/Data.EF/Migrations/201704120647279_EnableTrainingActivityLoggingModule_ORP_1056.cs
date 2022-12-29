namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnableTrainingActivityLoggingModule_ORP_1056 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "EnableTrainingActivityLoggingModule", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "EnableTrainingActivityLoggingModule");
        }
    }
}
