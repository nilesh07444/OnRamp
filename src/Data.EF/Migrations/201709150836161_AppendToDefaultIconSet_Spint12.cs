namespace Data.EF.Migrations
{
    using Ramp.Contracts.CommandParameter.Migration;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AppendToDefaultIconSet_Spint12 : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new AppendToDefaultIconSet_Spint12_Command());
        }
        
        public override void Down()
        {
        }
    }
}
