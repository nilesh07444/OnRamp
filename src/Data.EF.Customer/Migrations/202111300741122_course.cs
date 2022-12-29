namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class course : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AssignedCourses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        DocumentId = c.Guid(nullable: false),
                        DocumentType = c.Int(nullable: false),
                        AssignedBy = c.Guid(nullable: false),
                        AdditionalMsg = c.String(),
                        AssignedDate = c.DateTime(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        IsRecurring = c.Boolean(nullable: false),
                        OrderNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AssociatedDocuments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(nullable: false),
                        CourseId = c.Guid(nullable: false),
                        DocumentId = c.Guid(nullable: false),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.CourseId);
            
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CreatedBy = c.Guid(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        Status = c.Int(nullable: false),
                        Points = c.Int(nullable: false),
                        CoverPicture = c.String(),
                        GlobalAccessEnabled = c.Boolean(),
                        ExpiryEnabled = c.Boolean(),
                        ExpiryInDays = c.Int(nullable: false),
                        AchievementId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        EditedOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AssociatedDocuments", "CourseId", "dbo.Courses");
            DropIndex("dbo.AssociatedDocuments", new[] { "CourseId" });
            DropTable("dbo.Courses");
            DropTable("dbo.AssociatedDocuments");
            DropTable("dbo.AssignedCourses");
        }
    }
}
