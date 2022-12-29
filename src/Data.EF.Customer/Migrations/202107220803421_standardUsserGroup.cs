namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class standardUsserGroup : DbMigration
    {
        public override void Up()
        {
			CreateTable(
				"dbo.StandardUserGroups",
				c => new
				{
					Id = c.Guid(nullable: false, identity: true),
					UserId = c.Guid(),
					GroupId = c.Guid(),
					DateCreated = c.DateTime(nullable: false),
				})
				.PrimaryKey(t => t.Id);

			Sql("INSERT INTO StandardUserGroups(Id, UserId, GroupId, DateCreated)" +
				"SELECT NEWID(), Id, Group_Id, CreatedOn FROM StandardUsers ");
			
		}
        
        public override void Down()
        {
        }
    }
}
