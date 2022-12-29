namespace Data.EF.Migrations
{
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AppendToDefaultIconSet_Sprint16 : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new AppendToDefaultIconSet_Sprint16_Command());
        }
        
        public override void Down()
        {
        }
    }
}
