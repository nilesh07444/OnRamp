namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerConfigurationUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomConfigurations", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.CustomConfigurations", "Version", c => c.Int(nullable: false));
            AddColumn("dbo.CustomConfigurations", "Deleted", c => c.Boolean());
            AddColumn("dbo.CustomConfigurations", "Upload_Id", c => c.Guid());
            CreateIndex("dbo.CustomConfigurations", "Upload_Id");
            AddForeignKey("dbo.CustomConfigurations", "Upload_Id", "dbo.FileUploads", "FileId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomConfigurations", "Upload_Id", "dbo.FileUploads");
            DropIndex("dbo.CustomConfigurations", new[] { "Upload_Id" });
            DropColumn("dbo.CustomConfigurations", "Upload_Id");
            DropColumn("dbo.CustomConfigurations", "Deleted");
            DropColumn("dbo.CustomConfigurations", "Version");
            DropColumn("dbo.CustomConfigurations", "Type");
        }
    }
}
