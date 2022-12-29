namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assignedCourseupdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssignedCourses", "CourseId", c => c.Guid(nullable: false));
            AddColumn("dbo.AssociatedDocuments", "OrderNo", c => c.Int(nullable: false));
            DropColumn("dbo.AssignedCourses", "DocumentId");
            DropColumn("dbo.AssignedCourses", "DocumentType");
            DropColumn("dbo.AssignedCourses", "OrderNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AssignedCourses", "OrderNumber", c => c.Int(nullable: false));
            AddColumn("dbo.AssignedCourses", "DocumentType", c => c.Int(nullable: false));
            AddColumn("dbo.AssignedCourses", "DocumentId", c => c.Guid(nullable: false));
            DropColumn("dbo.AssociatedDocuments", "OrderNo");
            DropColumn("dbo.AssignedCourses", "CourseId");
        }
    }
}
