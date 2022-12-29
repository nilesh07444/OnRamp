namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Ramp.Contracts.CommandParameter.Migration;
    
    public partial class AppendToDefaultIconSet : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new AddIconsToDefaultIconSetCommand_ORP1047());
        }
        
        public override void Down()
        {
        }
    }
}
