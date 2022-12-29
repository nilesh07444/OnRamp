using Common;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class TestHistoryReportQueryHandler : QueryHandlerBase<TestHistoryReportParameter, TestHistoryReportViewModel>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TestResult> _testResultRepository;

        public TestHistoryReportQueryHandler(IQueryExecutor queryExecutor, IRepository<StandardUser> userRepository, IRepository<TestResult> testResultRepository)
        {
            _queryExecutor = queryExecutor;
            _userRepository = userRepository;
            _testResultRepository = testResultRepository;
        }

        public override TestHistoryReportViewModel ExecuteQuery(TestHistoryReportParameter queryParameters)
        {
            var vm = new TestHistoryReportViewModel();
            var allCompanies = _queryExecutor.Execute<CustomerCompanyQueryParameter, CompanyViewModelLong>(new CustomerCompanyQueryParameter
            {
                IsForAdmin = !queryParameters.ProvisionalCompanyId.HasValue,
                ProvisionalCompanyId = queryParameters.ProvisionalCompanyId.HasValue ? queryParameters.ProvisionalCompanyId.Value : new Guid()
            }).CompanyList;
            var allGroups = _queryExecutor.Execute<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter());
            var allGuides = _queryExecutor.Execute<AllTrainingGuideQueryParameter, List<TrainingGuideViewModel>>(new AllTrainingGuideQueryParameter());
            var allUsers = _queryExecutor.Execute<AllStandardUserQueryParameter, List<UserViewModel>>(new AllStandardUserQueryParameter());
            var allResults = _testResultRepository.List.AsQueryable();

            vm.Companies = allCompanies;
            vm.Groups = allGroups;
            vm.TrainingGuides = allGuides;
            if (queryParameters.GroupId.HasValue)
                allUsers = allUsers.Where(x => x.SelectedGroupId == queryParameters.GroupId.Value).ToList();
            vm.Users = allUsers;

            if (queryParameters.TrainingGuideId.HasValue)
            {
                allResults = allResults.Where(c => c.TrainingGuideId.HasValue && c.TrainingGuideId.Value == queryParameters.TrainingGuideId.Value);

                if (queryParameters.UserId.HasValue)
                    allResults = allResults.Where(c => c.TestTakenByUserId == queryParameters.UserId.Value);
                if (queryParameters.GroupId.HasValue)
                {
                    var applicableUsers = allUsers.Select(u => u.Id).ToList();
                    allResults = allResults.Where(x => applicableUsers.Contains(x.TestTakenByUserId));
                }
                if (queryParameters.To.HasValue)
                {
                    queryParameters.To = queryParameters.To.AtEndOfDay();
                    allResults = allResults.Where(c => c.TestDate <= queryParameters.To.Value);
                }
                if (queryParameters.From.HasValue)
                {
                    queryParameters.From = queryParameters.From.AtBeginningOfDay();
                    allResults = allResults.Where(c => c.TestDate >= queryParameters.From.Value);
                }
                allResults = allResults.OrderByDescending(c => c.TestDate).ThenBy(c => c.TestTakenByUserId);

                var userIds = allResults.Select(c => c.TestTakenByUserId).Distinct().ToList();
                var users = _userRepository.List.AsQueryable().Where(c => userIds.Contains(c.Id));

                foreach (var result in allResults.ToList())
                {
                    var user = users.FirstOrDefault(c => c.Id == result.TestTakenByUserId);
                    if (user != null && user.Group != null)
                    {
                        vm.Data.Items.Add(new TestHistoryReportViewModel.TestHistorySummaryDataItem
                        {
                            Date = result.TestDate,
                            Passed = result.TestResultStatus,
                            Result = result.TestScore,
                            MaxResult = result.Total,
                            Percentage = Math.Round(((double)result.TestScore / (double)result.Total) * 100, 2),
                            MarksObtain = result.TestScore,
                            TestName = result.TestTitle,
                            PlaybookName = result.TrainingGuideTitle,
                            User = new TestHistoryReportViewModel.UserDetail
                            {
                                Id = user.Id,
                                Name = string.Format("{0} {1}", user.FirstName, user.LastName),
                                EmployeeNo = user.EmployeeNo,
                                GroupName = user.Group.Title
                            },
                            Version = result.Version
                        });
                    }
                }
            }

            return vm;
        }
    }
}