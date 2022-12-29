namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Text;

    public partial class TrainingActivity_FileUploads_FileUpload_CloneScript : DbMigration
    {
        public override void Up()
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO [dbo].[Uploads] ([Id],[Type],[ContentType],[Data],[Name],[Description],[Deleted],[Order],[{ForeignKey}]) ");
            sb.Append("SELECT NEWID(),[Type],[ContentType],[Data],[Name],[Description],CONVERT(BIT,0),0,[{ForeignKey}] ");
            sb.Append("FROM [dbo].[FileUploads] WHERE [{ForeignKey}] IS NOT NULL");
            this.Sql(sb.ToString().Replace("[{ForeignKey}]", "[BursaryTrainingActivityDetail_Id]"));
            this.Sql(sb.ToString().Replace("[{ForeignKey}]", "[ExternalTrainingActivityDetail_Id]"));
            this.Sql(sb.ToString().Replace("[{ForeignKey}]", "[StandardUserTrainingActivityLog_Id]"));
        }
        
        public override void Down()
        {
        }
    }
}
