namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CoursesUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Courses", "AllocatedAdmins", c => c.String());
            AddColumn("dbo.Courses", "CategoryId", c => c.Guid(nullable: false));
            AddColumn("dbo.Courses", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Courses", "EndDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Courses", "EndDate");
            DropColumn("dbo.Courses", "StartDate");
            DropColumn("dbo.Courses", "CategoryId");
            DropColumn("dbo.Courses", "AllocatedAdmins");
        }
    }
}
