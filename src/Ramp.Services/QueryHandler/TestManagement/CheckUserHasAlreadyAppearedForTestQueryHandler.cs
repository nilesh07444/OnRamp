using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class CheckUserHasAlreadyAppearedForTestQueryHandler :
        QueryHandlerBase<CheckUserHasAlreadyAppearedForTestQueryParameter, CheckUserHasAlreadyAppearedForTestViewModel>
    {
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TrainingTest> _testRepository;
        public CheckUserHasAlreadyAppearedForTestQueryHandler(IRepository<TestResult> testResultRepository,IRepository<TrainingTest> testRepository)
        {
            _testResultRepository = testResultRepository;
            _testRepository = testRepository;
        }

        public override CheckUserHasAlreadyAppearedForTestViewModel ExecuteQuery(CheckUserHasAlreadyAppearedForTestQueryParameter queryParameters)
        {
            var model = new CheckUserHasAlreadyAppearedForTestViewModel();
            var tests = queryParameters.Tests.Any() ? queryParameters.Tests : _testRepository.List;
            var test = tests.AsQueryable().FirstOrDefault(x => x.Id == queryParameters.TrainingTestId);
            if (test == null)
                throw new System.Exception($"No test found with id : {queryParameters.TrainingTestId}");
            var maxRewrites = test.MaximumNumberOfRewites.HasValue ? test.MaximumNumberOfRewites.Value : Int32.MaxValue;
            var testResults = queryParameters.TestResults.Any() ? queryParameters.TestResults : _testResultRepository.List;
            var attempts = testResults.AsQueryable().Where(tt => tt.TrainingTestId == queryParameters.TrainingTestId
                                                                     &&
                                                                     tt.TestTakenByUserId ==
                                                                     queryParameters.CurrentlyLoggedInUserId && !tt.MaximumTestRewritesReached).ToList();
            var passed = attempts.Exists(tt => tt.TestResultStatus);
            if (passed)
            {
                model.IsUserEligibleToTakeTest = false;
                model.Message = $"Sorry, you have already taken this test";
            }
            else
            {
                if (attempts.Count() >= maxRewrites)
                {
                    model.IsUserEligibleToTakeTest = false;
                    model.Message = $"Sorry , you have reached the maximum rewrites for this test";
                }
                else
                {
                    model.IsUserEligibleToTakeTest = true;
                }
            }
            return model;
        }
    }
}