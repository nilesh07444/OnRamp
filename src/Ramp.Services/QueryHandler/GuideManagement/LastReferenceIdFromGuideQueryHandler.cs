using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.QueryParameter.GuideManagement;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class LastReferenceIdFromGuideQueryHandler : QueryHandlerBase<LastReferenceIdFromGuideQueryParameter, string>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public LastReferenceIdFromGuideQueryHandler(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override string ExecuteQuery(LastReferenceIdFromGuideQueryParameter queryParameters)
        {
            string lastReferenceId = "";

            TrainingGuide guide = _trainingGuideRepository.GetAll().OrderByDescending(g => g.ReferenceId).FirstOrDefault();
            if (guide != null)
            {
                lastReferenceId = guide.ReferenceId;
            }
            return lastReferenceId;
        }
    }
}