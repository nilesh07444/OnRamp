using Ramp.Contracts.CommandParameter.Migration;

namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AppendToDefaultIconSet_Sprint9 : DbMigration
    {
        public override void Up()
        {
            this.Dispatch(new AddSprint9IconsCommand());
        }
        
        public override void Down()
        {
        }
    }
}
