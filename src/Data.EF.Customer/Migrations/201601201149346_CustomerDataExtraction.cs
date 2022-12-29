namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerDataExtraction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerSurveyDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(),
                        RatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StandardUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.StandardUsers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LayerSubDomain = c.String(),
                        ParentUserId = c.Guid(nullable: false),
                        CompanyId = c.Guid(nullable: false),
                        FirstName = c.String(),
                        ContactNumber = c.String(),
                        LastName = c.String(),
                        EmailAddress = c.String(),
                        Password = c.String(),
                        MobileNumber = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        LogoImageUrl = c.String(),
                        CompanyType = c.String(),
                        IsConfirmEmail = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsUserExpire = c.Boolean(nullable: false),
                        ExpireDays = c.Int(),
                        IsFromSelfSignUp = c.Boolean(nullable: false),
                        EmployeeNo = c.String(),
                        Group_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerGroups", t => t.Group_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.CustomerGroups",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        CompanyId = c.Guid(nullable: false),
                        IsforSelfSignUpGroup = c.Boolean(nullable: false),
                        ParentId = c.String(nullable: true),
                })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CustomerRoles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RoleName = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StandardUserActivityLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ActivityType = c.String(),
                        Description = c.String(),
                        ActivityDate = c.DateTime(nullable: false),
                        User_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StandardUsers", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.StandardUserCorrespondanceLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CorrespondenceType = c.Int(nullable: false),
                        Description = c.String(),
                        UserId = c.Guid(nullable: false),
                        CorrespondenceDate = c.DateTime(nullable: false),
                        Content = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StandardUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.StandardUserLoginStats",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LogInTime = c.DateTime(nullable: false),
                        LogOutTime = c.DateTime(),
                        LoggedInUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StandardUsers", t => t.LoggedInUser_Id, cascadeDelete: true)
                .Index(t => t.LoggedInUser_Id);
            
            CreateTable(
                "dbo.StandardUsersInCustomerRole",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        RoleId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.StandardUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.CustomerRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StandardUserLoginStats", "LoggedInUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUserCorrespondanceLogs", "UserId", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUserActivityLogs", "User_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.CustomerSurveyDetails", "UserId", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUsersInCustomerRole", "RoleId", "dbo.CustomerRoles");
            DropForeignKey("dbo.StandardUsersInCustomerRole", "UserId", "dbo.StandardUsers");
            DropForeignKey("dbo.StandardUsers", "Group_Id", "dbo.CustomerGroups");
            DropIndex("dbo.StandardUsersInCustomerRole", new[] { "RoleId" });
            DropIndex("dbo.StandardUsersInCustomerRole", new[] { "UserId" });
            DropIndex("dbo.StandardUserLoginStats", new[] { "LoggedInUser_Id" });
            DropIndex("dbo.StandardUserCorrespondanceLogs", new[] { "UserId" });
            DropIndex("dbo.StandardUserActivityLogs", new[] { "User_Id" });
            DropIndex("dbo.StandardUsers", new[] { "Group_Id" });
            DropIndex("dbo.CustomerSurveyDetails", new[] { "UserId" });
            DropTable("dbo.StandardUsersInCustomerRole");
            DropTable("dbo.StandardUserLoginStats");
            DropTable("dbo.StandardUserCorrespondanceLogs");
            DropTable("dbo.StandardUserActivityLogs");
            DropTable("dbo.CustomerRoles");
            DropTable("dbo.CustomerGroups");
            DropTable("dbo.StandardUsers");
            DropTable("dbo.CustomerSurveyDetails");
        }
    }
}
