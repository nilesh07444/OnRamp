namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class jitsiServer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "JitsiServerName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "JitsiServerName");
        }
    }
}
