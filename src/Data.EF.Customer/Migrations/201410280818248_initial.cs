namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            var up = Resources.GetString("UP");
            var commands = up.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var command in commands)
            {
                Sql(command);
            }
        }
        
        public override void Down()
        {
        }
    }
}
