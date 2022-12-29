using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.QueryHandler.CorrespondenceManagement;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class TotalCreatedTrainingGuidesInSystemQueryHandler :
        IQueryHandler<TotalCreatedTrainingGuidesInSystemQuery, AllTrainingGuidesInSystemViewModel>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        public TotalCreatedTrainingGuidesInSystemQueryHandler(IRepository<TrainingGuide> trainingGuideRepository )
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public AllTrainingGuidesInSystemViewModel ExecuteQuery(TotalCreatedTrainingGuidesInSystemQuery query)
        {
            return new AllTrainingGuidesInSystemViewModel
            {
                Count = _trainingGuideRepository.List.Count()
            };
        }
    }
}
