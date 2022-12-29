namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingActivityTrainingLabelsCommaSeparatedString : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TrainingLabels", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs");
            DropIndex("dbo.TrainingLabels", new[] { "StandardUserTrainingActivityLog_Id" });
            AddColumn("dbo.StandardUserTrainingActivityLogs", "TrainingLabels", c => c.String());
            DropColumn("dbo.TrainingLabels", "StandardUserTrainingActivityLog_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TrainingLabels", "StandardUserTrainingActivityLog_Id", c => c.Guid());
            DropColumn("dbo.StandardUserTrainingActivityLogs", "TrainingLabels");
            CreateIndex("dbo.TrainingLabels", "StandardUserTrainingActivityLog_Id");
            AddForeignKey("dbo.TrainingLabels", "StandardUserTrainingActivityLog_Id", "dbo.StandardUserTrainingActivityLogs", "Id");
        }
    }
}
