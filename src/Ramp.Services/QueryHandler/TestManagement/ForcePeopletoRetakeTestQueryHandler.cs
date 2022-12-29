using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class ForcePeopletoRetakeTestQueryHandler :
        QueryHandlerBase<ForcePeopletoRetakeTestQueryParameter, SendMailToUsersToReTakeTest>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestAssigned> _assignedtestRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;

        public ForcePeopletoRetakeTestQueryHandler(IRepository<StandardUser> userRepository,
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TestAssigned> assignedtestRepository,
            IRepository<TrainingGuide> guideRepository)
        {
            _userRepository = userRepository;
            _trainingTestRepository = trainingTestRepository;
            _testResultRepository = testResultRepository;
            _assignedtestRepository = assignedtestRepository;
            _guideRepository = guideRepository;
        }

        public override SendMailToUsersToReTakeTest ExecuteQuery(ForcePeopletoRetakeTestQueryParameter queryParameters)
        {
            var trainingTest = _trainingTestRepository.Find(queryParameters.TrainingTestId);
            if (trainingTest == null)
                throw new Exception($"Test not found with id: {queryParameters.TrainingTestId} ");
            if(trainingTest.Version == 0)
                return null;
            var guide = _guideRepository.Find(trainingTest.TrainingGuideId);
            if(guide == null)
                throw new Exception($"Guide not found with id: {trainingTest.TrainingGuideId} ");

            var tests = guide.TestVersion.Versions.Select(x => x.Id).ToList();
            var allUsersAsignedTheTest = _assignedtestRepository.List.Where(t => tests.Contains(t.TestId)).Select(u => u.UserId).ToList();
            var allPreviousAssignedTests = _assignedtestRepository.List.Where(t => tests.Contains(t.TestId)).ToList();
            foreach (var userId in allUsersAsignedTheTest)
            {
                var user = _userRepository.Find(userId);
                if (user != null)
                {
                    new CommandDispatcher().Dispatch(new AssignTestsToUsersOrGroupsCommand
                    {
                        AssignedBy = queryParameters.CurrentlyLoggedInUser,
                        AssignTestToUsersOrGroupViewModel = new AssignTestToUsersOrGroupViewModel
                        {
                            SelectedOption = "User",
                            CustomerStandardUsers = new List<UserViewModel>() { Project.UserViewModelFrom(user) },
                        },
                        CompanyViewModel = queryParameters.Company,
                        TestIds = new List<System.Guid>() { guide.TestVersion.Versions.OrderBy(x => x.Version).Last().Id }
                    });
                    allPreviousAssignedTests.Where(x => x.UserId.Equals(userId)).ToList().ForEach(a => _assignedtestRepository.Delete(a));
                }
            }
            return null;
        }
    }
}