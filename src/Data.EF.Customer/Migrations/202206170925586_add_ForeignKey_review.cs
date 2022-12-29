namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_ForeignKey_review : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StandardUserAdobeFieldValues", "AcrobatFieldContentBox_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.StandardUserAdobeFieldValues", "AcrobatFieldContentBox_Id");
            AddForeignKey("dbo.StandardUserAdobeFieldValues", "AcrobatFieldContentBox_Id", "dbo.AcrobatFieldContentBoxes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StandardUserAdobeFieldValues", "AcrobatFieldContentBox_Id", "dbo.AcrobatFieldContentBoxes");
            DropIndex("dbo.StandardUserAdobeFieldValues", new[] { "AcrobatFieldContentBox_Id" });
            DropColumn("dbo.StandardUserAdobeFieldValues", "AcrobatFieldContentBox_Id");
        }
    }
}
