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
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class PointsExportReportQueryHandler : IQueryHandler<PointsExportReportParameter, PointsExportViewModel>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public PointsExportReportQueryHandler(IQueryExecutor queryExecutor,
            IRepository<StandardUser> userRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<Domain.Customer.Models.Categories> categoryRepository,
            IRepository<TrainingGuide> trainingGuideRepository)
        {
            _queryExecutor = queryExecutor;
            _userRepository = userRepository;
            _testResultRepository = testResultRepository;
            _categoryRepository = categoryRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public PointsExportViewModel ExecuteQuery(PointsExportReportParameter queryParameters)
        {
            var vm = new PointsExportViewModel();
            var allCategories = _categoryRepository.List.AsQueryable().OrderBy(c => c.CategoryTitle);
            var allCompanies = _queryExecutor.Execute<CustomerCompanyQueryParameter, CompanyViewModelLong>(new CustomerCompanyQueryParameter
            {
                IsForAdmin = !queryParameters.ProvisionalCompanyId.HasValue,
                ProvisionalCompanyId = queryParameters.ProvisionalCompanyId.HasValue ? queryParameters.ProvisionalCompanyId.Value : new Guid()
            }).CompanyList;
            var allGroups = _queryExecutor.Execute<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter());
            var allUsers = _userRepository.List.AsQueryable().Where(x => x.Roles.Any(r => r.RoleName.Equals(Ramp.Contracts.Security.Role.StandardUser) && x.Group != null)).OrderBy(u => u.FirstName);
            vm.Categories = allCategories.Select(Project.ToCategoryModel).ToList();
            vm.Companies = allCompanies;
            vm.Groups = allGroups;
            
            if (queryParameters.GroupId.HasValue)
                allUsers = allUsers.Where(x => x.Group.Id.Equals(queryParameters.GroupId.Value)).OrderBy(u => u.FirstName);

            vm.Users = allUsers.Select(x => new UserViewModel
            {
                EmployeeNo = x.EmployeeNo,
                GroupName = x.Group.Title,
                SelectedGroupId = x.Group.Id,
                Id = x.Id,
                FullName = x.FirstName +" "+ x.LastName
            }).ToList();

            var results = _testResultRepository.List.AsQueryable();
            
            if (queryParameters.UserId.HasValue)
                results = results.Where(c => c.TestTakenByUserId == queryParameters.UserId.Value);
            if (queryParameters.From.HasValue)
            {
                queryParameters.From = queryParameters.From.AtBeginningOfDay();
                results = results.Where(x => x.TestDate >= queryParameters.From.Value);
            }
            if (queryParameters.To.HasValue)
            {
                queryParameters.To = queryParameters.To.AtEndOfDay();
                results = results.Where(x => x.TestDate <= queryParameters.To.Value);
            }
            if (queryParameters.CategoryId.HasValue)
                results = results.Where(x => x.TrainingGuideCategoryId.HasValue && x.TrainingGuideCategoryId.Value.Equals(queryParameters.CategoryId.Value));

            results = results.OrderByDescending(c => c.TestDate).ThenBy(c => c.TestTakenByUserId);
            var userIds = results.ToList().Select(c => c.TestTakenByUserId).Distinct();

            var users = allUsers.Where(c => userIds.Contains(c.Id)).Select(x => new PointsExportViewModel.UserDetail
            {
                EmployeeNo = x.EmployeeNo,
                GroupName = x.Group.Title,
                Id = x.Id,
                Name = x.FirstName + " " +  x.LastName
            }).ToList();
           

            foreach (var user in users)
            {
                results.Where(x => x.TestTakenByUserId.Equals(user.Id)).ToList().ForEach(x => vm.Data.Items.Add(new PointsExportViewModel.TestHistorySummaryDataItem
                {
                    Date = x.TestDate,
                    Passed = x.TestResultStatus,
                    Result = x.TestScore,
                    MaxResult = x.Total,
                    Percentage = Math.Round((double)(x.TestScore)/(double)(x.Total)*100,2),
                    MarksObtain = x.TestScore,
                    PointsObtain = x.TestResultStatus ? x.Points : 0,
                    TestName = x.TestTitle,
                    PlaybookName = x.TrainingGuideTitle,
                    PlayBookCategory = x.TrainingGuideCategory,
                    User = user

                }));
            }
            var trainingActivities = _queryExecutor.Execute<TrainingActivityListQuery, TrainingActivityReportModel>(new TrainingActivityListQuery
            {
                From = queryParameters.From,
                To = queryParameters.To,
                Groups = queryParameters.GroupId.HasValue ? new[] { new GroupViewModelShort { Id = queryParameters.GroupId.Value } } : new GroupViewModelShort[0],
                UsersTrained = queryParameters.UserId.HasValue ? new[] { new UserModelShort { Id = queryParameters.UserId.Value } } : new UserModelShort[0]
            });
            vm.TrainingActivities = trainingActivities.FilteredResults;
            return vm;
        }
    }
}