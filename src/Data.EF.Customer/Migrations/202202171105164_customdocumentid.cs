namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customdocumentid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingManuals", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Policies", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Memos", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Checklists", "IsCustomDocument", c => c.Boolean());
            AddColumn("dbo.Tests", "IsCustomDocument", c => c.Boolean());


        }
        
        public override void Down()
        {
        }
    }
}
