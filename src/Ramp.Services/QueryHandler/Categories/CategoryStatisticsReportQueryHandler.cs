using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.Categories
{
    public class CategoryStatisticsReportQueryHandler : QueryHandlerBase<CategoryStatisticsReportQueryParameter, CategoryStatisticsReportViewModel>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;

        public CategoryStatisticsReportQueryHandler(
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
            IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository,
            IRepository<TestAssigned> assignedTestRepository, 
            IQueryExecutor queryExecutor,
            IRepository<StandardUser> userRepository,
            IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _testResultRepository = testResultRepository;
            _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
            _assignedTestRepository = assignedTestRepository;
            _queryExecutor = queryExecutor;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
        }
        private void Recurse(IEnumerable<Domain.Customer.Models.Categories> categories,Action<Guid> action)
        {
            if (categories.Count() == 0)
                return;
            else
                categories.ToList().ForEach(delegate (Domain.Customer.Models.Categories model)
                {
                    action(model.Id);
                    Recurse(_categoryRepository.List.AsQueryable().Where(x => x.ParentCategoryId.HasValue && x.ParentCategoryId.Value == model.Id), action);
                });
        }
        public override CategoryStatisticsReportViewModel ExecuteQuery(CategoryStatisticsReportQueryParameter queryParameters)
        {
            var categories = new List<CategoryViewModel>();
            var vm = new CategoryStatisticsReportViewModel();
            var company = _queryExecutor.Execute<CustomerCompanyQueryParameter, CompanyViewModelLong>(new CustomerCompanyQueryParameter
            {
                IsForAdmin = !queryParameters.ProvisionalCompanyId.HasValue,
                ProvisionalCompanyId = queryParameters.ProvisionalCompanyId.HasValue ? queryParameters.ProvisionalCompanyId.Value : new Guid()
            });

            if (queryParameters.SelectedCompanyId != Guid.Empty)
                categories = _queryExecutor.Execute<AllCategoriesQueryParameter, List<CategoryViewModel>>(new AllCategoriesQueryParameter());

            vm.Companies = company.CompanyList;
            vm.Categories = categories;
            var alltrainingGuides = _trainingGuideRepository.List.AsQueryable();
            var allUsers = _userRepository.List.AsQueryable();
            var allAssignedGuides = _assignedTrainingGuidesRepository.List.AsQueryable().Where(x => x.UserId.HasValue);
            var allAssignedTests = _assignedTestRepository.List.AsQueryable().Where(x => x.UserId.HasValue && x.Test != null && x.Test.TrainingGuideId.HasValue);
            var allTrainingGuideUsageStats = _trainingGuideUsageStatsRepository.List.AsQueryable();
            var allPassedResults = _testResultRepository.List.AsQueryable().Where(x => x.TestResultStatus && x.TrainingGuideId.HasValue);
            if (queryParameters.SelectedCategoryId != Guid.Empty)
            {
                var applicableCategoryIds = new List<Guid>();
                applicableCategoryIds.Add(queryParameters.SelectedCategoryId);
                Recurse(_categoryRepository.List.AsQueryable().Where(x => x.ParentCategoryId.HasValue && x.ParentCategoryId.Value == queryParameters.SelectedCategoryId), delegate (Guid x)
                 {
                     if (!applicableCategoryIds.Contains(x))
                         applicableCategoryIds.Add(x);
                 });
                alltrainingGuides = alltrainingGuides.Where(x => x.Categories.Any(c => applicableCategoryIds.Contains(c.Id)));
             
                foreach (var guide in alltrainingGuides.ToList())
                {
                    var allocatedUserIds = allAssignedGuides.Where(c => c.TrainingGuideId.Equals(guide.Id)).Select(assigned => assigned.UserId.Value).ToList();
                    var assignedUserIds = allAssignedTests.Where(c => c.Test.TrainingGuideId.Value.Equals(guide.Id)).Select(assigned => assigned.UserId.Value).ToList();
                    var interactedUserIds = allTrainingGuideUsageStats.Where(c => c.TrainingGuidId.Equals(guide.Id)).Select(assigned => assigned.UserId).Distinct().ToList();
                    var passedUserIds = allPassedResults.Where(c => c.TrainingGuideId.Value.Equals(guide.Id)).Select(assigned => assigned.TestTakenByUserId).ToList();
                    var testTakenIds = allAssignedTests.Where(c => c.Test.TrainingGuideId.Value.Equals(guide.Id)).Select(user => user.UserId.Value).Distinct().ToList();
                    var allUsersIds = Combine(allocatedUserIds, assignedUserIds, interactedUserIds, passedUserIds, testTakenIds);
                    var users = allUsers.Where(x => allUsersIds.Contains(x.Id)).ToList();
                    vm.Summary.Items.Add(new CategoryStatisticsReportViewModel.TrainingGuideSummary()
                    {
                        Guide = guide.Title,
                        GuideId = guide.Id,
                        Allocated = allocatedUserIds.Count(u => users.Any(user => user.Id.Equals(u))),
                        Passed = passedUserIds.Count(u => users.Any(user => user.Id.Equals(u))),
                        Interacted = interactedUserIds.Count(u => users.Any(user => user.Id.Equals(u))),
                        Assigned = assignedUserIds.Count(u => users.Any(user => user.Id.Equals(u))),
                        TestTaken = testTakenIds.Count(u => users.Any(user => user.Id.Equals(u)))
                    });
                }
            }
            if (queryParameters.TrainingGuideId.HasValue)
            {
                var detail = new CategoryStatisticsReportViewModel.TrainingGuideDetail();
                var guide = alltrainingGuides.Where(x => x.Id == queryParameters.TrainingGuideId).FirstOrDefault();
                if (guide == null)
                    return vm; 
                detail.Guide = guide.Title;
                detail.GuideId = guide.Id;

                var allocatedUserIds = allAssignedGuides.Where(c => c.TrainingGuideId == queryParameters.TrainingGuideId.Value).Select(assigned => assigned.UserId.Value).ToList();
                var assignedUserIds = allAssignedTests.Where(c => c.Test.TrainingGuideId.Value == queryParameters.TrainingGuideId.Value).Select(assigned => assigned.UserId.Value).ToList();
                var interactedUserIds = allTrainingGuideUsageStats.Where(c => c.TrainingGuidId == queryParameters.TrainingGuideId.Value).Select(assigned => assigned.UserId).Distinct().ToList();
                var passedUserIds = allPassedResults.Where(c => c.TrainingGuideId.Value == queryParameters.TrainingGuideId.Value).Select(assigned => assigned.TestTakenByUserId).ToList();

                var allUserIds = Combine(allocatedUserIds, assignedUserIds, interactedUserIds, passedUserIds);
                var users = allUsers.Where(x => allUserIds.Contains(x.Id));
                detail.Allocated = users.Where(c => allocatedUserIds.Contains(c.Id)).Select(user => new CategoryStatisticsReportViewModel.UserDetail { Id = user.Id, Name = user.FirstName + " " +user.LastName }).ToList();
                detail.Assigned = users.Where(c => assignedUserIds.Contains(c.Id)).Select(user => new CategoryStatisticsReportViewModel.UserDetail { Id = user.Id, Name = user.FirstName + " " + user.LastName }).ToList();
                detail.Passed = users.Where(c => passedUserIds.Contains(c.Id)).Select(user => new CategoryStatisticsReportViewModel.UserDetail { Id = user.Id, Name =  user.FirstName + " " + user.LastName }).ToList();
                detail.Interacted = users.Where(c => interactedUserIds.Contains(c.Id)).Select(user => new CategoryStatisticsReportViewModel.UserDetail { Id = user.Id, Name = user.FirstName + " " + user.LastName }).ToList();
                vm.Detail = detail;
            }
            return vm;
        }

        private List<Guid> Combine(params List<Guid>[] userIdLists)
        {
            var result = new List<Guid>();
            userIdLists.ToList().ForEach(delegate (List<Guid> x)
            {
                x.ForEach(delegate (Guid id)
                {
                    if (!result.Contains(id))
                        result.Add(id);
                });
            });
            return result;
        }
    }
}