using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class TestNotApperUsersHandler : QueryHandlerBase<TestNotApperUsersParameter, TestNotApperUsersViewModel>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        public TestNotApperUsersHandler(IRepository<StandardUser> userRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<TrainingGuide> guideRepository)
        {
            _userRepository = userRepository;
            _assignedTestRepository = assignedTestRepository;
            _testResultRepository = testResultRepository;
            _trainingTestRepository = trainingTestRepository;
            _guideRepository = guideRepository;
        }

        public override TestNotApperUsersViewModel ExecuteQuery(TestNotApperUsersParameter queryParameters)
        {
            var userModel = new TestNotApperUsersViewModel();
            var allTests = _guideRepository.List.AsQueryable().Where(x => x.TestVersion != null && x.TestVersion.LastPublishedVersion != null).Select(x => x.TestVersion.LastPublishedVersion);
            ((List<TestViewModel>)userModel.TestList).AddRange(allTests.OrderBy(x => x.TestTitle).Select(x => new TestViewModel
            {
                TrainingTestId = x.Id,
                TestTitle = x.TestTitle
            }).ToList());
            if (queryParameters.TestId.HasValue)
            {
                var allActiveTests = allTests.Select(x => x.Id).ToList();
                var allAssignedTests = _assignedTestRepository.List.AsQueryable().Where(x => x.Test != null && x.Test.Id == queryParameters.TestId.Value);
                var allResults = _testResultRepository.List.AsQueryable().Where(x => x.TrainingTestId.HasValue && x.TrainingTestId.Value == queryParameters.TestId.Value).Select(x => new
                {
                    TestId = x.TrainingTestId.Value,
                    UserId = x.TestTakenByUserId
                });
                var notTakenTests = allAssignedTests.Where(x => x.UserId.HasValue && !allResults.Any(r => r.TestId == x.TestId && r.UserId == x.UserId.Value));
                var notTakenTestUserIds = notTakenTests.Select(x => x.UserId.Value);
                var allUsers = _userRepository.List.AsQueryable().Where(x => notTakenTestUserIds.Contains(x.Id)).Select(x => new UserViewModel
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    ContactNumber = x.ContactNumber,
                    CompanyId = x.CompanyId,
                    EmailAddress = x.EmailAddress,
                    MobileNumber = x.MobileNumber,
                    LastName = x.LastName,
                });
                //var testAssignedButNotAppeared = _assignedTestRepository.List.Where(
                //    u => !_testResultRepository.List.Any(taken => taken.TrainingTestId == u.TestId && taken.TestTakenByUserId == u.UserId)
                //    ).ToList();


                foreach (var result in notTakenTests.ToList())
                {
                    var userM = allUsers.FirstOrDefault(x => x.Id == result.UserId);
                    if (userM != null)
                    {
                        userM.TestName = result.Test.TestTitle;
                        userM.TestExpiryDate = result.Test.TestExpiryDate;
                        userM.TestId = result.TestId;
                        userModel.UserList.Add(userM);
                    }
                }
            }
            return userModel;
        }
    }
}