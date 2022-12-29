using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Progress;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Progress
{
    public class GetStandardUserProgressQueryHandler : IQueryHandler<GetStandardUserProgressQuery, StandardUserProgressViewModel>
    {
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TrainingTestUsageStats> _testStatsRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedGuidesRepository;
        private readonly IRepository<TrainingGuideusageStats> _guideStatsRepository;
        public GetStandardUserProgressQueryHandler(IRepository<TestAssigned> assignedTestRepository,
            IRepository<TrainingTestUsageStats> testStatsRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<AssignedTrainingGuides> assignedGuidesRepository,
            IRepository<TrainingGuideusageStats> guideStatsRepository)
        {
            _assignedTestRepository = assignedTestRepository;
            _assignedGuidesRepository = assignedGuidesRepository;
            _testStatsRepository = testStatsRepository;
            _testResultRepository = testResultRepository;
            _guideStatsRepository = guideStatsRepository;
        }
        public StandardUserProgressViewModel ExecuteQuery(GetStandardUserProgressQuery query)
        {
            var result = new StandardUserProgressViewModel();
            var assignedGuides = _assignedGuidesRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId.Value.Equals(query.UserId)).Select(x => x.TrainingGuideId).ToList();
            var viewedGuides = _guideStatsRepository.List.AsQueryable().Where(x => x.UserId.Equals(query.UserId) && assignedGuides.Contains(x.TrainingGuidId) && !x.Unassigned).Select(x => x.TrainingGuidId).Distinct().ToList();
            var assignedTests = _assignedTestRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.UserId.Value.Equals(query.UserId)).Select(x => x.TestId).ToList();
            var viewedTests = _testStatsRepository.List.AsQueryable().Where(x => x.UserId.Equals(query.UserId) && assignedTests.Any(y => y.Equals(x.TrainingTestId)) && !x.Unassigned).Select(x => x.TrainingTestId).Distinct().ToList();
            var passedTests = _testResultRepository.List.AsQueryable().Where(x => x.TestTakenByUserId.Equals(query.UserId) && x.TrainingGuideId.HasValue && assignedTests.Contains(x.TrainingTestId.Value) && x.TestResultStatus && !x.MaximumTestRewritesReached).ToList();

            result.TotalGuides = assignedGuides.Count();
            result.ViewedGuides = viewedGuides.Count();
            result.TotalTests = assignedTests.Count();
            result.ViewedTests = viewedTests.Count();
            result.PassedTests = passedTests.Count();
            if (result.TotalGuides != 0)
                result.GuideProgress = Math.Round(((double)result.ViewedGuides / result.TotalGuides) * 100, 2);
            if (result.TotalTests != 0)
            {
                result.TestProgress = Math.Round(((double)result.PassedTests / result.TotalTests) * 100, 2);
                result.OverallProgress = Math.Round((result.GuideProgress + result.TestProgress) / 2, 2);
            }
            else
            {
                result.OverallProgress = result.GuideProgress;
            }
            return result;
        }
    }
}
