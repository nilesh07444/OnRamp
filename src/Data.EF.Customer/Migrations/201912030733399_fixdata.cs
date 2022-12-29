namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixdata : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                  "dbo.CheckLists",
                  c => new
                  {
                      Id = c.String(nullable: false, maxLength: 128),
                      ReferenceId = c.String(),
                      Title = c.String(),
                      Description = c.String(),
                      DocumentStatus = c.Int(nullable: false),
                      CreatedOn = c.DateTime(nullable: false),
                      CreatedBy = c.String(),
                      Points = c.Int(nullable: false),
                      Printable = c.Boolean(nullable: false),
                      LastEditDate = c.DateTime(),
                      LastEditedBy = c.String(),
                      Deleted = c.Boolean(nullable: false),
                      TrainingLabels = c.String(),
                      PreviewMode = c.Int(nullable: false),
                      CheckListExpiresNumberDaysFromAssignment = c.Int(),
                      NotificationInteval = c.Int(nullable: false),
                      NotificationIntevalDaysBeforeExpiry = c.Int(),
                      ExpiryNotificationSentOn = c.DateTime(),
                      DocumentCategoryId = c.String(maxLength: 128),
                      CoverPictureId = c.String(maxLength: 128),
                  })
                  .PrimaryKey(t => t.Id)
                  .ForeignKey("dbo.DocumentCategories", t => t.DocumentCategoryId)
                  .ForeignKey("dbo.Uploads", t => t.CoverPictureId)
                  .Index(t => t.DocumentCategoryId)
                  .Index(t => t.CoverPictureId);



            CreateTable(
              "dbo.CheckListChapters",
              c => new
              {
                  Id = c.String(nullable: false, maxLength: 128),
                  CheckListId = c.String(maxLength: 128),
                  Title = c.String(),
                  Number = c.Int(nullable: false),
                  Content = c.String(),
                  Deleted = c.Boolean(nullable: false),
                  AttachmentRequired = c.Boolean(nullable: false),
                  IsChecked = c.Boolean(nullable: false),
              })
              .PrimaryKey(t => t.Id)
              .ForeignKey("dbo.CheckLists", t => t.CheckListId)
              .Index(t => t.CheckListId);



            CreateTable(
                "dbo.CheckListStandardUsers",
                c => new
                {
                    CheckList_Id = c.String(nullable: false, maxLength: 128),
                    StandardUser_Id = c.Guid(nullable: false),
                })
                .PrimaryKey(t => new { t.CheckList_Id, t.StandardUser_Id })
                .ForeignKey("dbo.CheckLists", t => t.CheckList_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.CheckList_Id)
                .Index(t => t.StandardUser_Id);



            CreateTable(
                "dbo.CheckListChapterContentToolsUploads",
                c => new
                {
                    CheckListChapter_Id = c.String(nullable: false, maxLength: 128),
                    Upload_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.CheckListChapter_Id, t.Upload_Id })
                .ForeignKey("dbo.CheckListChapters", t => t.CheckListChapter_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.CheckListChapter_Id)
                .Index(t => t.Upload_Id);



            CreateTable(
                "dbo.CheckListChapterUploads",
                c => new
                {
                    CheckListChapter_Id = c.String(nullable: false, maxLength: 128),
                    Upload_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.CheckListChapter_Id, t.Upload_Id })
                .ForeignKey("dbo.CheckListChapters", t => t.CheckListChapter_Id, cascadeDelete: true)
                .ForeignKey("dbo.Uploads", t => t.Upload_Id, cascadeDelete: true)
                .Index(t => t.CheckListChapter_Id)
                .Index(t => t.Upload_Id);


            CreateTable(
               "CheckListChapterUserResults",
               c => new
               {
                   Id = c.String(nullable: false, maxLength: 128),
                   AssignedDocumentId = c.String(maxLength: 128),
                   CheckListChapterId = c.String(maxLength: 128),
                   IsChecked = c.Boolean(nullable: false),
                   CreatedDate = c.DateTime(nullable: false),
                   IssueDiscription = c.String(maxLength: 128)
               })
               .PrimaryKey(t => t.Id)
               .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
               .ForeignKey("dbo.CheckListChapters", t => t.CheckListChapterId)
               .Index(t => t.AssignedDocumentId)
               .Index(t => t.CheckListChapterId);



            CreateTable(
                 "dbo.CheckListChapterUserUploadResults",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    AssignedDocumentId = c.String(maxLength: 128),
                    CheckListChapterId = c.String(maxLength: 128),
                    UploadId = c.String(maxLength: 128),
                    CreatedDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssignedDocuments", t => t.AssignedDocumentId)
                .ForeignKey("dbo.CheckListChapters", t => t.CheckListChapterId)
                .ForeignKey("dbo.Uploads", t => t.UploadId)
                .Index(t => t.AssignedDocumentId)
                .Index(t => t.CheckListChapterId)
                .Index(t => t.UploadId);


            CreateTable(
        "dbo.CheckListUserResults",
        c => new
        {
            Id = c.String(nullable: false, maxLength: 128),
            AssignedDocumentId = c.String(maxLength: 128),
            SubmittedDate = c.DateTime(nullable: false),
            Status = c.Boolean(nullable: false),
        })
        .PrimaryKey(t => t.Id)

        .Index(t => t.AssignedDocumentId);

            AddColumn("dbo.CheckListChapters", "CheckRequired", c => c.Boolean(nullable: false));

        }
        
        public override void Down()
        {
        
        }
    }
}
