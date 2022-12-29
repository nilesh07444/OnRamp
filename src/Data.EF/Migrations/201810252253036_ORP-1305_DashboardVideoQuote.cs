namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1305_DashboardVideoQuote : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "DashboardVideoTitle", c => c.String());
            AddColumn("dbo.Companies", "DashboardVideoDescription", c => c.String());
            AddColumn("dbo.Companies", "DashboardQuoteText", c => c.String());
            AddColumn("dbo.Companies", "DashboardQuoteAuthor", c => c.String());
            AddColumn("dbo.Companies", "DashboardVideoFile_Id", c => c.Guid());
            CreateIndex("dbo.Companies", "DashboardVideoFile_Id");
            AddForeignKey("dbo.Companies", "DashboardVideoFile_Id", "dbo.FileUploads", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Companies", "DashboardVideoFile_Id", "dbo.FileUploads");
            DropIndex("dbo.Companies", new[] { "DashboardVideoFile_Id" });
            DropColumn("dbo.Companies", "DashboardVideoFile_Id");
            DropColumn("dbo.Companies", "DashboardQuoteAuthor");
            DropColumn("dbo.Companies", "DashboardQuoteText");
            DropColumn("dbo.Companies", "DashboardVideoDescription");
            DropColumn("dbo.Companies", "DashboardVideoTitle");
        }
    }
}
