using System;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.ViewModel;
using System.Linq;
using System.Collections.Generic;
using Common;
using Common.Data;
using Domain.Customer;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.PolicyResponse;
using Domain.Customer.Models.Test;
using Domain.Enums;
using Ramp.Contracts.Query.UserFeedback;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using VirtuaCon;
using Data.EF.Customer;
using Domain.Models;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.TrainingManual;
using System.Linq.Expressions;
using Domain.Customer.Models.Policy;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Data.Entity;
using static Ramp.Contracts.ViewModel.UserActivityAndPerformanceViewModel;
using Domain.Customer.Models.CheckLists;
using System.Globalization;

using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler.Reporting
{
    public class UserActivityAndPerformanceReportQueryHandler :
        IQueryHandler<UserActivityAndPerformanceReportQuery, UserActivityAndPerformanceViewModel>
    {
        private readonly ITransientReadRepository<AssignedDocument> _assignedDocumentRepository;
        private readonly ITransientReadRepository<DocumentUsage> _documentUsageRepository;
        private readonly ITransientReadRepository<Test_Result> _testResultRepository;
        private readonly ITransientReadRepository<PolicyResponse> _policyResponseRepository;
        private readonly ITransientReadRepository<StandardUser> _userRepository;
        private readonly IReadRepository<RaceCodes> _raceCodeRepository;
        private readonly ITransientReadRepository<Memo> _memoRepository;
        private readonly ITransientReadRepository<TrainingManual> _trainingManualRepository;
        private readonly ITransientReadRepository<Test> _testRepository;
        private readonly ITransientReadRepository<Policy> _policyRepository;
        private readonly ITransientReadRepository<CheckList> _checkListRepository;
        private readonly ITransientReadRepository<CheckListChapter> _checkListChapterRepository;
        private readonly ITransientRepository<CheckListChapterUserResult> _checkListChapterUserResultRepository;
        private readonly ITransientRepository<CheckListUserResult> _checkListUserResultRepository;
        private readonly IQueryHandler<PointsStatementQuery, PointsStatementViewModel> _pointsStatementQueryHandler;
        private readonly IQueryHandler<TrainingActivityListQuery, TrainingActivityReportModel> _trainingActivityReportQueryHandler;
        private readonly IQueryHandler<FilteredUserFeedbackQuery, IEnumerable<UserFeedbackViewModelShort>> _feedbackQueryHandler;
        private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;

        public UserActivityAndPerformanceReportQueryHandler(
            IRepository<StandardUserGroup> standardUserGroupRepository,
            IRepository<CustomerGroup> groupRepository,
            ITransientReadRepository<AssignedDocument> assignedDocumentRepository,
            ITransientReadRepository<DocumentUsage> documentUsageRepository,
            ITransientReadRepository<Test_Result> testResultRepository,
            ITransientReadRepository<PolicyResponse> policyResponseRepository,
            ITransientReadRepository<StandardUser> userRepository,
            IReadRepository<RaceCodes> raceCodeRepository,
            ITransientReadRepository<Memo> memoRepository,
            ITransientReadRepository<TrainingManual> trainingManualRepository,
            ITransientReadRepository<Test> testRepository,
            ITransientReadRepository<Policy> policyRepository,
            ITransientReadRepository<CheckList> checkListRepository,
            ITransientReadRepository<CheckListChapter> checkListChapterRepository,
            IQueryHandler<PointsStatementQuery, PointsStatementViewModel> pointsStatementQueryHandler,
            ITransientRepository<CheckListChapterUserResult> checkListChapterUserResultRepository,
            ITransientRepository<CheckListUserResult> checkListUserResultRepository,
            IQueryHandler<TrainingActivityListQuery, TrainingActivityReportModel> trainingActivityReportQueryHandler,
            IQueryHandler<FilteredUserFeedbackQuery, IEnumerable<UserFeedbackViewModelShort>> feedbackQueryHandler)
        {
            _standardUserGroupRepository = standardUserGroupRepository;
            _groupRepository = groupRepository;
            _assignedDocumentRepository = assignedDocumentRepository;
            _documentUsageRepository = documentUsageRepository;
            _testResultRepository = testResultRepository;
            _policyResponseRepository = policyResponseRepository;
            _userRepository = userRepository;
            _raceCodeRepository = raceCodeRepository;
            _memoRepository = memoRepository;
            _trainingManualRepository = trainingManualRepository;
            _testRepository = testRepository;
            _policyRepository = policyRepository;
            _pointsStatementQueryHandler = pointsStatementQueryHandler;
            _trainingActivityReportQueryHandler = trainingActivityReportQueryHandler;
            _feedbackQueryHandler = feedbackQueryHandler;
            _checkListRepository = checkListRepository;
            _checkListChapterRepository = checkListChapterRepository;
            _checkListChapterUserResultRepository = checkListChapterUserResultRepository;
            _checkListUserResultRepository = checkListUserResultRepository;
        }

        private IList<Document_DocumentUsageModel<T>> GetDocumentModelsFromAssignment<TEntity, T>(string userId, IEnumerable<AssignedDocument> assignedDocuments,
                                                                                                  IReadRepository<TEntity> repository,
                                                                                                  Expression<Func<TEntity, T>> projection)
            where TEntity : IdentityModel<string>
            where T : IdentityModel<string>
        {
            if (assignedDocuments.Any() && !string.IsNullOrWhiteSpace(userId))
            {
                var ids = assignedDocuments.Select(ad => ad.DocumentId).ToList();
                var docType = assignedDocuments.First().DocumentType;
                var usages = _documentUsageRepository.Get(x => x.UserId == userId && x.DocumentType == docType && ids.Contains(x.DocumentId)).OrderByDescending(c => c.ViewDate).ToList();
                return repository.List.AsQueryable().Where(x => ids.Contains(x.Id)).Select(projection).ToList().Select(doc => new Document_DocumentUsageModel<T>
                {
                    Document = doc,
                    Usages = usages.Where(x => x.DocumentId == doc.Id).ToList()
                }).ToList();
            }
            return new List<Document_DocumentUsageModel<T>>();
        }
        public UserActivityAndPerformanceViewModel ExecuteQuery(UserActivityAndPerformanceReportQuery query)
        {
            query.FromDate = query.FromDate.AtBeginningOfDay();
            query.ToDate = query.ToDate.AtEndOfDay();

            var vm = new UserActivityAndPerformanceViewModel();
            vm.EnableRaceCode = query.PortalContext.UserCompany.EnableRaceCode;
            vm.EnableTrainingActivities = query.PortalContext.UserCompany.EnableTrainingActivityLoggingModule;
            vm.EnableGlobalAccessDocuments = query.PortalContext.UserCompany.EnableGlobalAccessDocuments;
            var user = _userRepository.Get(x => x.Id.ToString() == query.UserId).Select(u => new UserActivityAndPerformanceViewModel.UserViewModel
            {
                Id = u.Id.ToString(),
                Name = u.FirstName + " " + u.LastName,
                ContactNumber = u.MobileNumber,
                Email = u.EmailAddress,
                IDNumber = u.IDNumber,
                Gender = u.Gender.HasValue ? u.Gender.Value.ToString() : null,
                EmployeeNumber = u.EmployeeNo,
                Race = vm.EnableRaceCode && u.RaceCodeId.HasValue ? u.RaceCodeId.Value.ToString() : null,
                Group = u.Group != null ? u.Group.Title : ""
            }).ToList().FirstOrDefault();
            if (user != null)
            {

                #region
                //below code added by neeraj

                var groups = _groupRepository.List;

                //	var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == user.Id).ToList();
                var groupList = _standardUserGroupRepository.Get(c => c.UserId.ToString() == user.Id.ToString()).Join(groups, sug => sug.GroupId, g => g.Id, (sug, g) => new { SUG = sug, G = g })
                         .Select(gl => new { grplist = gl.SUG, grps = gl.G }).ToList();

                string name = null;
                //List<string> name = new List<string>();
                if (groupList.Count > 0)
                {
                    foreach (var gl in groupList)
                    {
                        //foreach (var gl in groups)
                        //{
                        //	if (gl.Id == g.GroupId)
                        //	{
                        if (name != null)
                            name = name + "," + gl.grps.Title;
                        else name = name + gl.grps.Title;
                        //	}
                        //}
                    }
                }
                if (name != null)
                {
                    //vm.Groups = name;
                    user.Group = name;
                }


                #endregion

                if (!string.IsNullOrWhiteSpace(user.Gender))
                    if (Enum.TryParse<GenderEnum.Gender>(user.Gender, out var gender))
                        user.Gender = GenderEnum.GetDescription(gender);
                if (!string.IsNullOrEmpty(user.Race))
                    user.Race = _raceCodeRepository.Get(x => x.Id.ToString() == user.Race).FirstOrDefault()?.Description;
                vm.UserModel = user;
                var assignedDocuments = _assignedDocumentRepository.Get(x => x.UserId == query.UserId && x.Deleted == false);
                if (query.FromDate.HasValue && query.ToDate.HasValue)
                {
                    if (query.FromDate.HasValue)
                        assignedDocuments = assignedDocuments.Where(x => x.AssignedDate >= query.FromDate);
                    if (query.ToDate.HasValue)
                        assignedDocuments = assignedDocuments.Where(x => x.AssignedDate <= query.ToDate && x.Deleted == false);

                    assignedDocuments.GroupBy(x => x.DocumentType).ToList().ForEach(document_documentTypeGrouping =>
                    {
                        switch (document_documentTypeGrouping.Key)
                        {
                            case DocumentType.Memo:
                                var memos_usages = GetDocumentModelsFromAssignment(user.Id, document_documentTypeGrouping, _memoRepository, x => new MemoListModel { Id = x.Id, Title = x.Title }).Where(c => !c.Document.IsGlobalAccessed);
                                var memoModels = new ConcurrentBag<InteractionModel>();
                                var memoLoopResult = Parallel.ForEach(document_documentTypeGrouping.ToList(), assignedDocument =>
                                {
                                    //var document_usage = memos_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any() && !x.Usages.FirstOrDefault().IsGlobalAccessed);
                                    var document_usage = memos_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId);
                                    if (document_usage != null)
                                    {
                                        memoModels.Add(new InteractionModel
                                        {
                                            DocumentTitle = document_usage.Document.Title,
                                            TrainingLabels = document_usage.Document.TrainingLabels,
                                            DateAssigned = assignedDocument.AssignedDate.ToLocalTime(),
                                            IsGlobalAccess = document_usage.Document.IsGlobalAccessed,
                                            DateViewed = !document_usage.Usages.Any() ? new DateTime?() : document_usage.Usages.FirstOrDefault().ViewDate.ToLocalTime(),
                                            Viewed = document_usage.Usages.Any(),
                                            TimeTaken = !document_usage.Usages.Any() ? null : new TimeSpan(document_usage.Usages.Sum(x => x.Duration.Ticks)).ToString(@"%h\h\ %m\m\ %s\s")
                                        });
                                    }
                                    else
                                    {
                                        var memoDetails = memos_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any());
                                        if (memoDetails != null && memoDetails.Usages.Any())
                                        {
                                            var tm = memoDetails.Usages.FirstOrDefault(c => !c.IsGlobalAccessed);
                                            if (tm != null)
                                            {
                                                memoModels.Add(new InteractionModel
                                                {
                                                    DocumentTitle = memoDetails.Document.Title,
                                                    TrainingLabels = memoDetails.Document != null ? memoDetails.Document.TrainingLabels : "",
                                                    DateViewed = tm.ViewDate,
                                                    DateAssigned = assignedDocument.AssignedDate,
                                                    Viewed = true,
                                                    IsGlobalAccess = tm.IsGlobalAccessed,
                                                    TimeTaken = new TimeSpan(tm.Duration.Ticks).ToString(@"%h\h\ %m\m\ %s\s")
                                                });
                                            }

                                        }
                                    }
                                });
                                while (!memoLoopResult.IsCompleted) { continue; }
                                if (!string.IsNullOrEmpty(query.Tags))
                                {
                                    vm.MemoInteractions = memoModels.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateAssigned).ThenBy(x => x.DateViewed).ToList();

                                }
                                else
                                {
                                    vm.MemoInteractions = memoModels.OrderBy(x => x.DateAssigned).ThenBy(x => x.DateViewed).ToList();

                                }
                                break;
                            case DocumentType.TrainingManual:
                                var trainingManuals_usages = GetDocumentModelsFromAssignment(user.Id, document_documentTypeGrouping, _trainingManualRepository, x => new TrainingManualModel { Id = x.Id, Title = x.Title }).Where(c => !c.Document.IsGlobalAccessed);
                                var trainingManualModels = new ConcurrentBag<InteractionModel>();
                                var trainingManualLoopResult = Parallel.ForEach(document_documentTypeGrouping, assignedDocument =>
                                {
                                    //var document_usage = trainingManuals_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any() && !x.Usages.FirstOrDefault().IsGlobalAccessed);
                                    var document_usage = trainingManuals_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId);

                                    if (document_usage != null)
                                    {
                                        trainingManualModels.Add(new InteractionModel
                                        {
                                            DocumentTitle = document_usage.Document.Title,
                                            TrainingLabels = document_usage.Document.TrainingLabels,
                                            DateAssigned = assignedDocument.AssignedDate,
                                            DateViewed = !document_usage.Usages.Any() ? new DateTime?() : document_usage.Usages.FirstOrDefault().ViewDate,
                                            Viewed = document_usage.Usages.Any(),
                                            IsGlobalAccess = document_usage.Document.IsGlobalAccessed,
                                            TimeTaken = !document_usage.Usages.Any() ? null : new TimeSpan(document_usage.Usages.Sum(x => x.Duration.Ticks)).ToString(@"%h\h\ %m\m\ %s\s")
                                        });
                                    }
                                    else
                                    {
                                        var trainingDetails = trainingManuals_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any());
                                        if (trainingDetails != null && trainingDetails.Usages.Any())
                                        {
                                            var tm = trainingDetails.Usages.FirstOrDefault(c => !c.IsGlobalAccessed);

                                            if (tm != null)
                                            {
                                                trainingManualModels.Add(new InteractionModel
                                                {
                                                    DocumentTitle = trainingDetails.Document.Title,
                                                    TrainingLabels = trainingDetails.Document.TrainingLabels,
                                                    DateViewed = tm.ViewDate,
                                                    Viewed = true,
                                                    DateAssigned = assignedDocument.AssignedDate,
                                                    IsGlobalAccess = tm.IsGlobalAccessed,
                                                    TimeTaken = new TimeSpan(tm.Duration.Ticks).ToString(@"%h\h\ %m\m\ %s\s")
                                                });
                                            }
                                        }

                                    }
                                });
                                while (!trainingManualLoopResult.IsCompleted) { continue; }
                                if (!string.IsNullOrEmpty(query.Tags))
                                {
                                    vm.TrainingManualInteractions = trainingManualModels.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateAssigned).ThenBy(x => x.DateViewed).ToList();
                                }
                                else
                                {
                                    vm.TrainingManualInteractions = trainingManualModels.OrderBy(x => x.DateAssigned).ThenBy(x => x.DateViewed).ToList();
                                }
                                break;
                            case DocumentType.Test:
                                var tests_usages = GetDocumentModelsFromAssignment(user.Id, document_documentTypeGrouping, _testRepository, x => new TestModel { Id = x.Id, Title = x.Title }).Where(c => !c.Document.IsGlobalAccessed);
                                var testIds = tests_usages.Where(c => !c.Document.IsGlobalAccessed).Select(x => x.Document.Id).ToList();
                                var testResults = _testResultRepository.List.AsQueryable().Include(r => r.Test).Include(r => r.Questions).Where(x => x.UserId == user.Id && testIds.Contains(x.TestId) && !x.IsGloballyAccessed).ToList();
                                var testdetails1 = _testRepository.List.AsQueryable().ToList();
                                var testModels = new ConcurrentBag<TestInteractionModel>();
                                var testLoopResult = Parallel.ForEach(document_documentTypeGrouping.ToList(), assignedDocument =>
                                {
                                    var document_usage = tests_usages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any() && !x.Usages.FirstOrDefault().IsGlobalAccessed);
                                    var results = testResults.Where(x => x.TestId == assignedDocument.DocumentId).Select(x => new { x.Score, x.Questions, x.Percentage, x.Passed, x.Id, x.Created, x.TimeTaken, x.Total, Title = x.Test.Title, TrainingLabels = x.Test.TrainingLabels, TestId = x.TestId, IsGlobalAccess = x.IsGloballyAccessed }).OrderBy(x => x.Created).ToList();
                                    int count = 1;
                                    foreach (var item in results.ToList())
                                    {
                                        var testResult = new UserTestResult()
                                        {
                                            Attempt = "Attempt " + count,
                                            Viewed = true,
                                            IsGlobalAccess = item.IsGlobalAccess,
                                            DateViewed = item.Created.UtcDateTime,
                                            DateAssigned = assignedDocument.AssignedDate,
                                            Result = $"{item.Score}/{item.Total} ({item.Percentage.ToString("G")}%)",
                                            FinalResult = item.Passed ? "Passed" : "Failed",
                                            TestId = item.TestId,
                                            Title = item.Title
                                        };
                                        vm.TestResultList.Add(testResult);
                                        count++;
                                    }


                                    var test = testResults.Where(x => x.TestId == assignedDocument.DocumentId).FirstOrDefault();
                                    var trainingLabels = (test != null && test.Test != null) ? test.Test.TrainingLabels : "";
                                    if (test != null && test.Test != null)
                                    {
                                        vm.Title = test.Test.Title;
                                    }
                                    else if (document_usage != null)
                                    {
                                        vm.Title = document_usage.Document.Title;
                                    }
                                    var testdetails2 = testdetails1.Where(c => c.Id == assignedDocument.DocumentId).FirstOrDefault();

                                    if (!results.Any())
                                    {

                                        testModels.Add(new TestInteractionModel
                                        {
                                            DateAssigned = assignedDocument.AssignedDate,
                                            DocumentTitle = testdetails2.Title,
                                            TrainingLabels = testdetails2.TrainingLabels,
                                            Viewed = false,
                                            IsGlobalAccess = false,
                                        });
                                    }
                                    else
                                    {
                                        var interaction = new TestInteractionModel
                                        {
                                            DocumentTitle = vm.Title,
                                            TrainingLabels = trainingLabels,
                                            Viewed = true,
                                            DateAssigned = assignedDocument.AssignedDate,
                                            IsGlobalAccess = false,
                                            Result1 = $"{results.First().Score}/{results.First().Total} ({results.First().Percentage.ToString("G")}%)",
                                            ResultId1 = results.First().Id,
                                            DateViewed1 = results.First().Created.UtcDateTime,
                                            TimeTaken1 = results.First().TimeTaken.ToString(@"%m\m\ %s\s")
                                        };
                                        if (results.Count() > 1)
                                        {
                                            interaction.Result2 =
                                                $"{results.ElementAt(1).Score}/{results.ElementAt(1).Total} ({results.ElementAt(1).Percentage.ToString("G")}%)";
                                            interaction.ResultId2 = results.ElementAt(1).Id;
                                            interaction.DateViewed2 = results.ElementAt(1).Created.UtcDateTime;
                                            interaction.TimeTaken2 = results.ElementAt(1).TimeTaken.ToString(@"%m\m\ %s\s");
                                        }

                                        if (results.Count() > 2)
                                        {
                                            interaction.Result3 =
                                                $"{results.ElementAt(2).Score}/{results.ElementAt(2).Total} ({results.ElementAt(2).Percentage.ToString("G")}%)";
                                            interaction.ResultId3 = results.ElementAt(2).Id;
                                            interaction.DateViewed3 = results.ElementAt(2).Created.UtcDateTime;
                                            interaction.TimeTaken3 = results.ElementAt(2).TimeTaken.ToString(@"%m\m\ %s\s");
                                        }
                                        if (results.Count() > 3)
                                        {
                                            interaction.Result4 =
                                                $"{results.ElementAt(3).Score}/{results.ElementAt(3).Total} ({results.ElementAt(3).Percentage.ToString("G")}%)";
                                            interaction.ResultId4 = results.ElementAt(3).Id;
                                            interaction.DateViewed4 = results.ElementAt(3).Created.UtcDateTime;
                                            interaction.TimeTaken4 = results.ElementAt(3).TimeTaken.ToString(@"%m\m\ %s\s");
                                        }
                                        if (results.Count() > 4)
                                        {
                                            interaction.Result5 =
                                                $"{results.ElementAt(4).Score}/{results.ElementAt(4).Total} ({results.ElementAt(4).Percentage.ToString("G")}%)";
                                            interaction.ResultId5 = results.ElementAt(4).Id;
                                            interaction.DateViewed5 = results.ElementAt(4).Created.UtcDateTime;
                                            interaction.TimeTaken5 = results.ElementAt(4).TimeTaken.ToString(@"%m\m\ %s\s");
                                        }

                                        if (results.Count() > 5)
                                        {
                                            interaction.Result6 =
                                                $"{results.ElementAt(5).Score}/{results.ElementAt(5).Total} ({results.ElementAt(5).Percentage.ToString("G")}%)";
                                            interaction.ResultId6 = results.ElementAt(5).Id;
                                            interaction.DateViewed6 = results.ElementAt(5).Created.UtcDateTime;
                                            interaction.TimeTaken6 = results.ElementAt(5).TimeTaken.ToString(@"%m\m\ %s\s");
                                        }
                                        testModels.Add(interaction);
                                    }

                                });
                                while (!testLoopResult.IsCompleted) { continue; }
                                if (!string.IsNullOrEmpty(query.Tags))
                                {
                                    vm.TestInteractions = testModels.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateAssigned).ThenBy(x => x.DateViewed).ToList();

                                }
                                else
                                {
                                    vm.TestInteractions = testModels.OrderBy(x => x.DateAssigned).ThenBy(x => x.DateViewed).ToList();

                                }
                                break;
                            case DocumentType.Policy:
                                var policies_useages = GetDocumentModelsFromAssignment(user.Id, document_documentTypeGrouping, _policyRepository, x => new PolicyModel { Id = x.Id, Title = x.Title }).Where(c => !c.Document.IsGlobalAccessed);
                                var policyIds = policies_useages.Select(x => x.Document.Id).ToList();
                                var policyResponses = _policyResponseRepository.Get(x => x.UserId == user.Id && policyIds.Contains(x.PolicyId) && !x.IsGlobalAccessed).ToList();
                                var policyModels = new ConcurrentBag<PolicyInteractionModel>();
                                var policyLoopResult = Parallel.ForEach(document_documentTypeGrouping, assignedDocument =>
                                {
                                    //var document_usage = policies_useages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any() && !x.Usages.FirstOrDefault().IsGlobalAccessed);
                                    var document_usage = policies_useages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId);
                                    var responses = policyResponses.Where(x => x.PolicyId == assignedDocument.DocumentId && !x.IsGlobalAccessed).OrderByDescending(c => c.Created);
                                    if (document_usage != null)
                                    {
                                        policyModels.Add(new PolicyInteractionModel
                                        {
                                            DocumentTitle = document_usage.Document.Title,
                                            TrainingLabels = document_usage.Document.TrainingLabels,
                                            DateAssigned = assignedDocument.AssignedDate,
                                            IsGlobalAccess = document_usage.Document.IsGlobalAccessed,
                                            Response = !responses.Any() ? !document_usage.Usages.Any() ? "" : "Later" : !responses.First().Response.HasValue ? "Later" : responses.First().Response.Value ? "Yes" : "No",
                                            ResponseDate = !responses.Any() ? new DateTime?() : responses.First().Created,
                                            //DateViewed = !document_usage.Usages.Any() ? new DateTime?() : !responses.Any() ? document_usage.Usages.Last().ViewDate : document_usage.Usages.First().ViewDate,
                                            DateViewed = !document_usage.Usages.Any() ? new DateTime?() : document_usage.Usages.FirstOrDefault().ViewDate,
                                            TimeTaken = !document_usage.Usages.Any() ? null : new TimeSpan(document_usage.Usages.Sum(x => x.Duration.Ticks)).ToString(@"%h\h\ %m\m\ %s\s"),
                                            Viewed = document_usage.Usages.Any()
                                        });
                                    }
                                    else
                                    {

                                        var policyDetails = policies_useages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any());
                                        if (policyDetails != null && policyDetails.Usages.Any())
                                        {
                                            var tm = policyDetails.Usages.FirstOrDefault(c => !c.IsGlobalAccessed);
                                            if (tm != null)
                                            {
                                                policyModels.Add(new PolicyInteractionModel
                                                {
                                                    DocumentTitle = policyDetails.Document.Title,
                                                    TrainingLabels = policyDetails.Document != null ? policyDetails.Document.TrainingLabels : "",
                                                    IsGlobalAccess = false,
                                                    DateAssigned = assignedDocument.AssignedDate,
                                                    Response = !responses.Any() ? !policyDetails.Usages.Any() ? "" : "Later" : !responses.First().Response.HasValue ? "Later" : responses.First().Response.Value ? "Yes" : "No",
                                                    ResponseDate = !responses.Any() ? new DateTime?() : responses.First().Created,
                                                    DateViewed = !policyDetails.Usages.Any() ? new DateTime?() : !responses.Any() ? tm.ViewDate : tm.ViewDate,
                                                    TimeTaken = !policyDetails.Usages.Any() ? null : new TimeSpan(policyDetails.Usages.Sum(x => x.Duration.Ticks)).ToString(@"%h\h\ %m\m\ %s\s"),
                                                    Viewed = policyDetails.Usages.Any()
                                                });
                                            }

                                        }

                                    }
                                });
                                while (!policyLoopResult.IsCompleted) { continue; }
                                if (!string.IsNullOrEmpty(query.Tags))
                                {
                                    vm.PolicyInteractions = policyModels.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateAssigned).ThenBy(x => x.ResponseDate).ToList();
                                }
                                else
                                {
                                    vm.PolicyInteractions = policyModels.OrderBy(x => x.DateAssigned).ThenBy(x => x.ResponseDate).ToList();
                                }

                                break;
                            case DocumentType.Checklist:

                                var checklist_ueages = GetDocumentModelsFromAssignment(user.Id, document_documentTypeGrouping, _checkListRepository, x => new CheckListModel { Id = x.Id, Title = x.Title, CreatedOn = x.CreatedOn, Deleted = x.Deleted }).Where(x => !x.Document.Deleted && !x.Document.IsGlobalAccessed).ToList();

                                var checkListModel = new List<CheckLisInteractionModel>();
                                foreach (var item in checklist_ueages.Where(x => x.Document.Deleted == false))
                                {
                                    //var document_usage = checklist_ueages.FirstOrDefault(x => x.Document.Id == item.Document.Id && x.Usages.Any() && !x.Usages.FirstOrDefault().IsGlobalAccessed);
                                    var document_usage = checklist_ueages.FirstOrDefault(x => x.Document.Id == item.Document.Id);
                                    var checkListChapter = _checkListChapterRepository.Get(c => c.CheckListId == item.Document.Id).ToList();

                                    var assignedDocument = _assignedDocumentRepository.Get(x => x.UserId == query.UserId && x.Deleted == false && x.DocumentId == item.Document.Id).OrderByDescending(c => c.AssignedDate).FirstOrDefault();

                                    var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id).ToList();
                                    var checkListUserResult = _checkListUserResultRepository.GetAll().Where(c => c.AssignedDocumentId == assignedDocument.Id).FirstOrDefault();

                                    if (document_usage != null)
                                    {
                                        var checkList = new CheckLisInteractionModel
                                        {
                                            Id = item.Document.Id,
                                            IsGlobalAccess = document_usage.Document.IsGlobalAccessed,
                                            DocumentTitle = item.Document.Title,
                                            TrainingLabels = document_usage.Document.TrainingLabels,
                                            DateAssigned = assignedDocument.AssignedDate,
                                            //DateViewed = !document_usage.Usages.Any() ? new DateTime?() : document_usage.Usages.Last().ViewDate,
                                            DateViewed = !document_usage.Usages.Any() ? new DateTime?() : document_usage.Usages.FirstOrDefault().ViewDate,
                                            Viewed = document_usage.Usages.Any(),
                                            Completed = (checkListUserResult != null && checkListUserResult.Status) ? "Completed" : "InComplete",
                                            ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}"
                                        };
                                        checkListModel.Add(checkList);
                                    }
                                    else
                                    {

                                        var checklistDetails = checklist_ueages.FirstOrDefault(x => x.Document.Id == assignedDocument.DocumentId && x.Usages.Any());
                                        if (checklistDetails != null && checklistDetails.Usages.Any())
                                        {
                                            var tm = checklistDetails.Usages.FirstOrDefault(c => !c.IsGlobalAccessed);

                                            if (tm != null)
                                            {
                                                checkListModel.Add(new CheckLisInteractionModel
                                                {
                                                    Id = item.Document.Id,
                                                    IsGlobalAccess = false,
                                                    DateAssigned = assignedDocument.AssignedDate,
                                                    DocumentTitle = item.Document.Title,
                                                    TrainingLabels = checklistDetails.Document != null ? checklistDetails.Document.TrainingLabels : "",
                                                    DateViewed = tm.ViewDate,
                                                    Viewed = checklistDetails.Usages.Any(),
                                                    Completed = (checkListUserResult != null && checkListUserResult.Status) ? "Completed" : "InComplete",
                                                    ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}"
                                                });
                                            }

                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(query.Tags))
                                {
                                    vm.CheckLisInteractions = checkListModel.Where(c => c.TrainingLabels.Contains(query.Tags)).ToList();
                                }
                                else
                                {
                                    vm.CheckLisInteractions = checkListModel;
                                }


                                break;
                        }


                    });

                    #region Global Access
                    var memoModels1 = new List<InteractionModel>();
                    var memos_usages1 = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => c.UserId == query.UserId && c.IsGlobalAccessed && c.DocumentType == DocumentType.Memo).ToList();

                    foreach (var memoUses in memos_usages1)
                    {
                        var memoModel = new InteractionModel();
                        var assignedDocument = _assignedDocumentRepository.Get(x => !x.Deleted && x.UserId == query.UserId && x.DocumentId == memoUses.DocumentId).ToList();
                        var memo = _memoRepository.Find(memoUses.DocumentId);
                        if (assignedDocument.Count > 0)
                        {
                            memoModel = new InteractionModel
                            {
                                DocumentTitle = memo.Title,
                                DateAssigned = assignedDocument[0].AssignedDate,
                                TrainingLabels = memo.TrainingLabels,
                                DateViewed = memoUses.ViewDate,
                                Viewed = (memoUses != null) ? true : false,
                                IsGlobalAccess = memoUses.IsGlobalAccessed,
                                TimeTaken = new TimeSpan(memoUses.Duration.Ticks).ToString(@"%h\h\ %m\m\ %s\s")
                            };
                        }
                        else
                        {
                            memoModel = new InteractionModel
                            {
                                DocumentTitle = memo.Title,
                                TrainingLabels = memo.TrainingLabels,
                                DateViewed = memoUses.ViewDate,
                                Viewed = (memoUses != null) ? true : false,
                                IsGlobalAccess = memoUses.IsGlobalAccessed,
                                TimeTaken = new TimeSpan(memoUses.Duration.Ticks).ToString(@"%h\h\ %m\m\ %s\s")
                            };
                        }

                        memoModels1.Add(memoModel);
                    }

                    if (!string.IsNullOrEmpty(query.Tags))
                    {
                        vm.MemoGlobalInteractions = memoModels1.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateViewed).ToList();
                    }
                    else
                    {
                        vm.MemoGlobalInteractions = memoModels1.OrderBy(x => x.DateViewed).ToList();
                    }


                    var trainingManualModels1 = new List<InteractionModel>();
                    var trainingManuals_usages1 = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => c.UserId == query.UserId && c.IsGlobalAccessed && c.DocumentType == DocumentType.TrainingManual).ToList();

                    foreach (var trainingManualsusage in trainingManuals_usages1)
                    {
                        var trainingManualModel = new InteractionModel();
                        var assignedDocument = _assignedDocumentRepository.Get(x => !x.Deleted && x.UserId == query.UserId && x.DocumentId == trainingManualsusage.DocumentId).ToList();
                        var trainingManual = _trainingManualRepository.Find(trainingManualsusage.DocumentId);
                        if (assignedDocument.Count > 0)
                        {
                            trainingManualModel = new InteractionModel
                            {
                                DocumentTitle = trainingManual.Title,
                                DateAssigned = assignedDocument[0].AssignedDate,
                                TrainingLabels = trainingManual.TrainingLabels,
                                DateViewed = trainingManualsusage.ViewDate,
                                IsGlobalAccess = trainingManualsusage.IsGlobalAccessed,
                                Viewed = (trainingManualsusage != null) ? true : false,
                                TimeTaken = new TimeSpan(trainingManualsusage.Duration.Ticks).ToString(@"%h\h\ %m\m")
                            };
                        }
                        else
                        {
                            trainingManualModel = new InteractionModel
                            {
                                DocumentTitle = trainingManual.Title,
                                TrainingLabels = trainingManual.TrainingLabels,
                                DateViewed = trainingManualsusage.ViewDate,
                                IsGlobalAccess = trainingManualsusage.IsGlobalAccessed,
                                Viewed = (trainingManualsusage != null) ? true : false,
                                TimeTaken = new TimeSpan(trainingManualsusage.Duration.Ticks).ToString(@"%h\h\ %m\m")
                            };
                        }
                        trainingManualModels1.Add(trainingManualModel);
                    }

                    if (!string.IsNullOrEmpty(query.Tags))
                    {
                        vm.TrainingManualGlobalInteractions = trainingManualModels1.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateViewed).ToList();
                    }
                    else
                    {
                        vm.TrainingManualGlobalInteractions = trainingManualModels1.OrderBy(x => x.DateViewed).ToList();
                    }

                    var testModels1 = new List<TestInteractionModel>();
                    var testResults1 = _testResultRepository.List.AsQueryable().Include(r => r.Test).Include(r => r.Questions).Where(x => x.UserId == user.Id && x.IsGloballyAccessed).GroupBy(c => c.TestId).Select(c => c.FirstOrDefault()).ToList();

                    foreach (var testResult in testResults1.OrderBy(x => x.Created).ToList())
                    {
                        var test = _testRepository.Find(testResult.TestId);
                        int count = 1;

                        var results = _testResultRepository.List.AsQueryable().Include(r => r.Test).Include(r => r.Questions).Where(x => x.UserId == user.Id && x.TestId == testResult.TestId && x.IsGloballyAccessed).ToList();
                        //.Select(x => new { x.Score, x.Questions, x.Percentage, x.Passed, x.Id, x.Created, x.TimeTaken, x.Total, Title = x.Test.Title, TestId = x.TestId }).OrderBy(x => x.Created).ToList();
                        foreach (var item in results.ToList())
                        {
                            var userTestResult = new UserTestResult()
                            {
                                Attempt = "Attempt " + count,
                                Viewed = true,
                                IsGlobalAccess = item.IsGloballyAccessed,
                                DateViewed = item.Created.UtcDateTime,
                                Result = $"{item.Score}/{item.Total} ({item.Percentage.ToString("G")}%)",
                                FinalResult = item.Passed ? "Passed" : "Failed",
                                TestId = test.Id,
                                Title = test.Title
                            };
                            vm.TestResultGlobalList.Add(userTestResult);
                            count++;
                        }
                        vm.Title = test.Title;
                        if (!results.Any())
                            testModels1.Add(new TestInteractionModel
                            {
                                DocumentTitle = test.Title,
                                Viewed = false,
                                IsGlobalAccess = true,
                                TrainingLabels = test.TrainingLabels
                            });
                        else
                        {
                            var interaction = new TestInteractionModel
                            {
                                DocumentTitle = test.Title,
                                Viewed = true,
                                IsGlobalAccess = true,
                                TrainingLabels = test.TrainingLabels,
                                Result1 = $"{results.First().Score}/{results.First().Total} ({results.First().Percentage.ToString("G")}%)",
                                ResultId1 = results.First().Id,
                                DateViewed1 = results.First().Created.UtcDateTime,
                                TimeTaken1 = results.First().TimeTaken.ToString(@"%m\m\ %s\s")
                            };
                            if (results.Count() > 1)
                            {
                                interaction.Result2 =
                                    $"{results.ElementAt(1).Score}/{results.ElementAt(1).Total} ({results.ElementAt(1).Percentage.ToString("G")}%)";
                                interaction.ResultId2 = results.ElementAt(1).Id;
                                interaction.DateViewed2 = results.ElementAt(1).Created.UtcDateTime;
                                interaction.TimeTaken2 = results.ElementAt(1).TimeTaken.ToString(@"%m\m\ %s\s");
                            }

                            if (results.Count() > 2)
                            {
                                interaction.Result3 =
                                    $"{results.ElementAt(2).Score}/{results.ElementAt(2).Total} ({results.ElementAt(2).Percentage.ToString("G")}%)";
                                interaction.ResultId3 = results.ElementAt(2).Id;
                                interaction.DateViewed3 = results.ElementAt(2).Created.UtcDateTime;
                                interaction.TimeTaken3 = results.ElementAt(2).TimeTaken.ToString(@"%m\m\ %s\s");
                            }
                            if (results.Count() > 3)
                            {
                                interaction.Result4 =
                                    $"{results.ElementAt(3).Score}/{results.ElementAt(3).Total} ({results.ElementAt(3).Percentage.ToString("G")}%)";
                                interaction.ResultId4 = results.ElementAt(3).Id;
                                interaction.DateViewed4 = results.ElementAt(3).Created.UtcDateTime;
                                interaction.TimeTaken4 = results.ElementAt(3).TimeTaken.ToString(@"%m\m\ %s\s");
                            }
                            if (results.Count() > 4)
                            {
                                interaction.Result5 =
                                    $"{results.ElementAt(4).Score}/{results.ElementAt(4).Total} ({results.ElementAt(4).Percentage.ToString("G")}%)";
                                interaction.ResultId5 = results.ElementAt(4).Id;
                                interaction.DateViewed5 = results.ElementAt(4).Created.UtcDateTime;
                                interaction.TimeTaken5 = results.ElementAt(4).TimeTaken.ToString(@"%m\m\ %s\s");
                            }

                            if (results.Count() > 5)
                            {
                                interaction.Result6 =
                                    $"{results.ElementAt(5).Score}/{results.ElementAt(5).Total} ({results.ElementAt(5).Percentage.ToString("G")}%)";
                                interaction.ResultId6 = results.ElementAt(5).Id;
                                interaction.DateViewed6 = results.ElementAt(5).Created.UtcDateTime;
                                interaction.TimeTaken6 = results.ElementAt(5).TimeTaken.ToString(@"%m\m\ %s\s");
                            }
                            testModels1.Add(interaction);
                        }
                    }

                    if (!string.IsNullOrEmpty(query.Tags))
                    {
                        vm.TestGlobalInteractions = testModels1.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.DateViewed).ToList();

                    }
                    else
                    {
                        vm.TestGlobalInteractions = testModels1.OrderBy(x => x.DateViewed).ToList();

                    }
                    var policyModels1 = new List<PolicyInteractionModel>();
                    var policyResponsesList1 = _policyResponseRepository.Get(x => x.UserId == user.Id && x.IsGlobalAccessed).ToList();
                    foreach (var policyResponse in policyResponsesList1)
                    {
                        var policy = _policyRepository.Find(policyResponse.PolicyId);
                        var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => c.DocumentId == policy.Id && c.UserId == query.UserId && c.IsGlobalAccessed && c.DocumentType == DocumentType.Policy).ToList();
                        var policyModel = new PolicyInteractionModel
                        {
                            DocumentTitle = policy.Title,
                            TrainingLabels = policy.TrainingLabels,
                            IsGlobalAccess = policyResponse.IsGlobalAccessed,
                            Response = policyResponse.Response.HasValue && policyResponse.Response.Value ? "Yes" : "No",
                            ResponseDate = policyResponse.Created,
                            DateViewed = (docUses != null && docUses.Any()) ? docUses.FirstOrDefault().ViewDate : DateTime.UtcNow,
                            Viewed = (docUses != null) ? true : false,
                            TimeTaken = !docUses.Any() ? null : new TimeSpan(docUses.Sum(x => x.Duration.Ticks)).ToString(@"%h\h\ %m\m"),
                        };
                        policyModels1.Add(policyModel);
                    }

                    if (!string.IsNullOrEmpty(query.Tags))
                    {
                        vm.PolicyGlobalInteractions = policyModels1.Where(c => c.TrainingLabels.Contains(query.Tags)).OrderBy(x => x.ResponseDate).ToList();
                    }
                    else
                    {
                        vm.PolicyGlobalInteractions = policyModels1.OrderBy(x => x.ResponseDate).ToList();
                    }


                    var checkListModel1 = new List<CheckLisInteractionModel>();
                    var checkListUserResultList1 = _checkListUserResultRepository.GetAll().Where(c => c.UserId == query.UserId && c.IsGlobalAccessed).ToList();

                    foreach (var checkListUserResult in checkListUserResultList1)
                    {
                        var checklistDetail = _checkListRepository.Find(checkListUserResult.DocumentId);

                        var checkListChapterUserResult = _checkListChapterUserResultRepository.GetAll().Where(c => c.DocumentId == checkListUserResult.DocumentId && c.IsGlobalAccessed && c.UserId == checkListUserResult.UserId).ToList();
                        var checkListChapter = _checkListChapterRepository.Get(c => c.CheckListId == checkListUserResult.DocumentId && !c.Deleted).ToList();

                        var checkList = new CheckLisInteractionModel
                        {
                            Id = checklistDetail.Id,
                            DocumentTitle = checklistDetail.Title,
                            IsGlobalAccess = checkListUserResult.IsGlobalAccessed,
                            TrainingLabels = checklistDetail.TrainingLabels,
                            Completed = (checkListUserResult != null && checkListUserResult.Status) ? "Completed" : "InComplete",
                            ChecksCompleted = $"{checkListChapterUserResult.Where(c => c.IsChecked == true).Count()}/{checkListChapter.Count()}"
                        };

                        var docUses = _documentUsageRepository.List.AsQueryable().OrderByDescending(p => p.ViewDate).Where(c => c.DocumentId == checkList.Id && c.UserId == query.UserId && c.IsGlobalAccessed && c.DocumentType == DocumentType.Checklist).ToList();

                        checkList.Viewed = (docUses != null && docUses.Any()) ? true : false;
                        checkList.DateViewed = (docUses != null && docUses.Any()) ? docUses.FirstOrDefault().ViewDate : DateTime.UtcNow;

                        checkListModel1.Add(checkList);
                    }
                    if (!string.IsNullOrEmpty(query.Tags))
                    {
                        vm.CheckListGlobalInteractions = checkListModel1.Where(c => c.TrainingLabels.Contains(query.Tags)).ToList();
                    }
                    else
                    {
                        vm.CheckListGlobalInteractions = checkListModel1;
                    }
                    #endregion

                    vm.PointsStatement = _pointsStatementQueryHandler.ExecuteQuery(new PointsStatementQuery
                    {
                        UserId = query.UserId.AsGuid(),
                        FromDate = query.FromDate,
                        ToDate = query.ToDate,
                        EnableGlobalAccessDocuments = query.PortalContext.UserCompany.EnableGlobalAccessDocuments,
                        TrainingLabels = !string.IsNullOrEmpty(query.Tags) ? query.Tags.Split(',').ToList() : new List<string>(),
                        IsChecklistEnable = query.IsChecklistEnable
                    })
                    .Data
                    .Where(x => x.Points > 0)
                    .Select(x => new UserActivityAndPerformanceViewModel.PointModel
                    {
                        Title = x.DocumentTitle,
                        Type = VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(x.DocumentType),
                        Date = x.Date,
                        Points = x.Points,
                        IsGlobalAccess = x.IsGlobalAccessed ? "Global" : "Assigned"

                    }).ToList();

                    if (vm.EnableTrainingActivities)
                    {
                        vm.TrainingActivities = _trainingActivityReportQueryHandler.ExecuteQuery(new TrainingActivityListQuery
                        {
                            UsersTrained = new[]
                                {
                            new UserModelShort
                            {
                                Id = query.UserId.AsGuid()
                            }
                        },
                            From = query.FromDate,
                            IsChecklistEnable = query.IsChecklistEnable
                        })
                        .FilteredResults
                        .OrderBy(x => x.Created)
                        .Select(x => new UserActivityAndPerformanceViewModel.TrainingActivityModelShort
                        {
                            Title = x.Title,
                            FromDate = x.From,
                            ToDate = x.To,
                            Cost = $"R {x.CostImplication.ToString()}",
                            Type = x.TrainingActivityType.HasValue ? VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(x.TrainingActivityType) : "",
                            Points = x.RewardPoints ?? 0,
                            ActivityId = x.Id
                        }).ToList();
                        vm.PointsStatement.AddRange(vm.TrainingActivities.Where(x => x.Points > 0).Select(x => new UserActivityAndPerformanceViewModel.PointModel
                        {
                            Title = x.Title,
                            Type = "Other",
                            Date = x.FromDate ?? x.ToDate ?? query.FromDate ?? DateTime.UtcNow,
                            Points = x.Points
                        }));
                    }

                    vm.PointsStatement = vm.PointsStatement.OrderBy(x => x.Date).ToList();

                    vm.Feedback = _feedbackQueryHandler.ExecuteQuery(new FilteredUserFeedbackQuery
                    {
                        UserId = query.UserId,
                        FromDate = query.FromDate,
                        ToDate = query.ToDate,

                    }).Select(x => new UserActivityAndPerformanceViewModel.FeedbackModel
                    {
                        DocumentType = VirtuaCon.EnumUtility.GetFriendlyName<DocumentType>(x.DocumentType ?? DocumentType.Unknown),
                        DocumentTitle = x.DocumentTitle,
                        Date = x.Created.UtcDateTime,
                        Type = VirtuaCon.EnumUtility.GetFriendlyName<UserFeedbackContentType>(x.ContentType),
                        Comment = x.Content
                    }).ToList();
                }
            }
            if (query.FromDate.HasValue && query.ToDate.HasValue)
            {
                vm.CheckListGlobalInteractions = vm.CheckListGlobalInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
                vm.CheckLisInteractions = vm.CheckLisInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();

                vm.MemoGlobalInteractions = vm.MemoGlobalInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
                vm.MemoInteractions = vm.MemoInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();

                vm.PolicyGlobalInteractions = vm.PolicyGlobalInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
                vm.PolicyInteractions = vm.PolicyInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();

                vm.TrainingManualGlobalInteractions = vm.TrainingManualGlobalInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
                vm.TrainingManualInteractions = vm.TrainingManualInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();

                vm.TestGlobalInteractions = vm.TestGlobalInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
                vm.TestInteractions = vm.TestInteractions.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();

                vm.TestResultGlobalList = vm.TestResultGlobalList.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
                vm.TestResultList = vm.TestResultList.Where(x => x != null && x.DateAssigned != null && x.DateAssigned >= query.FromDate && x.DateAssigned <= query.ToDate).ToList();
            }
            return vm;
        }
    }
}