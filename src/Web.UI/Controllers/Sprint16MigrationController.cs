using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Models;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Domain.Enums;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ramp.Contracts.CommandParameter.Certificate;
using Ramp.Services.Helpers;
using TimeSpan = System.TimeSpan;
using System.Collections.Concurrent;
using System.Data.Entity;

namespace Web.UI.Controllers
{
    public class Sprint16MigrationController : RampController
    {
        [HttpGet]
        public ViewResult Index()
        {
            return View(ExecuteQuery<Sprint16MigrationQuery, Sprint16MigrationModel>(new Sprint16MigrationQuery()));
        }
        [HttpPost]
        public ActionResult Migrate(Sprint16MigrationCommand command)
        {
            ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = command.CompanyId.ToString() });
            var result = ExecuteCommand(command);
            ExecuteCommand(new UpdateConnectionStringCommand());
            if (!command.Validation.Any())
                return new JsonResult { Data = new { Message = command.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return new JsonResult { Data = command.Validation.ToArray(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public class Sprint16MigrationModel
        {
            public IEnumerable<CompanyModelShort> Companies { get; set; }
        }
        public class Sprint16MigrationQuery { }
        public class Sprint16MigrationQueryHandler : IQueryHandler<Sprint16MigrationQuery, Sprint16MigrationModel>
        {
            private readonly IRepository<Company> _companyRepository;
            public Sprint16MigrationQueryHandler(IRepository<Company> companyRepository)
            {
                _companyRepository = companyRepository;
            }
            public Sprint16MigrationModel ExecuteQuery(Sprint16MigrationQuery query)
            {
                var result = new Sprint16MigrationModel();
                result.Companies = _companyRepository.List.Where(x => x.CompanyType == Domain.Enums.CompanyType.CustomerCompany).ToList().Select(x => Project.CompanyModelShortFrom(x)).ToArray();
                return result;
            }
        }
        public class Sprint16MigrationCommand
        {
            public Guid CompanyId { get; set; }
            public string Message { get; set; }
            public List<IValidationResult> Validation { get; set; }
        }
        public class Sprint16MigrationCommandHandler : ICommandHandlerBase<Sprint16MigrationCommand>
        {
            #region Fields

            private readonly CustomerContext _context;
            private readonly IRepository<Categories> _categoriesRepository;
            private readonly IRepository<DocumentCategory> _documentCategoryRepository;
            private readonly IRepository<TrainingLabel> _trainingLabelRepository;
            private readonly IRepository<Label> _labelRepository;
            private readonly IRepository<TrainingGuide> _trainingGuideRepository;
            private readonly IRepository<TrainingManual> _trainingManualRepository;
            private readonly IRepository<TrainingTest> _trainingTestRepository;
            private readonly IRepository<Test> _testRepository;
            private readonly IRepository<TestResult> _testResultRepository;
            private readonly IRepository<Test_Result> _newTestResultRepository;
            private readonly IRepository<TestUserAnswer> _testUserAnswerRepository;
            private readonly IRepository<TestAssigned> _testAssignedRepository;
            private readonly IRepository<AssignedDocument> _assignedDocumentRepository;
            private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;
            private readonly IRepository<Upload> _uploadRepository;
            private readonly IRepository<StandardUser> _userRepository;
            private readonly IRepository<Certificate> _certificateRepository;
            private readonly IRepository<CustomConfiguration> _customConfigurationRepository;
            private readonly IRepository<StandardUserCorrespondanceLog> _standardUserCorrespondenceLogRepository;
            private readonly IRepository<DocumentUsage> _documentUsageRepository;
            private readonly IRepository<TrainingTestUsageStats> _trainingTestUsageStatsRepository;
            private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;

            #endregion
            public Sprint16MigrationCommandHandler(
                CustomerContext context,
                IRepository<Categories> categoriesRepository,
                IRepository<DocumentCategory> documentCategoryRepository,
                IRepository<TrainingLabel> trainingLabelRepository,
                IRepository<Label> labelRepository,
                IRepository<TrainingGuide> trainingGuideRepository,
                IRepository<TrainingManual> trainingManualRepository,
                IRepository<TrainingTest> trainingTestRepository,
                IRepository<Test> testRepository,
                IRepository<TestResult> testResultRepository,
                IRepository<Test_Result> newTestResultRepository,
                IRepository<TestUserAnswer> testUserAnswerRepository,
                IRepository<TestAssigned> testAssignedRepository,
                IRepository<AssignedDocument> assignedDocumentRepository,
                IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
                IRepository<Upload> uploadRepository,
                IRepository<StandardUser> userRepository,
                IRepository<CustomConfiguration> customConfigurationRepository,
                IRepository<Certificate> certificateRepository,
                IRepository<StandardUserCorrespondanceLog> standardUserCorrespondenceLogRepository,
                IRepository<DocumentUsage> documentUsageRepository,
                IRepository<TrainingTestUsageStats> trainingTestUsageStatsRepository,
                IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
            {
                _context = context;
                _categoriesRepository = categoriesRepository;
                _documentCategoryRepository = documentCategoryRepository;
                _trainingLabelRepository = trainingLabelRepository;
                _labelRepository = labelRepository;
                _trainingGuideRepository = trainingGuideRepository;
                _trainingManualRepository = trainingManualRepository;
                _trainingTestRepository = trainingTestRepository;
                _testRepository = testRepository;
                _testResultRepository = testResultRepository;
                _newTestResultRepository = newTestResultRepository;
                _testUserAnswerRepository = testUserAnswerRepository;
                _testAssignedRepository = testAssignedRepository;
                _assignedDocumentRepository = assignedDocumentRepository;
                _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
                _uploadRepository = uploadRepository;
                _userRepository = userRepository;
                _customConfigurationRepository = customConfigurationRepository;
                _certificateRepository = certificateRepository;
                _standardUserCorrespondenceLogRepository = standardUserCorrespondenceLogRepository;
                _documentUsageRepository = documentUsageRepository;
                _trainingTestUsageStatsRepository = trainingTestUsageStatsRepository;
                _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
            }

            private TrainingManual MigrateTrainingGuide(TrainingGuide tg, IDictionary<string, string> categoryMap, IDictionary<string, string> labelMap)
            {
                //Migrate all uploads linked to the trainingGuide
                var uploadMap = MigrateUploads(tg);
                var chapter_ckeUploadMap = MigrateCKEUploads(tg);

                using (var transactionScope = new TransactionScope())
                {
                    var tm = new TrainingManual
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = tg.Title,
                        ReferenceId = tg.ReferenceId,
                        CreatedBy = tg.CreatedBy.ToString(),
                        CreatedOn = tg.CreatedOn,
                        Deleted = false,
                        Description = tg.Description,
                        DocumentStatus = tg.IsActive ? DocumentStatus.Published : DocumentStatus.Draft,
                        LastEditDate = tg.LastEditDate,
                        LastEditedBy = tg.CreatedBy.ToString(), // assumption
                        Points = 0, //assumption
                        PreviewMode = tg.PlaybookPreviewMode == PlaybookPreviewMode.Landscape ? DocumentPreviewMode.Landscape : DocumentPreviewMode.Portrait,
                        Printable = tg.Printable,
                        Collaborators = tg.Collaborators,
                        Category = GetCategory(tg.Categories.FirstOrDefault(), categoryMap),
                        CoverPicture = GetUpload(tg.CoverPicture, uploadMap),
                        // TrainingLabels = GetLabels(tg.TrainingLabels, labelMap),
                    };

                    // Migrate TrainingGuideChapter to TrainingManualChapter
                    foreach (var tgc in tg.ChapterList)
                    {
                        var tmc = new TrainingManualChapter
                        {
                            Id = Guid.NewGuid().ToString(),
                            Deleted = false,
                            Number = tgc.ChapterNumber,
                            Title = tgc.ChapterName,
                            TrainingManual = tm,
                            Uploads = tgc.ChapterUploads.Where(x => x.Upload != null).Select(x => GetUpload(x, uploadMap)).ToList()
                        };
                        if (chapter_ckeUploadMap.ContainsKey(tgc.Id.ToString()))
                        {
                            var c_cke = chapter_ckeUploadMap[tgc.Id.ToString()];
                            tmc.Content = c_cke.HTML;
                            c_cke.Uploads.ToList().ForEach(x => tmc.ContentToolsUploads.Add(GetUpload(x, c_cke.UploadMap)));
                        }
                        tm.Chapters.Add(tmc);
                    }

                    _trainingManualRepository.Add(tm);
                    transactionScope.Complete();
                    return tm;
                }
            }
            private Test MigrateTrainingTest(TrainingTest tt, TrainingManual tm, DocumentStatus status)
            {
                var uploadMap = MigrateUploads(tt);
                var customConfiguration = _customConfigurationRepository.List.AsQueryable().FirstOrDefault(x => !x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value));
                var certificateMap = MigrateCustomCertificate(customConfiguration);
                var t = new Test
                {
                    Id = Guid.NewGuid().ToString(),
                    AssignMarksToQuestions = tt.AssignMarksToQuestions,
                    CreatedBy = tt.CreatedBy.ToString(),
                    Title = tt.TestTitle,
                    CreatedOn = tt.CreateDate,
                    Deleted = tt.Deleted.HasValue ? tt.Deleted.Value : false,
                    IntroductionContent = tt.IntroductionContent,
                    Description = string.Empty,
                    DocumentStatus = status,
                    Duration = tt.TestDuration,
                    EmailSummary = tt.EmailSummaryOnCompletion,
                    ExpiryNotificationSentOn = tt.ExpiryNotificationSentOn,
                    HighlightAnswersOnSummary = tt.HighlightAnswersOnSummary,
                    LastEditDate = tt.DraftEditDate,
                    LastEditedBy = tt.CreatedBy.ToString(),
                    MaximumAttempts = tt.MaximumNumberOfRewites.HasValue ? Math.Min(3, tt.MaximumNumberOfRewites.Value) : 1,
                    NotificationInteval = TestExpiryNotificationInterval.Custom,
                    NotificationIntevalDaysBeforeExpiry = 7,
                    OpenTest = false,
                    PassMarks = tt.PassMarks,
                    Points = tt.PassPoints,
                    PreviewMode = DocumentPreviewMode.Landscape, //not in editor,
                    Printable = false,
                    RandomizeQuestions = !tt.DisableQuestionRandomization,
                    ReferenceId = tt.ReferenceId,
                    TestReview = tt.TestReview,
                    Category = tm.Category,
                    Collaborators = tm.Collaborators, //so it shows up on the creators library
                    TrainingLabels = tm.TrainingLabels,
                    Certificate = certificateMap.Any() ? GetCertificate(customConfiguration, certificateMap) : null
                };

                // Migrate TrainingQuestion to TestQuestion
                foreach (var tq in tt.QuestionList)
                {
                    var testQuestion = new TestQuestion
                    {
                        Id = Guid.NewGuid().ToString(),
                        Test = t,
                        AnswerWeightage = tq.AnswerWeightage,
                        Deleted = false,
                        Number = tq.TestQuestionNumber,
                        Question = tq.TestQuestion,
                        Uploads = GetUploads(tq, uploadMap)
                    };
                    // Migrate TestAnswer to TestQuestionAnswer
                    foreach (var ta in tq.TestAnswerList)
                    {
                        var tqa = new TestQuestionAnswer
                        {
                            Id = Guid.NewGuid().ToString(),
                            TestQuestionId = testQuestion.Id,
                            Deleted = false,
                            Number = ta.Position.HasValue ? ta.Position.Value : tq.TestAnswerList.IndexOf(ta),
                            Option = ta.Option
                        };
                        if (ta.Id.ToString() == tq.CorrectAnswer)
                            testQuestion.CorrectAnswerId = tqa.Id;

                        testQuestion.Answers.Add(tqa);
                    }

                    t.Questions.Add(testQuestion);
                }
                _testRepository.Add(t);
                return t;
            }
            private void MigrateTestAssignment(TrainingTest tt, Test t)
            {
                // Migrate TestAssigned to AssignedDocument
                _assignedDocumentRepository.AddRange(_testAssignedRepository.List.AsQueryable().Where(ta => ta.TestId == tt.Id && ta.UserId.HasValue).ToList().Select(ta => new AssignedDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = t.Id,
                    DocumentType = DocumentType.Test,
                    UserId = ta.UserId.ToString(),
                    AssignedBy = ta.AssignedBy.ToString(),
                    AssignedDate = ta.AssignedDate ?? GetClosestAssignmentCorrespondence(ta)
                }).ToArray());
            }
            private void MigrateTestResult(TrainingTest tt, Test t, IList<StandardUser> users, IList<TestUserAnswer> testUserAnswers)
            {
                // Migrate TestResult to Test_Result
                var testResults = new ConcurrentBag<TestResult>(_testResultRepository.List.AsQueryable().Where(tr => tr.TrainingTestId == tt.Id).ToList());
                var testUsageStats = _trainingTestUsageStatsRepository.List.AsQueryable().Where(us => us.TrainingTestId == tt.Id).ToList();
                var result = new ConcurrentBag<Test_Result>();
                var loopResult = Parallel.ForEach(testResults, tr =>
                {
                    var testResult = new Test_Result
                    {
                        Id = Guid.NewGuid().ToString(),
                        Test = t,
                        UserId = tr.TestTakenByUserId.ToString(),
                        Created = tr.TestDate,
                        Score = tr.TestScore,
                        Total = tr.Total
                    };
                    var startTime = testUsageStats.FirstOrDefault(us => us.UserId == tr.TestTakenByUserId && us.DateTaken.HasValue &&
                                                                       tr.TestDate > us.DateTaken.Value && (tr.TestDate - us.DateTaken.Value) <= TimeSpan.FromMinutes(t.Duration))?.DateTaken;
                    testResult.TimeTaken = startTime != null ? (tr.TestDate - startTime.Value) : TimeSpan.FromMinutes(t.Duration); // assumption
                    testResult.Percentage =
                        Math.Round(
                            decimal.Parse(testResult.Score.ToString()) / decimal.Parse(testResult.Total.ToString()) *
                            100, 2);
                    testResult.Passed = testResult.Percentage >= t.PassMarks;

                    foreach (var tua in testUserAnswers.Where(tua => tua.Result.Id == tr.Id).ToArray())
                    {
                        var question =
                            t.Questions.First(q => q.Question == tua.Answer.TrainingQuestion.TestQuestion);
                        var answers = question.Answers.Select(a => new TestQuestionAnswer_Result
                        {
                            Id = Guid.NewGuid().ToString(),
                            Answer = a,
                            AnswerId = a.Id,
                            Deleted = a.Deleted, // ignore?
                            Selected = tua.Answer.Option == a.Option
                        }).ToList();

                        var tqr = new TestQuestion_Result
                        {
                            Id = Guid.NewGuid().ToString(),
                            Question = question,
                            QuestionId = question.Id,
                            Correct = tua.Answer.Correct,
                            Answers = answers
                        };
                        testResult.Questions.Add(tqr);
                    }

                    var answeredQuestions = testResult.Questions.Select(q => q.Question.Id);
                    var unansweredQuestions = t.Questions.Where(q => !q.Deleted && !answeredQuestions.Contains(q.Id)).ToList();
                    foreach (var uq in unansweredQuestions)
                    {
                        testResult.Questions.Add(new TestQuestion_Result
                        {
                            Id = Guid.NewGuid().ToString(),
                            Question = uq,
                            QuestionId = uq.Id,
                            UnAnswered = true,
                            Answers = uq.Answers.Select(a => new TestQuestionAnswer_Result
                            {
                                Id = Guid.NewGuid().ToString(),
                                Answer = a,
                                AnswerId = a.Id,
                                Deleted = a.Deleted // ignore?
                            }).ToList()
                        });
                    }

                    if (testResult.Passed && t.Certificate != null && t.Certificate.Upload != null &&
                        t.Certificate.Upload.Data != null)
                    {
                        var user = users.FirstOrDefault(u => u.Id == tr.TestTakenByUserId);
                        testResult.Certificate = new Upload
                        {
                            Data = CreateCertificate(user, testResult, t.Certificate.Upload.Data),
                            Id = Guid.NewGuid().ToString(),
                            Name = $"{user.Id}_{t.Id}_{testResult.Id}.pdf",
                            Type = FileUploadType.Certificate.ToString(),
                            ContentType = "pdf"
                        };
                    }
                    result.Add(testResult);

                });
                while (!loopResult.IsCompleted) { continue; }
                _newTestResultRepository.AddRange(result.ToArray());
            }
            private void MigrateTrainingGuideAssignment(TrainingGuide tg, TrainingManual tm)
            {
                // Migrate AssignedTrainingGuides to AssignedDocument

                _assignedDocumentRepository.AddRange(_assignedTrainingGuidesRepository.List.AsQueryable().Where(atg => atg.TrainingGuideId == tg.Id && atg.UserId.HasValue).ToList().Select(atg => new AssignedDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = tm.Id,
                    DocumentType = DocumentType.TrainingManual,
                    UserId = atg.UserId.ToString(),
                    AssignedBy = atg.AssignedBy.ToString(),
                    AssignedDate = atg.AssignedDate ?? GetClosestAssignmentCorrespondence(atg)
                }).ToArray());
            }


            public CommandResponse Execute(Sprint16MigrationCommand command)
            {
                var validation = new List<IValidationResult>();

                // Migrate Categories to DocumentCategory
                var categoryMap = MigrateCategories();
                // Migrate TrainingLabel to Label
                var labelMap = MigrateLabels();

                // Migrate TrainingGuide (Playbook) to TrainingManual
                var count = 0;
                var existingReferences = _trainingManualRepository.List.AsQueryable().Select(trainingManual =>
                                trainingManual.ReferenceId).ToList();
                foreach (var tg in _trainingGuideRepository.List.AsQueryable().Where(tg => !existingReferences.Contains(tg.ReferenceId)).ToArray())
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
                    {
                        try
                        {
                            count++;
                            var tm = MigrateTrainingGuide(tg, categoryMap, labelMap);
                            MigrateTrainingGuideAssignment(tg, tm);
                            MigrateTrainingGuideUsage(tg, tm);

                            var originalTest = tg?.TestVersion?.LastPublishedVersion;
                            if (originalTest == null)
                            {
                                _context.SaveChanges();
                                scope.Complete();
                                continue;
                            }

                            var t = MigrateTrainingTest(originalTest, tm, DocumentStatus.Published);
                            MigrateTestResult(originalTest, t, _userRepository.List.AsQueryable().ToList(), _testUserAnswerRepository.List.AsQueryable().Include(x => x.Result).Include(x => x.Answer).Include(x => x.Answer.TrainingQuestion).Where(tua => tua.Result != null).ToList());
                            MigrateTestAssignment(originalTest, t);
                            MigrateTestUsage(originalTest, t);
                            //calls save on the entire customer context
                            _context.SaveChanges();
                            scope.Complete();
                        }
                        catch (Exception e)
                        {
                            validation.Add(new ValidationResult
                            {
                                MemberName = $"Playbook Ref: {tg.ReferenceId}",
                                Message = $"Exception: {string.Join(",", e.Message, e.InnerException?.Message)}"
                            });
                            count--;
                        }
                    }
                }

                command.Message = $"Migrated {count} of {_trainingGuideRepository.List.Count()} Playbooks";
                command.Validation = validation;

                return null;
            }

            private void MigrateTrainingGuideUsage(TrainingGuide tg, TrainingManual tm)
            {
                _documentUsageRepository.AddRange(_trainingGuideUsageStatsRepository.List.AsQueryable().Where(us => us.TrainingGuidId == tg.Id && !us.Unassigned).ToList().Select(us => new DocumentUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = tm.Id,
                    DocumentType = DocumentType.TrainingManual,
                    UserId = us.UserId.ToString(),
                    ViewDate = us.ViewDate,
                    Duration = TimeSpan.FromSeconds(0)
                }).ToArray());
            }

            private void MigrateTestUsage(TrainingTest originalTest, Test test)
            {
                _documentUsageRepository.AddRange(_trainingTestUsageStatsRepository.List.AsQueryable().Where(us => us.TrainingTestId == originalTest.Id && !us.Unassigned && us.DateTaken.HasValue).ToList().Select(us => new DocumentUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = test.Id,
                    DocumentType = DocumentType.Test,
                    UserId = us.UserId.ToString(),
                    ViewDate = us.DateTaken.Value,
                    Duration = TimeSpan.FromSeconds(0)
                }).ToArray());
            }

            #region Helper Methods
            private IDictionary<string, string> MigrateCategories()
            {
                var r = new Dictionary<string, string>();
                using (var scope = new TransactionScope())
                {
                    var rootCategories = _categoriesRepository.List.Where(c => c.ParentCategoryId == null);
                    foreach (var c in rootCategories)
                    {
                        MigrateCategoryAndChildren(c, r, null);
                    }
                    scope.Complete();
                    _documentCategoryRepository.SaveChanges();
                }
                return r;
            }
            private void MigrateCategoryAndChildren(Categories c, IDictionary<string, string> map, string parentId = null)
            {
                var dc = _documentCategoryRepository.List.FirstOrDefault(documentCategory =>
                    documentCategory.Title == c.CategoryTitle);
                if (dc == null)
                {
                    dc = new DocumentCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = c.CategoryTitle,
                        ParentId = parentId
                    };
                    _documentCategoryRepository.Add(dc);
                }

                map.Add(c.Id.ToString(), dc.Id);

                var children = _categoriesRepository.List.Where(cat => cat.ParentCategoryId == c.Id);
                foreach (var child in children)
                {
                    MigrateCategoryAndChildren(child, map, dc.Id);
                }
            }
            private DocumentCategory GetCategory(Categories original, IDictionary<string, string> map)
            {
                if (original != null)
                {
                    if (map.ContainsKey(original.Id.ToString()))
                        return _documentCategoryRepository.List.FirstOrDefault(x => x.Id == map[original.Id.ToString()]);
                }
                return _documentCategoryRepository.List.FirstOrDefault(c => c.Title == "Default");
            }
            private IDictionary<string, string> MigrateLabels()
            {
                var result = new Dictionary<string, string>();
                using (var scope = new TransactionScope())
                {
                    foreach (var tl in _trainingLabelRepository.List.ToList())
                    {
                        var label = _labelRepository.List.FirstOrDefault(l => l.Name == tl.Name);
                        if (label == null)
                        {
                            label = new Label
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = tl.Name,
                                Description = tl.Description
                            };
                            _labelRepository.Add(label);
                        }

                        result.Add(tl.Id.ToString(), label.Id);
                    }
                    scope.Complete();
                    _labelRepository.SaveChanges();
                }
                return result;
            }
            private IList<Label> GetLabels(IEnumerable<TrainingLabel> originals, IDictionary<string, string> map)
            {
                var newIds = originals.Select(x => map[x.Id.ToString()]).ToList();
                return _labelRepository.List.Where(x => newIds.Contains(x.Id)).ToList();
            }
            private IDictionary<string, string> MigrateUploads(TrainingGuide tg)
            {
                var r = new Dictionary<string, string>();
                var coverPicture = new List<FileUploads>();
                var chapterUploads = new List<ChapterUpload>();

                if (tg.CoverPicture != null)
                    coverPicture.Add(tg.CoverPicture);
                tg.ChapterList.SelectMany(x => x.ChapterUploads).ToList().ForEach(x => chapterUploads.Add(x));

                coverPicture.ForEach(x =>
                {
                    var u = x.Clone_FileUpload_Upload_CoverPicture();
                    r.Add(x.Id.ToString(), u.Id);
                    _uploadRepository.Add(u);
                });
                chapterUploads.ForEach(x =>
                {
                    if (x.Upload != null)
                    {
                        var u = x.Clone_ChapterUpload_Upload();
                        r.Add(x.Upload.Id.ToString(), u.Id);
                        _uploadRepository.Add(u);
                    }
                });

                return r;
            }
            private IDictionary<string, string> MigrateUploads(TrainingTest t)
            {
                var r = new Dictionary<string, string>();
                var uploads = new List<FileUploads>();
                t.QuestionList.ForEach(q =>
                {
                    if (q.Audio != null && q.Audio.Upload != null)
                        uploads.Add(q.Audio.Upload);
                    if (q.Image != null && q.Image.Upload != null)
                        uploads.Add(q.Image.Upload);
                    if (q.Video != null && q.Video.Upload != null)
                        uploads.Add(q.Video.Upload);
                });

                uploads.ForEach(x =>
                {
                    var u = x.Clone_FileUpload_Upload_CoverPicture();
                    r.Add(x.Id.ToString(), u.Id);
                    _uploadRepository.Add(u);
                });

                return r;
            }
            private IDictionary<string, string> MigrateCustomCertificate(CustomConfiguration c)
            {
                var r = new Dictionary<string, string>();
                if (c != null && c.Certificate != null)
                {
                    var u = c.Certificate.Clone_FileUpload_Upload_CoverPicture();
                    _uploadRepository.Add(u);
                    var certificate = new Certificate { Id = Guid.NewGuid().ToString(), Upload = u };
                    r.Add(c.Certificate.Id.ToString(), certificate.Id);
                    _certificateRepository.Add(certificate);
                }
                return r;
            }
            private IDictionary<string, ContentToolsBinder> MigrateCKEUploads(TrainingGuide tg)
            {
                var r = new Dictionary<string, ContentToolsBinder>();
                var contentToolsUploads = new List<ContentToolsBinder>();
                contentToolsUploads.AddRange(tg.ChapterList.Select(x => new ContentToolsBinder { ChapterId = x.Id.ToString(), HTML = x.ChapterContent, Uploads = x.CKEUploads }).ToArray());
                contentToolsUploads.ForEach(x =>
                {
                    if (x.Uploads.Any())
                        x.Uploads.Where(y => y.Upload != null).ToList().ForEach(y =>
                        {
                            var u = y.Clone_CKEUpload_Upload();
                            if (!string.IsNullOrEmpty(x.HTML))
                                x.HTML = x.HTML.Replace($"/Upload/Get/{y.Upload.Id}", $"/Upload/Get/{u.Id}").Replace($"/Upload/GetThumbnail/{y.Upload.Id}", $"/Upload/GetThumbnail/{u.Id}");
                            x.UploadMap.Add(y.Upload.Id.ToString(), u.Id);
                            _uploadRepository.Add(u);
                        });
                    r.Add(x.ChapterId, x);
                });
                return r;
            }

            private Upload GetUpload(FileUploads original, IDictionary<string, string> map)
            {
                if (original != null && map.ContainsKey(original.Id.ToString()))
                    return _uploadRepository.List.FirstOrDefault(x => x.Id == map[original.Id.ToString()]);
                return null;
            }
            private Upload GetUpload(ChapterUpload original, IDictionary<string, string> map)
            {
                if (original != null && original.Upload != null && map.ContainsKey(original.Upload.Id.ToString()))
                {
                    return _uploadRepository.List.FirstOrDefault(x => x.Id == map[original.Upload.Id.ToString()]);
                }

                return null;
            }
            private Upload GetUpload(CKEUpload original, IDictionary<string, string> map)
            {
                if (original != null && original.Upload != null && map.ContainsKey(original.Upload.Id.ToString()))
                    return _uploadRepository.List.FirstOrDefault(x => x.Id == map[original.Upload.Id.ToString()]);
                return null;
            }
            private IList<Upload> GetUploads(TrainingQuestion tq, IDictionary<string, string> map)
            {
                var result = new List<Upload>();
                if (tq.Audio != null && tq.Audio.Upload != null && map.ContainsKey(tq.Audio.Id.ToString()))
                    result.Add(_uploadRepository.List.FirstOrDefault(x => x.Id == map[tq.Audio.Id.ToString()]));
                if (tq.Image != null && tq.Image.Upload != null && map.ContainsKey(tq.Image.Id.ToString()))
                    result.Add(_uploadRepository.List.FirstOrDefault(x => x.Id == map[tq.Image.Id.ToString()]));
                if (tq.Video != null && tq.Video.Upload != null && map.ContainsKey(tq.Video.Id.ToString()))
                    result.Add(_uploadRepository.List.FirstOrDefault(x => x.Id == map[tq.Video.Id.ToString()]));
                return result;
            }
            private Certificate GetCertificate(CustomConfiguration configuration, IDictionary<string, string> map)
            {
                if (configuration != null && configuration.Certificate != null && map.ContainsKey(configuration.Certificate.Id.ToString()))
                    return _certificateRepository.List.FirstOrDefault(x => x.Id == map[configuration.Certificate.Id.ToString()]);
                return null;
            }

            private byte[] CreateCertificate(StandardUser user, Test_Result r, byte[] cert)
            {
                byte[] result;
                try
                {
                    //make it a bmp first
                    System.Drawing.Image image = null;
                    using (MemoryStream stream = new MemoryStream(cert))
                    {
                        var temp = Bitmap.FromStream(stream, true, true);
                        using (var tempStream = new MemoryStream())
                        {
                            temp.Save(tempStream, ImageFormat.Bmp);
                            image = Bitmap.FromStream(tempStream, true, true);
                        }
                    }
                    using (var graphics = Graphics.FromImage(image))
                    {

                        System.Drawing.Font font = new System.Drawing.Font("Times New Roman", 20.0f);
                        System.Drawing.Font fontlogo = new System.Drawing.Font("Times New Roman", 40.0f);
                        // Create text position
                        var name = user.FirstName.RemoveSpecialCharacters().Contains(" ") ? user.FirstName.RemoveSpecialCharacters() : $"{user.FirstName} {user.LastName}".RemoveSpecialCharacters();
                        int intWidth = (int)graphics.MeasureString(name, font).Width;
                        PointF pointUserName = new PointF(1250 - (intWidth / 2), 1150);
                        // Draw text User Name
                        graphics.DrawString(name, font, Brushes.Black, pointUserName);

                        int intWidthPlaybook = (int)graphics.MeasureString(r.Test.Title.RemoveSpecialCharacters(), font).Width;
                        PointF pointPlaybookName = new PointF(1250 - (intWidthPlaybook / 2), 1420);
                        // Draw text
                        graphics.DrawString(r.Test.Title.RemoveSpecialCharacters(), font, Brushes.Black, pointPlaybookName);

                        string score = string.Format("{0} %", r.Percentage.ToString());
                        int intWidthMarks = (int)graphics.MeasureString(score, font).Width;
                        PointF pointMarks = new PointF(1250 - (intWidthMarks / 2), 1700);
                        // Draw text total marks scored
                        graphics.DrawString(score, font, Brushes.Black, pointMarks);

                        int intWidthDate = (int)graphics.MeasureString(r.Created.ToString("dd-MMMM-yyyy"), font).Width;
                        PointF pointDate = new PointF(1250 - (intWidthDate / 2), 1900);

                        graphics.DrawString(r.Created.ToString("dd-MMMM-yyyy"), font, Brushes.Black, pointDate);
                        var document = new Document(PageSize.A4, 0, 0, 0, 0);
                        var pdfS = new MemoryStream();

                        PdfWriter.GetInstance(document, pdfS);
                        document.Open();
                        var images = iTextSharp.text.Image.GetInstance(image, ImageFormat.Bmp);
                        images.ScaleToFit(iTextSharp.text.PageSize.A4);
                        document.Add(images);
                        document.Close();
                        result = pdfS.ToArray();
                        pdfS.Dispose();
                    }
                    return result;
                }
                catch (DocumentException de)
                {
                    //throw de;
                    var docmsg = de.Message;
                    throw de;
                }
                catch (IOException ex)
                {
                    //throw ex;
                    var msg = ex.Message;
                    throw ex;
                }
            }

            private DateTime GetClosestAssignmentCorrespondence(AssignedTrainingGuides atg)
            {
                var referenceId = atg.TrainingGuide.ReferenceId;
                var correspondence = _standardUserCorrespondenceLogRepository.List.AsQueryable()
                    .Where(c => c.UserId == atg.UserId && c.Description != null && c.Content != null && c.Description.Contains("Playbook Assignment") &&
                                c.Content.Contains(referenceId)).OrderByDescending(c => c.CorrespondenceDate)
                    .FirstOrDefault();
                return correspondence?.CorrespondenceDate ?? atg.TrainingGuide.CreatedOn; // assumption
            }
            private DateTime GetClosestAssignmentCorrespondence(TestAssigned ta)
            {
                var referenceId = ta.Test.ReferenceId;
                var correspondence = _standardUserCorrespondenceLogRepository.List.AsQueryable()
                    .Where(c => c.UserId == ta.UserId && c.Description != null && c.Content != null && c.Description.Contains("Test Assignment") &&
                                c.Content.Contains(referenceId)).ToList().AsQueryable()
                    .Select(c => new {
                        Ticks = Math.Abs((c.CorrespondenceDate -
                                          (ta.Test.ActivePublishDate ?? ta.Test.DraftEditDate ?? ta.Test.CreateDate)).Ticks),
                        Correspondence = c
                    })
                    .OrderBy(c => c.Ticks)
                    .FirstOrDefault()?.Correspondence;
                return correspondence?.CorrespondenceDate ?? ta.Test.CreateDate; // assumption
            }

            private class ContentToolsBinder
            {
                public IDictionary<string, string> UploadMap { get; set; } = new Dictionary<string, string>();
                public string ChapterId { get; set; }
                public string HTML { get; set; }
                public IEnumerable<CKEUpload> Uploads { get; set; } = new List<CKEUpload>();
            }
            #endregion
        }
    }
    public static class MigrationExtensions
    {
        public static Upload Clone_FileUpload_Upload_CoverPicture(this FileUploads x)
        {
            if (x == null)
                return null;
            return new Upload
            {
                ContentType = x.ContentType,
                Data = x.Data,
                Deleted = x.Deleted,
                Description = x.Description,
                Id = Guid.NewGuid().ToString(),
                Name = x.Name,
                Order = 0, //assumption
                Type = x.Type
            };
        }
        public static Upload Clone_ChapterUpload_Upload(this ChapterUpload x)
        {
            if (x == null || (x != null && x.Upload == null))
                return null;
            return new Upload
            {
                Id = Guid.NewGuid().ToString(),
                Data = x.Upload.Data,
                Content = x.Content,
                ContentType = x.Upload.ContentType,
                Deleted = x.Upload.Deleted,
                Description = x.Upload.Description,
                Name = x.Upload.Name,
                Order = x.ChapterUploadSequence,
                Type = x.Upload.Type
            };
        }
        public static Upload Clone_CKEUpload_Upload(this CKEUpload x)
        {
            if (x == null || (x != null && x.Upload == null))
                return null;
            return x.Upload.Clone_FileUpload_Upload_CoverPicture();
        }
    }
}