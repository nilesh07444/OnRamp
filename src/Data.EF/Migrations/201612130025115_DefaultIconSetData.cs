namespace Data.EF.Migrations
{
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;

    public partial class DefaultIconSetData : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new CreateDefaultIconSetCommand());
        }
        
        public override void Down()
        {
        }
    }
}
