namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeFeedbackSubjectDataType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Feedbacks", "Subject", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Feedbacks", "Subject", c => c.Guid(nullable: false));
        }
    }
}
