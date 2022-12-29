namespace Data.EF.Customer.Migrations
{
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;

    public partial class TestFeedbackDataSubjectToId : DbMigration
    {
        public override void Up() {
            this.Dispatch(new MigrateTestFeedbackToTestIdCommand());
        }
        
        public override void Down()
        {
        }
    }
}
