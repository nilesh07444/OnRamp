using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Reporting;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler.CorrespondenceManagement;
using VirtuaCon.Reporting;
using VirtuaCon;
using System.Globalization;
using Domain.Models;
using VirtuaCon.Reporting.Styles;
using System.Drawing;

namespace Ramp.Services.QueryHandler
{
    public class IndividualDevelopmentQueryHandler : ReportingQueryHandler<IndividualDevelopementRecordReportQuery>,
                                                     IQueryHandler<IndividualDevelopementRecordReportQuery, IndividualDevelopmentReportModel>
    {
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly IRepository<TrainingTestUsageStats> _testUsageStatsRepository;
        private readonly IRepository<RaceCodes> _raceCodeRepository;
        private readonly IQueryExecutor _executor;

        public IndividualDevelopmentQueryHandler(IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository,
                                                 IRepository<TestResult> testResultRepository,
                                                 IRepository<StandardUser> userRepository,
                                                 IRepository<TestAssigned> assignedTestRepository,
                                                 IRepository<TrainingTest> testRepository,
                                                 IRepository<TrainingTestUsageStats> testUsageStatsRepository,
                                                 IRepository<TrainingGuide> trainingGuideRepository,
                                                 IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository,
                                                 IRepository<RaceCodes> raceCodeRepository,
                                                 IQueryExecutor executor)
        {
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
            _testResultRepository = testResultRepository;
            _userRepository = userRepository;
            _assignedTestRepository = assignedTestRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
            _testRepository = testRepository;
            _testUsageStatsRepository = testUsageStatsRepository;
            _raceCodeRepository = raceCodeRepository;
            _executor = executor;
        }
        public override void BuildReport(ReportDocument document, out string title, out string recepitent, IndividualDevelopementRecordReportQuery data)
        {
            var result = _executor.Execute<IndividualDevelopementRecordReportQuery, IndividualDevelopmentReportModel>(data).FilteredResults.FirstOrDefault();
            title = "Individual Development Record Report";
            recepitent = "";
            var section = CreateSection("", PageOrientation.Portrait);
            if (data.PortalContext != null &&
                data.PortalContext.UserCompany != null)
            {
                if (data.PortalContext.UserCompany.CustomerConfigurations.Where(w => w.Type == CustomerConfigurationType.DashboardLogo).OrderByDescending(x => x.Version).FirstOrDefault(w => w.UploadModel != null && w.UploadModel.Data != null) != null)
                    section.AddElement(CreateCompanyLogo(data.PortalContext));
                //else add defualt image
            }
            section.AddElement(new ParagraphBlock("Individual Development Record Report"));



            if (result.User != null)
            {
                section.AddElement(CreateSubHeading("Personal Information"));
                var user = CreateGrid();
                user.ColumnWidths.Add(50);
                user.ColumnWidths.Add(50);
                CreateTableDataRowWithStyles(user, HeaderStyle, new[] { "Full Name", "ID Number" });
                CreateTableDataRow(user, new[] { result.User.FullName, result.User.IDNumber });
                CreateTableDataRowWithStyles(user, HeaderStyle, new[] { "Gender", "Race Code" });
                CreateTableDataRow(user, new[] { result.User.Gender, result.User.RaceCodes.FirstOrDefault()?.Description });
                CreateTableDataRowWithStyles(user, HeaderStyle, new[] { "Employee Number", "Email Address" });
                CreateTableDataRow(user, new[] { result.User.EmployeeNo, result.User.EmailAddress });
                CreateTableDataRowWithStyles(user, HeaderStyle, new[] { "Mobile Number", "Group" });
                CreateTableDataRow(user, new[] { result.User.MobileNumber, result.User.GroupList.FirstOrDefault()?.Name });
                section.AddElement(user);
                section.AddElement(CreateCenteredHorizontalRule());
            }
            if (result.OnRampTraining != null && result.OnRampTraining.Playbooks.Any())
            {
                section.AddElement(CreateSubHeading("Online Training Activities"));
                var playbooks = CreateGrid();
                CreateTableHeader(playbooks,
                    new[]
                    {
                        new Tuple<string,int>("Playbook Name",50),
                        new Tuple<string,int>("Date Assigned",10),
                        new Tuple<string,int>("Viewed",10),
                        new Tuple<string,int>("Test Assigned",10),
                        new Tuple<string, int>("Test Assigned Data",10),
                        new Tuple<string,int>("Test Attempted",10),
                        new Tuple<string,int>("Test Result",10)
                    });
                var green = new List<string>();
                var red = new List<string>();
                result.OnRampTraining.Playbooks.ForEach(delegate (PlaybookSummaryModel model)
                {
                    if (model.Test.Assigned && model.Test.TakenTest && model.Test.Passed.HasValue && model.Test.Passed.Value)
                        green.Add(model.Title);
                    else if (model.Test.Assigned && model.Test.TakenTest && model.Test.Passed.HasValue && !model.Test.Passed.Value)
                    {
                        red.Add(model.Title);
                    }
                    CreateTableDataRow(playbooks, new[]
                    {
                        model.Title,
                        model.DateAssigned.HasValue ? model.DateAssigned.Value.ToShortDateString() : "",
                        model.Viewed.HasValue ? model.Viewed.Value ? "Yes" : "No"  :"No" ,
                        model.Test.Assigned ? "Yes" : "No",
                        model.Test.DateAssigned.HasValue ? model.Test.DateAssigned.Value.ToShortDateString() :"",
                        model.Test.TakenTest ? "Yes" : "No",
                        model.Test.Percentage.HasValue ? model.Test.Percentage.Value.ToString() : "0"
                    });
                });
                playbooks.Elements.ForEach(delegate (GridRowBlock row)
                {
                    var t = row.Elements[0].Value.ToString();
                    if (green.Contains(t))
                        row.Elements[6].AddStyle(new BackgroundColorElementStyle(ColorTranslator.FromHtml("#dff0d8")));
                    else if (red.Contains(t))
                        row.Elements[6].AddStyle(new BackgroundColorElementStyle(ColorTranslator.FromHtml("#f2dede")));
                    else
                        row.Elements[6].Value = "";
                });
                section.AddElement(playbooks);
                section.AddElement(CreateCenteredHorizontalRule());
            }
            if (result.TrainingActivities.Any())
            {
                section.AddElement(CreateSubHeading("Other Training Activities"));
                var training = CreateGrid();
                CreateTableHeader(training,
                    new[]
                    {
                        new Tuple<string,int>("Date",10),
                        new Tuple<string,int>("Title",20),
                        new Tuple<string,int>("Description",30),
                        new Tuple<string,int>("Type",10),
                        new Tuple<string,int>("Reward Points",10),
                        new Tuple<string,int>("Training Labels",20)
                    });
                result.TrainingActivities.ForEach(delegate (TrainingActivityModel model)
                {
                    CreateTableDataRow(training, new[]
                    {
                        model.From.HasValue ? model.From.Value.ToShortDateString() : "",
                        model.Title,
                        model.Description,
                        VirtuaCon.EnumUtility.GetFriendlyName<TrainingActivityType>(model.TrainingActivityType),
                        model.RewardPoints.HasValue ? model.RewardPoints.Value.ToString() : "",
                        model.TrainingLabels
                    });
                });
                section.AddElement(training);
                section.AddElement(CreateCenteredHorizontalRule());
            }


            section.AddElement(CreateSubHeading("Report Criteria"));
            var range = "";
            if (data.From.HasValue && data.To.HasValue)
                range = $"{data.From.Value.ToShortDateString()} - {data.To.Value.ToShortDateString()}";
            else if (data.From.HasValue && !data.To.HasValue)
                range = $"{data.From.Value.ToShortDateString()} - infinity";
            else if (!data.From.HasValue && data.To.HasValue)
                range = $"infinity - {data.To.Value.ToShortDateString()}";
            else
            {
                range = $"infinity";
            }
            var criteria = CreateGrid();
            criteria.ColumnWidths.Add(50);
            criteria.ColumnWidths.Add(50);
            CreateTableDataRowWithStyles(criteria, HeaderStyle, new[] { "Date Range", "Date Generated" });
            CreateTableDataRow(criteria, new[] { range, DateTime.Today.ToShortDateString() });
            section.AddElement(criteria);
            document.AddElement(section);

        }
        public new IndividualDevelopmentReportModel ExecuteQuery(IndividualDevelopementRecordReportQuery query)
        {
            var user = _userRepository.Find(query.UserId.ConvertToGuid());
            if (user == null)
                return null;
            var userModel = Project.UserViewModelFrom(user);
            RaceCodeViewModel raceCodeModel = new RaceCodeViewModel { Description = "None" };
            if (user.RaceCodeId.HasValue)
            {
                var raceCode = _raceCodeRepository.Find(user.RaceCodeId);
                if (raceCode != null)
                    raceCodeModel = new RaceCodeViewModel
                    {
                        Code = raceCode.Code,
                        Description = raceCode.Description,
                        Id = raceCode.Id
                    };
            }
            userModel.RaceCodes.Add(raceCodeModel);
            if (!userModel.GroupList.Any())
                userModel.GroupList.Add(new GroupViewModelShort() { Name = "None" });

            var reportModel = _executor.Execute<TrainingActivityListQuery, TrainingActivityReportModel>(
                new TrainingActivityListQuery
                {
                    From = query.From,
                    To = query.To,
                    UsersTrained = new[] { Project.StandardUserToUserModelShort.Compile().Invoke(user) }
                });
            return new IndividualDevelopmentReportModel
            {
                FilteredResults = new[] { new IndividualDevelopmentModel{
                                                                            OnRampTraining = GetOnRampTraining(query, user),
                                                                            TrainingActivities = reportModel.FilteredResults.OrderBy(x => x.From).ThenBy(x => x.Title).ToList(),
                                                                            User = userModel ,
                                                                            CompanyLogoId = query.PortalContext?.UserCompany?.CustomerConfigurations.OrderByDescending(x => x.Version).FirstOrDefault(x => x.Type == Domain.Models.CustomerConfigurationType.DashboardLogo)?.UploadModel?.Id.ToString()
                                                                        }
                }
            };
        }
        private OnRampTrainingModel GetOnRampTraining(IndividualDevelopementRecordReportQuery query, StandardUser user)
        {
            var from = query.From.AtBeginningOfDay();
            var to = query.To.AtEndOfDay();
            var result = new OnRampTrainingModel();

            var allAsscociatedPlaybooks = FindAllPlaybooksAssociatedWithUser(user, from, to);
            var guides = FindGuides(user, from, to);
            var tests = FindTests(user, from, to);

            var unassignedGuides = allAsscociatedPlaybooks
                .Select(x => x.ToString())
                .Intersect(tests.Where(x => x.PlaybookId != null).Select(x => x.PlaybookId))
                .Where(x => !guides.Select(g => g.Id).Contains(x)).ToList();

            foreach (var id in unassignedGuides)
            {
                var gId = id.ConvertToGuid();
                var model = Project.ToPlaybookSummaryModel.Compile().Invoke(_trainingGuideRepository.Find(gId));
                model.Viewed = false;
                (result.Playbooks as IList<PlaybookSummaryModel>).Add(model);
            }
            (result.Playbooks as IList<PlaybookSummaryModel>).AddRange(guides);

            foreach (var playbook in result.Playbooks)
            {
                playbook.Test = tests.FirstOrDefault(x => x.ReferenceId == playbook.ReferenceId + "T");
                if (playbook.Test == null)
                    playbook.Test = new TestSummaryModel
                    {
                        Assigned = false,
                        Percentage = new decimal(0),
                        Passed = false,
                        TakenTest = false,
                        ReferenceId = playbook.ReferenceId + "T"
                    };
            }
            result.Playbooks = result.Playbooks.OrderBy(x => x.Title).ToList();
            return result;
        }
        private IList<Guid> FindAllPlaybooksAssociatedWithUser(StandardUser user, DateTime? from, DateTime? to)
        {
            var assignedPlaybooks = _assignedTrainingGuideRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId.Value == user.Id);
            var assignedTests = _assignedTestRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId.Value == user.Id && x.Test != null);

            if (from.HasValue)
            {
                assignedPlaybooks = assignedPlaybooks.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value >= from.Value);
                assignedTests = assignedTests.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value >= from.Value);
            }
            if (to.HasValue)
            {
                assignedPlaybooks = assignedPlaybooks.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value <= to.Value);
                assignedTests = assignedTests.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value <= to.Value);
            }
            var allAssignedPlaybookIds = assignedPlaybooks.Select(x => x.TrainingGuideId).Distinct().ToList();
            var allAssignedTests_PlaybookIds = assignedTests.Select(x => x.Test.TrainingGuideId).ToList();
            var result = new List<Guid>();
            result.AddRange(allAssignedPlaybookIds);
            result.AddRange(allAssignedTests_PlaybookIds.Where(x => x.HasValue).Select(x => x.Value).ToList().Where(x => !result.Contains(x)).ToList());
            return result;

        }
        private IList<PlaybookSummaryModel> FindGuides(StandardUser user, DateTime? from, DateTime? to)
        {
            var allAssignedGuides = _assignedTrainingGuideRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId == user.Id && x.TrainingGuide != null);
            if (from.HasValue)
                allAssignedGuides =
                    allAssignedGuides.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value >= from.Value);
            if (to.HasValue)
                allAssignedGuides =
                    allAssignedGuides.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value <= to.Value);

            var assignedIds = allAssignedGuides.Select(x => x.TrainingGuide.Id).ToList();
            var result = _trainingGuideRepository.List.AsQueryable().Where(x => assignedIds.Contains(x.Id)).Select(Project.ToPlaybookSummaryModel).ToList();
            foreach (var model in result)
            {
                var id = model.Id.ConvertToGuid();
                model.Assigned = true;
                var stats = _trainingGuideUsageStatsRepository.List.AsQueryable()
                    .Where(x => x.TrainingGuidId == id &&
                                x.UserId.Equals(user.Id) && !x.Unassigned);
                model.Viewed = stats.ToList().Any();
                model.DateAssigned = allAssignedGuides.FirstOrDefault(x => x.TrainingGuide.Id == id)?.AssignedDate;
            }

            return result;
        }
        private IEnumerable<TestSummaryModel> FindTests(StandardUser user, DateTime? from, DateTime? to)
        {
            var result = new List<TestSummaryModel>();
            var allAssignedTests = _assignedTestRepository.List.AsQueryable()
                .Where(x => x.UserId.HasValue && x.UserId == user.Id && x.Test != null);
            if (from.HasValue)
                allAssignedTests = allAssignedTests.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value >= from.Value);
            if (to.HasValue)
                allAssignedTests = allAssignedTests.Where(x => x.AssignedDate.HasValue && x.AssignedDate.Value <= to.Value);

            var assignedTestIds = allAssignedTests.Select(x => x.Test.Id).ToList();
            var tests = _testRepository.List.AsQueryable().Where(x => assignedTestIds.Contains(x.Id)).Select(Project.ToTestSummaryModel).ToList();

            foreach (var model in tests)
            {
                var id = model.Id.ConvertToGuid();
                var testResult = _testResultRepository.List.AsQueryable()
                .FirstOrDefault(x => x.TestTakenByUserId.Equals(user.Id) &&
                            x.TrainingTestId.HasValue &&
                            x.TrainingTestId.Value == id && x.TestResultStatus);
                model.Assigned = true;
                model.Passed = testResult == null ? false : true;
                model.Percentage = testResult == null ? new decimal(0) : RecalculatePercentage(testResult, model.MaximumScore);
                model.DateAssigned = allAssignedTests.FirstOrDefault(x => x.Test.Id == id)?.AssignedDate;
                var stats = _testUsageStatsRepository.List.AsQueryable().Where(x => x.TrainingTestId == id && x.UserId == user.Id && !x.Unassigned);
                if (from.HasValue)
                    stats = stats.Where(x => x.DateTaken.HasValue && x.DateTaken.Value >= from.Value);
                if (to.HasValue)
                    stats = stats.Where(x => x.DateTaken.HasValue && x.DateTaken.Value <= to.Value);
                model.TakenTest = stats.Any();
                result.Add(model);
            }
            return result;
        }
        private decimal? RecalculatePercentage(TestResult result, int maximumScore)
        {
            return Math.Round(((decimal.Parse(result.TestScore.ToString(CultureInfo.InvariantCulture)) /
                 (decimal.Parse(maximumScore.ToString(CultureInfo.InvariantCulture))))) * 100, 2);
        }
    }
}

namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<TrainingGuide, PlaybookSummaryModel>> ToPlaybookSummaryModel = x
             => new PlaybookSummaryModel
             {
                 Id = x.Id.ToString(),
                 ReferenceId = x.ReferenceId,
                 Title = x.Title
             };
        public static readonly Expression<Func<TrainingTest, TestSummaryModel>> ToTestSummaryModel = x
              => new TestSummaryModel
              {
                  Id = x.Id.ToString(),
                  ReferenceId = x.ReferenceId,
                  PlaybookId = x.TrainingGuide != null ? x.TrainingGuide.Id.ToString() : null,
                  MaximumScore = x.QuestionList.Sum(r => r.AnswerWeightage),
                  PointsScored = x.PassPoints,
                  Version = x.Version
              };
    }

}
