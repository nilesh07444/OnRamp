using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class UserCanTakeTestQueryHandler : IQueryHandler<UserCanTakeTrainingTestQuery, bool>
    {
        private readonly IQueryExecutor _executor;
        private readonly IRepository<TrainingTest> _testRepository;
        public UserCanTakeTestQueryHandler(IQueryExecutor executor,IRepository<TrainingTest> testRepository)
        {
            _executor = executor;
            _testRepository = testRepository;
        }
        public bool ExecuteQuery(UserCanTakeTrainingTestQuery query)
        {
            var test = _testRepository.Find(query.TestId);
            if (test == null)
                return false;
            if (!query.UserId.HasValue)
                return false;

            var hasAppeared = _executor.Execute<CheckUserHasAlreadyAppearedForTestQueryParameter, CheckUserHasAlreadyAppearedForTestViewModel>(new CheckUserHasAlreadyAppearedForTestQueryParameter
            {
                CurrentlyLoggedInUserId = query.UserId.Value,
                TrainingTestId = query.TestId
            });
            return hasAppeared.IsUserEligibleToTakeTest && (!test.TestExpiryDate.HasValue || (test.TestExpiryDate.HasValue && test.TestExpiryDate.Value >= DateTime.Today));
            //var checkUserHasAlreadyAppearedForTestQueryParameter =
            //   new CheckUserHasAlreadyAppearedForTestQueryParameter
            //   {
            //       CurrentlyLoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId(),
            //       TrainingTestId = trainingTestId
            //   };

            //CheckUserHasAlreadyAppearedForTestViewModel checkUserHasAlreadyAppearedForTestViewModel =
            //    ExecuteQuery<CheckUserHasAlreadyAppearedForTestQueryParameter,
            //        CheckUserHasAlreadyAppearedForTestViewModel>(checkUserHasAlreadyAppearedForTestQueryParameter);
        }
    }
}
