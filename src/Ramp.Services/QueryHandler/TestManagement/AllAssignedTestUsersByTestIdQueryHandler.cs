using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class AllAssignedTestUsersByTestIdQueryHandler :
        QueryHandlerBase<AllAssignedTestUsersByTestIdQueryParameter, TestAssignedUsersAndNotAppearedUsersViewModel>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly IRepository<TestAssigned> _assignedTestRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingTest> _testRepository;

        public AllAssignedTestUsersByTestIdQueryHandler(IRepository<Domain.Models.User> userRepository,
            IRepository<TestAssigned> assignedTestRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<TrainingTest> testRepository)
        {
            _userRepository = userRepository;
            _assignedTestRepository = assignedTestRepository;
            _testResultRepository = testResultRepository;
            _testRepository = testRepository;
        }

        public override TestAssignedUsersAndNotAppearedUsersViewModel ExecuteQuery(
            AllAssignedTestUsersByTestIdQueryParameter queryParameters)
        {
            var viewModel = new TestAssignedUsersAndNotAppearedUsersViewModel();

            var allUsersAppeared = new List<UserViewModel>();

            var allUsersNotAppeared = new List<UserViewModel>();

            var testAssignedAndAppeared =
               _testResultRepository.GetAll()
                   .Where(
                       u => u.TrainingTestId == queryParameters.TestId &&
                            _assignedTestRepository.GetAll()
                                .Any(taken => taken.TestId == u.TrainingTestId && taken.UserId == u.TestTakenByUserId))
                   .ToList();

            foreach (TestResult result in testAssignedAndAppeared)
            {
                Domain.Models.User user = _userRepository.Find(result.TestTakenByUserId);
                if (user != null)
                {
                    var userModel = new UserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        ContactNumber = user.ContactNumber,
                        CompanyId = user.CompanyId,
                        EmailAddress = user.EmailAddress,
                        MobileNumber = user.MobileNumber,
                        LastName = user.LastName,
                    };
                    var actualTest = _testRepository.Find(result.TrainingTestId);
                    var resultModel = new TestResultViewModel
                    {
                        TestResultId = result.Id,
                        NumberOfRightAnswers = result.CorrectAnswers,
                        NumberOfWrongAnswers = result.WrongAnswers,
                        NumberOfUnattemptedQuestions = actualTest.QuestionList.Count - (result.CorrectAnswers + result.WrongAnswers),
                        TestResult = result.TestResultStatus,
                        TotalMarksScored = result.TestScore,
                        TestTitle = result.TestTitle
                    };
                    userModel.TestResult = resultModel;
                    List<UserViewModel> check = allUsersAppeared.Where(um => um.Id == userModel.Id).ToList();

                    if (check.Count == 0)
                    {
                        allUsersAppeared.Add(userModel);
                    }
                }
            }

            List<TestAssigned> testAssignedButNotAppeared =
                _assignedTestRepository.GetAll()
                    .Where(
                        u => u.TestId == queryParameters.TestId &&
                             !_testResultRepository.GetAll()
                                 .Any(taken => taken.TrainingTestId == u.TestId && taken.TestTakenByUserId == u.UserId))
                    .ToList();
            foreach (var result in testAssignedButNotAppeared)
            {
                var user = _userRepository.Find(result.UserId);
                if (user != null)
                {
                    var model = new UserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        ContactNumber = user.ContactNumber,
                        CompanyId = user.CompanyId,
                        EmailAddress = user.EmailAddress,
                        MobileNumber = user.MobileNumber,
                        LastName = user.LastName,
                        TestName = result.Test.TestTitle,
                        TestId = result.TestId
                    };
                    List<UserViewModel> check = allUsersNotAppeared.Where(um => um.Id == model.Id).ToList();

                    if (check.Count == 0)
                    {
                        allUsersNotAppeared.Add(model);
                    }
                }
            }

            viewModel.TestAppearedUsers = allUsersAppeared;
            viewModel.TestNotAppearedUsers = allUsersNotAppeared;

            return viewModel;
        }
    }
}