namespace Data.EF.Migrations
{
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;

    public partial class AppendToDefaultIconSet_ORP914 : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new AddDuplicateIconToDefaultIconSetCommand_ORP914());
        }
        
        public override void Down()
        {
        }
    }
}
