namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileUploads_Description : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileUploads", "Description", c => c.String());
            Sql("UPDATE dbo.FileUploads SET Description = Name");
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileUploads", "Description");
        }
    }
}
