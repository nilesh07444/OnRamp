namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingTest_DisableQuestionRandomization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingTests", "DisableQuestionRandomization", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingTests", "DisableQuestionRandomization");
        }
    }
}
