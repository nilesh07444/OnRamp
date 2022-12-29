using Domain.Customer.Base;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.PolicyResponse;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.VirtualClassRooms;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer.Models.Restore;
using Domain.Customer.Models.Groups;
using Domain.Customer.Models.CustomRole;
using Domain.Customer.Models.ScheduleReport;
using Domain.Customer.Models.Forms;
using Domain.Customer.Models.Tests;
using Domain.Customer.Models.Custom_Fields;

namespace Data.EF.Customer
{
    public class CustomerContext : DbContext
    {
        public Guid id = Guid.NewGuid();

        static CustomerContext()
        {
            Database.SetInitializer(new CustomerContextInitializer());
        }

        public CustomerContext()
        {
        }

        public CustomerContext(string connectionstring)
            : base(connectionstring)
        {
        }

        public IDbSet<CourseContent> CourseContentSet { get; set; }
        public IDbSet<TrainingGuide> TrainingGuideSet { get; set; }
        public IDbSet<TraningGuideChapter> TraningGuideChapterSet { get; set; }
        public IDbSet<TestResult> TestResultSet { get; set; }
        public IDbSet<ChapterUpload> ChapterUploadSet { get; set; }
        public IDbSet<TrainingTest> TrainingTestSet { get; set; }
        public IDbSet<TrainingQuestion> TrainingQuestionSet { get; set; }
        public IDbSet<TestAnswer> TestAnswerSet { get; set; }
        public IDbSet<Categories> CategoriesSet { get; set; }
        public IDbSet<TestAssigned> AssignedTestSet { get; set; }
        public IDbSet<AssignedTrainingGuides> AssignedTrainingGuideSet { get; set; }
        public IDbSet<TrainingGuideusageStats> TrainingGuideusageStatsSet { get; set; }
        public IDbSet<FileUploads> UploadsSet { get; set; }
        public IDbSet<TrainingTestUsageStats> TrainingTestUsageStats { get; set; }
        public IDbSet<StandardUser> StandardUsers { get; set; }
        public IDbSet<CustomerGroup> Groups { get; set; }
        public IDbSet<CustomerRole> Roles { get; set; }
        public IDbSet<StandardUserActivityLog> StandardUserActivityLogs { get; set; }
        public IDbSet<StandardUserLoginStats> StandardUserLoginStats { get; set; }
        public IDbSet<StandardUserCorrespondanceLog> StandardUserCorrespondanceLogs { get; set; }
        public IDbSet<CustomerSurveyDetail> CustomerSurveyDetails { get; set; }
        public IDbSet<QuestionUpload> QuestionUpload { get; set; }
        public IDbSet<CustomConfiguration> CustomConfiguration { get; set; }
        public IDbSet<TestUserAnswer> TestUserAnswer { get; set; }
        public IDbSet<Feedback> Feedback { get; set; }
        public IDbSet<FeedbackRead> FeedbackReads { get; set; }
        public IDbSet<CKEUpload> CKEUploads { get; set; }
        public IDbSet<TestVersion> TestVersions { get; set; }
        public IDbSet<StandardUserDisclaimerActivityLog> StandardUserDisclaimerActivityLogs { get; set; }
        public IDbSet<TrainingLabel> TrainingLabels { get; set; }
        public IDbSet<StandardUserTrainingActivityLog> StandardUserTrainingActivityLogs { get; set; }
        public IDbSet<ToolboxTalkTrainingActivityDetail> ToolboxTalkTrainingActivityDetails { get; set; }
        public IDbSet<InternalTrainingActivityDetail> InternalTrainingActivityDetails { get; set; }
        public IDbSet<MentoringAndCoachingTrainingActivityDetail> MentoringAndCoachingTrainingActivityDetails { get; set; }
        public IDbSet<BursaryTrainingActivityDetail> BursaryTrainingActivityDetails { get; set; }
        public IDbSet<ExternalTrainingActivityDetail> ExternalTrainingActivityDetails { get; set; }
        public IDbSet<ExternalTrainingProvider> ExternalTrainingProvider { get; set; }
        public IDbSet<BEECertificate> BEECertificates { get; set; }
        public IDbSet<Certificate> Certificates { get; set; }
        public IDbSet<VirtualClassRoom> VirtualClassRooms { get; set; }
        public IDbSet<DocumentAuditTrack> DocumentAuditTracks { get; set; }
        public IDbSet<RestoreTrack> RestoreTracks { get; set; }
        public IDbSet<ExternalMeetingUser> ExternalMeetingUsers { get; set; }
        public IDbSet<DocumentUrl> DocumentUrls { get; set; }
        public IDbSet<DocumentNotification> DocumentNotifications { get; set; }

        public IDbSet<Test> Tests { get; set; }
        public IDbSet<TestQuestion> TestQuestions { get; set; }
        public IDbSet<TestQuestionAnswer> TestQuestionAnswers { get; set; }
        public IDbSet<TrainingManual> TrainingManuals { get; set; }
        public IDbSet<TrainingManualChapter> TrainingManualChapters { get; set; }
        public IDbSet<Policy> Policies { get; set; }
        public IDbSet<PolicyContentBox> PolicyContentBoxes { get; set; }
        public IDbSet<PolicyResponse> PolicyResponses { get; set; }
        public IDbSet<Memo> Memos { get; set; }
        public IDbSet<MemoContentBox> MemoContentBoxs { get; set; }
        public IDbSet<DocumentCategory> DocumentCategories { get; set; }
        public IDbSet<Upload> Uploads { get; set; }
        public IDbSet<Label> Labels { get; set; }
        public IDbSet<UserFeedback> UserFeedbacks { get; set; }
        public IDbSet<UserFeedbackRead> UserFeedbackReads { get; set; }
        public IDbSet<Test_Result> Test_Results { get; set; }
        public IDbSet<TestQuestion_Result> TestQuestion_Results { get; set; }
        public IDbSet<TestQuestionAnswer_Result> TestQuestionAnswer_Results { get; set; }
        public IDbSet<AssignedDocument> AssignedDocuments { get; set; }
        public IDbSet<CheckListChapterUserResult> CheckListChapterUserResults { get; set; }
        public IDbSet<DocumentUsage> DocumentUsages { get; set; }
        public IDbSet<TestSession> TestSessions { get; set; }
        public IDbSet<CheckList> CheckLists { get; set; }
        public IDbSet<CheckListChapter> CheckListChapters { get; set; }
        public IDbSet<CheckListChapterUserUploadResult> CheckListChapterUserUploadResults { get; set; }
        public IDbSet<CheckListUserResult> CheckListUserResults { get; set; }

        public IDbSet<MemoChapter> MemoChapter { get; set; }
        public IDbSet<MemoChapterUserResult> MemoChapterUserResult { get; set; }
        public IDbSet<MemoChapterUserUploadResult> MemoChapterUserUploadResult { get; set; }
        public IDbSet<CustomDocumentAnswerSubmission> CustomDocumentAnswerSubmission { get; set; }
        public IDbSet<CustomDocumentMessageCenter> CustomDocumentMessageCenter { get; set; }

        public IDbSet<AcrobatField> AcrobatField { get; set; }
        public IDbSet<AcrobatFieldContentBox> AcrobatFieldContentBoxs { get; set; }

        public IDbSet<AcrobatFieldChapter> AcrobatFieldChapter { get; set; }
        public IDbSet<AcrobatFieldChapterUserResult> AcrobatFieldChapterUserResult { get; set; }
        public IDbSet<AcrobatFieldChapterUserUploadResult> AcrobatFieldChapterUserUploadResult { get; set; }

        public IDbSet<StandardUserAdobeFieldValues> StandardUserAdobeFieldValues { get; set; }

        public IDbSet<TrainingManualChapterUserResult> TrainingManualChapterUserResult { get; set; }
        public IDbSet<TrainingManualChapterUserUploadResult> TrainingManualChapterUserUploadResult { get; set; }




        //added by softude start

        public IDbSet<PolicyContentBoxUserResult> PolicyContentBoxUserResult { get; set; }
        public IDbSet<PolicyContentBoxUserUploadResult> PolicyContentBoxUserUploadResult { get; set; }

        //forms table start
        public IDbSet<Form> Form { get; set; }
        public IDbSet<FormChapter> FormChapter { get; set; }
        public IDbSet<FormChapterUserResult> FormChapterUserResult { get; set; }
        //forms table end

        //added by softude end

        public IDbSet<TestChapterUserResult> TestChapterUserResult { get; set; }
        public IDbSet<TestChapterUserUploadResult> TestChapterUserUploadResult { get; set; }
        public IDbSet<ConditionalTable> ConditionalTable { get; set; }


        public IDbSet<StandardUserGroup> StandardUserGroup { get; set; }
        public IDbSet<CustomUserRoles> CustomUserRole { get; set; }
        public IDbSet<DocumentWorkflowAuditMessages> DocumentWorkflowAuditMessages { get; set; }
        public IDbSet<CustomDocument> CustomDocument { get; set; }
        public IDbSet<Course> CustomDoCoursecument { get; set; }
        public IDbSet<AssociatedDocument> AssociatedDocument { get; set; }
        public IDbSet<AssignedCourse> AssignedCourse { get; set; }
        public IDbSet<AutoAssignWorkflow> AutoAssignWorkflow { get; set; }

        public IDbSet<AutoAssignDocuments> AutoAssignDocuments { get; set; }
        public IDbSet<AutoAssignGroups> AutoAssignGroups { get; set; }

        public IDbSet<ScheduleReportModel> ScheduleReport { get; set; }

        public IDbSet<ScheduleReportParameter> ScheduleReportParameter { get; set; }

        public IDbSet<ScheduleReportParams> ScheduleReportParams { get; set; }

        public IDbSet<SelectedScheduleReportParameter> SelectedScheduleReportParameter { get; set; }

        public IDbSet<FormFiledUserResult> FormFiledUserResults { get; set; }

        public IDbSet<Fields> Fields { get; set; }

        public void Add<T>(T obj) where T : CustomerDomainObject
        {
            Set<T>().Add(obj);
        }

        public void Save()
        {
            base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //TODO : For Reference only
            modelBuilder.Entity<TrainingTestUsageStats>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TrainingTestUsageStatsId");

            modelBuilder.Entity<TrainingGuideusageStats>().HasKey(x => x.Id).Property(x => x.Id)
              .HasColumnName("TrainingGuideusageStatsId");

            modelBuilder.Entity<TestResult>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TestResultId");

            modelBuilder.Entity<CourseContent>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("CourseContentId");

            modelBuilder.Entity<TrainingTest>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TrainingTestId");


            modelBuilder.Entity<TrainingQuestion>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TrainingQuestionId");

            modelBuilder.Entity<TrainingGuide>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TrainingGuideId");
            modelBuilder.Entity<TestVersion>().HasOptional(x => x.CurrentVersion);
            modelBuilder.Entity<TestVersion>().HasOptional(x => x.LastPublishedVersion);
            modelBuilder.Entity<TestVersion>().HasRequired(x => x.TrainingGuide);
            modelBuilder.Entity<TraningGuideChapter>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TraningGuideChapterId");

            modelBuilder.Entity<ChapterUpload>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("ChapterUploadId");

            modelBuilder.Entity<TestAnswer>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TestAnswerId");

            modelBuilder.Entity<Categories>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("CategoryId");

            modelBuilder.Entity<TestAssigned>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("TestAssignedId");

            modelBuilder.Entity<TestAssigned>().HasRequired<TrainingTest>(s => s.Test);

            modelBuilder.Entity<AssignedTrainingGuides>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("AssignedTrainingGuideId");

            modelBuilder.Entity<FileUploads>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("FileId");

            modelBuilder.Entity<AssignedTrainingGuides>().HasRequired<TrainingGuide>(s => s.TrainingGuide);

            modelBuilder.Entity<Categories>().HasOptional(x => x.TrainingGuides);

            modelBuilder.Entity<TrainingQuestion>().HasRequired<TrainingTest>(s => s.TrainingTest)
                .WithMany(s => s.QuestionList).HasForeignKey(s => s.TrainingTestId);

            modelBuilder.Entity<TrainingGuide>()
                .HasMany(x => x.Categories)
                .WithMany(t => t.TrainingGuides)
                .Map(m =>
                {
                    m.ToTable("TrainingGuideCategories");
                    m.MapLeftKey("TrainingGuideId");
                    m.MapRightKey("CategoryId");
                });
            modelBuilder.Entity<TestAnswer>().HasRequired<TrainingQuestion>(s => s.TrainingQuestion)
                .WithMany(s => s.TestAnswerList).HasForeignKey(s => s.TrainingQuestionId);

            modelBuilder.Entity<TraningGuideChapter>().HasRequired<TrainingGuide>(s => s.TrainingGuide)
                .WithMany(s => s.ChapterList).HasForeignKey(s => s.TraningGuidId);

            modelBuilder.Entity<ChapterUpload>().HasRequired<TraningGuideChapter>(s => s.TraningGuideChapter)
                .WithMany(s => s.ChapterUploads).HasForeignKey(s => s.TraningGuideChapterId).WillCascadeOnDelete();

            modelBuilder.Entity<ChapterLink>().HasRequired<TraningGuideChapter>(s => s.TraningGuideChapter)
                .WithMany(s => s.ChapterLinks).WillCascadeOnDelete();
            modelBuilder.Entity<CKEUpload>().HasRequired(s => s.TrainingGuideChapter).WithMany(s => s.CKEUploads).WillCascadeOnDelete();

            modelBuilder.Entity<StandardUser>()
                .HasMany(x => x.Roles)
                .WithMany()
                .Map(x =>
                {
                    x.ToTable("StandardUsersInCustomerRole");
                    x.MapLeftKey("UserId");
                    x.MapRightKey("RoleId");
                });

            modelBuilder.Entity<StandardUserCorrespondanceLog>().HasKey(x => x.Id);
            modelBuilder.Entity<StandardUserCorrespondanceLog>().HasRequired(x => x.User);
            modelBuilder.Entity<CustomerSurveyDetail>().HasKey(x => x.Id);
            modelBuilder.Entity<CustomerSurveyDetail>().HasRequired(x => x.User);

            modelBuilder.Entity<StandardUserActivityLog>().HasRequired(l => l.User).WithMany().WillCascadeOnDelete();
            modelBuilder.Entity<StandardUserCorrespondanceLog>().HasRequired(l => l.User).WithMany().WillCascadeOnDelete();
            modelBuilder.Entity<StandardUserLoginStats>().HasRequired(l => l.LoggedInUser).WithMany().WillCascadeOnDelete();
            modelBuilder.Entity<QuestionUpload>().HasRequired(l => l.TrainingQuestion).WithMany().WillCascadeOnDelete();

            modelBuilder.Entity<StandardUser>().HasMany(x => x.TestCertificates).WithRequired().WillCascadeOnDelete();

            modelBuilder.Entity<Feedback>()
                .HasMany(m => m.Reads)
                .WithRequired(r => r.Feedback)
                .WillCascadeOnDelete();

            modelBuilder.Entity<TestUserAnswer>().HasRequired(x => x.Answer).WithMany().WillCascadeOnDelete(true);
            modelBuilder.Entity<TestUserAnswer>().HasRequired(x => x.Result).WithMany().WillCascadeOnDelete(true);
            modelBuilder.Entity<StandardUserDisclaimerActivityLog>().HasRequired(x => x.User).WithMany().WillCascadeOnDelete(true);

            modelBuilder.Entity<StandardUserTrainingActivityLog>().HasMany(x => x.UsersTrained).WithMany().Map(x => x.ToTable("StandardUserTrainingActivityLog_StandardUser"));
            modelBuilder.Entity<InternalTrainingActivityDetail>().HasMany(x => x.ConductedBy).WithMany().Map(x => x.ToTable("InternalTrainingActivityDetail_StandardUser"));
            modelBuilder.Entity<BursaryTrainingActivityDetail>().HasMany(x => x.ConductedBy).WithMany().Map(x => x.ToTable("BursaryTrainingActivityDetail_StandardUser"));
            modelBuilder.Entity<MentoringAndCoachingTrainingActivityDetail>().HasMany(x => x.ConductedBy).WithMany().Map(x => x.ToTable("MentoringAndCoachingTrainingActivityDetail_StandardUser"));
            modelBuilder.Entity<ExternalTrainingActivityDetail>().HasMany(x => x.ConductedBy).WithMany().Map(x => x.ToTable("ExternalTrainingActivityDetail_ExternalTrainingProvider"));
            modelBuilder.Entity<ToolboxTalkTrainingActivityDetail>().HasMany(x => x.ConductedBy).WithMany().Map(x => x.ToTable("ToolboxTalkTrainingActivityDetail_StandardUser"));

            modelBuilder.Entity<CheckList>().HasMany(x => x.Collaborators).WithMany();
            modelBuilder.Entity<CheckListChapter>().HasMany(x => x.Uploads).WithMany().Map(x => x.ToTable("CheckListChapterUploads").MapLeftKey("CheckListChapter_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<CheckListChapter>().HasMany(x => x.ContentToolsUploads).WithMany().Map(x => x.ToTable("CheckListChapterContentToolsUploads").MapLeftKey("CheckListChapter_Id").MapRightKey("Upload_Id"));



            modelBuilder.Entity<TrainingManual>().HasMany(x => x.Collaborators).WithMany();
            modelBuilder.Entity<TrainingManualChapter>().HasMany(x => x.Uploads).WithMany().Map(x => x.ToTable("TrainingManualChapterUploads").MapLeftKey("TrainingManualChapter_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<TrainingManualChapter>().HasMany(x => x.ContentToolsUploads).WithMany().Map(x => x.ToTable("TrainingManualChapterContentToolsUploads").MapLeftKey("TrainingManualChapter_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<Test>().HasMany(x => x.Collaborators).WithMany();
            modelBuilder.Entity<TestQuestion>().HasMany(x => x.Uploads).WithMany().Map(x => x.ToTable("TestQuestionUploads").MapLeftKey("TestQuestion_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<TestQuestion>().HasMany(x => x.ContentToolsUploads).WithMany().Map(x => x.ToTable("TestQuestionContentToolsUploads").MapLeftKey("TestQuestion_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<Memo>().HasMany(x => x.Collaborators).WithMany();
            modelBuilder.Entity<MemoContentBox>().HasMany(x => x.Uploads).WithMany().Map(x => x.ToTable("MemoContentBoxUploads").MapLeftKey("MemoContentBox_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<MemoContentBox>().HasMany(x => x.ContentToolsUploads).WithMany().Map(x => x.ToTable("MemoContentBoxContentToolsUploads").MapLeftKey("MemoContentBox_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<Policy>().HasMany(x => x.Collaborators).WithMany();
            modelBuilder.Entity<PolicyContentBox>().HasMany(x => x.Uploads).WithMany().Map(x => x.ToTable("PolicyContentBoxUploads").MapLeftKey("PolicyContentBox_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<PolicyContentBox>().HasMany(x => x.ContentToolsUploads).WithMany().Map(x => x.ToTable("PolicyContentBoxContentToolsUploads").MapLeftKey("PolicyContentBox_Id").MapRightKey("Upload_Id"));
            modelBuilder.Entity<TestQuestion>().HasMany(x => x.Answers).WithRequired(x => x.TestQuestion).WillCascadeOnDelete(true);
            modelBuilder.Entity<TestQuestionAnswer>().HasRequired(x => x.TestQuestion).WithMany().WillCascadeOnDelete(true);
            modelBuilder.Entity<Test>().HasOptional(x => x.Certificate).WithMany().HasForeignKey(x => x.CertificateId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Test_Result>().HasOptional(x => x.Certificate).WithMany().HasForeignKey(x => x.CertificateId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Test_Result>().HasRequired(x => x.Test).WithMany().HasForeignKey(x => x.TestId).WillCascadeOnDelete(true);
            modelBuilder.Entity<Test_Result>().Property(x => x.UserId).IsRequired();
            modelBuilder.Entity<Test_Result>().HasMany(x => x.Questions).WithMany().Map(x => x.ToTable("Test_Result_Questions_Map").MapLeftKey("Test_Result_Id").MapRightKey("TestQuestionResult_Id"));
            modelBuilder.Entity<TestQuestion_Result>().HasMany(x => x.Answers).WithMany().Map(x => x.ToTable("TestQuestion_Result_Answers_Map").MapLeftKey("TestQuestion_Result_Id").MapRightKey("TestQuestionAnswer_Result_Id"));
            modelBuilder.Entity<UserFeedbackRead>().Property(x => x.UserId).IsRequired();
            modelBuilder.Entity<UserFeedback>().Property(x => x.CreatedById).IsRequired();
            modelBuilder.Entity<UserFeedback>().Property(x => x.Type).IsRequired();
            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }
    }
}