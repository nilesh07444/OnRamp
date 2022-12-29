namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1390 : DbMigration
    {
        public override void Up()
        {
            var template = "DELETE FROM [{0}]  WHERE [{1}] NOT IN (SELECT [Id] FROM [StandardUsers])";
            var sqlCommands = new[] { string.Format(template, "AssignedDocuments", "UserId"),
                                      string.Format(template,"DocumentUsages","UserId"),
                                      string.Format(template,"UserFeedbacks","CreatedById"),
                                      string.Format(template,"TestSessions","UserId"),
                                      string.Format(template,"PolicyResponses","UserId"),
                                      string.Format(template,"Test_Result","UserId")
            };
            foreach (var script in sqlCommands)
                Sql(script);
        }
        
        public override void Down()
        {
        }
    }
}
