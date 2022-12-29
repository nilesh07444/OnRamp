using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class CheckForTrainingTestTitleExistQueryHandler : QueryHandlerBase<CheckForTrainingTestTitleExistQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public CheckForTrainingTestTitleExistQueryHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(CheckForTrainingTestTitleExistQueryParameter queryParameters)
        {
            return (new RemoteValidationResponseViewModel
            {
                Response = _trainingTestRepository.GetAll().Where(u => u.TestTitle != null && (!u.Deleted.HasValue || (u.Deleted.HasValue && !u.Deleted.Value)))
                .Any(u => u.TestTitle.Equals(queryParameters.TestName))
            });
        }
    }
}