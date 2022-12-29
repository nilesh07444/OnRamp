using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.User
{
    public class UserKpiReportQueryHandler : QueryHandlerBase<UserKpiReportQueryParameter, UserKpiReportViewModel>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<StandardUserActivityLog> _userActivityLogRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;

        public UserKpiReportQueryHandler(IQueryExecutor queryExecutor,
            IRepository<StandardUserActivityLog> userActivityLogRepository,
            IRepository<StandardUser> userRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
        {
            _queryExecutor = queryExecutor;
            _userActivityLogRepository = userActivityLogRepository;
            _userRepository = userRepository;
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
            _testResultRepository = testResultRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
        }

        public override UserKpiReportViewModel ExecuteQuery(UserKpiReportQueryParameter queryParameters)
        {
            var vm = new UserKpiReportViewModel();

            var company = _queryExecutor.Execute<CustomerCompanyQueryParameter, CompanyViewModelLong>(new CustomerCompanyQueryParameter
            {
                IsForAdmin = !queryParameters.ProvisionalCompanyId.HasValue,
                ProvisionalCompanyId = queryParameters.ProvisionalCompanyId.HasValue ? queryParameters.ProvisionalCompanyId.Value : new Guid()
            });

            vm.Companies = company.CompanyList;

            vm.Users =
                _queryExecutor.Execute<AllStandardUserQueryParameter, List<UserViewModel>>(new AllStandardUserQueryParameter());

            if (queryParameters.UserId.HasValue)
            {
                if (queryParameters.UserId != null)
                {
                    var user = _userRepository.Find(queryParameters.UserId);

                    var trainingGuides =
                        _assignedTrainingGuideRepository.List.Where(c => c.UserId == queryParameters.UserId.Value)
                            .Select(c => c.TrainingGuide);

                    foreach (var guide in trainingGuides)
                    {
                        vm.TrainingGuideStatistics.Add(new UserKpiReportViewModel.TrainingGuideStatistic
                        {
                            GuideId = guide.Id,
                            Guide = guide.Title,
                            LookedAt =
                                _trainingGuideUsageStatsRepository.List.Any(
                                    c => c.TrainingGuidId == guide.Id && c.UserId == queryParameters.UserId.Value),
                            Passed =
                                _testResultRepository.List.Any(
                                    c =>
                                        c.TrainingGuideId == guide.Id &&
                                        c.TestTakenByUserId == queryParameters.UserId.Value && c.TestResultStatus)
                        });
                    }
                    foreach (var stat in vm.TrainingGuideStatistics)
                    {
                        if (stat.Passed)
                            stat.LookedAt = true;
                    }

                    var testResultSearch =
                        _testResultRepository.List.Where(c => c.TestTakenByUserId == queryParameters.UserId.Value);

                    if (queryParameters.From.HasValue)
                    {
                        queryParameters.From = queryParameters.From.AtBeginningOfDay();
                        testResultSearch = testResultSearch.Where(c => c.TestDate >= queryParameters.From.Value);
                    }

                    if (queryParameters.To.HasValue)
                    {
                        queryParameters.To = queryParameters.To.AtEndOfDay();
                        testResultSearch = testResultSearch.Where(c => c.TestDate.Date <= queryParameters.To.Value);
                    }

                    testResultSearch = testResultSearch.OrderByDescending(c => c.TestDate);

                    foreach (var result in testResultSearch)
                    {
                        vm.TestResults.Add(new UserKpiReportViewModel.TestResultItem
                        {
                            TestDate = result.TestDate,
                            Guide = result.TrainingGuideTitle,
                            Result = result.TestResultStatus,
                            Score = result.TestScore,
                            MaxScore = result.Total
                        });
                    }

                    foreach (var activity in _userActivityLogRepository.List.Where(
                        c => c.User.Id == queryParameters.UserId.Value)
                        .OrderByDescending(c => c.ActivityDate))
                    {
                        vm.Activity.Add(new UserKpiReportViewModel.ActivityItem
                        {
                            EventDate = activity.ActivityDate,
                            Type = activity.ActivityType
                        });
                    }

                    var userLoginFrequencyReport = _queryExecutor
                        .Execute<UserLoginFrequencyQueryParameter, UserLoginFrequencyReportViewModel>(
                            new UserLoginFrequencyQueryParameter
                            {
                                SelectedUserId = queryParameters.UserId.Value,
                                FromDate = Convert.ToDateTime(queryParameters.From),
                                ToDate = Convert.ToDateTime(queryParameters.To.Value.AddDays(1))
                            });

                    vm.LoginCount = userLoginFrequencyReport.UserLoginCount;
                    vm.LoginFrequency = userLoginFrequencyReport.LoginFrequency;
                }
            }
            return vm;
        }
    }
}