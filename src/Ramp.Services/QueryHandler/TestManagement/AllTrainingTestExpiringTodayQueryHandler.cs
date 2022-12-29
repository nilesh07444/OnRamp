using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class AllTrainingTestExpiringTodayQueryHandler :
        QueryHandlerBase<AllTrainingTestExpiringTodayQueryParameter, List<TrainingTestViewModel>>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public AllTrainingTestExpiringTodayQueryHandler(IRepository<TrainingTest> trainingTestRepository,
            IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override List<TrainingTestViewModel> ExecuteQuery(
            AllTrainingTestExpiringTodayQueryParameter queryParameters)
        {
            var trainingTestViewModelList = new List<TrainingTestViewModel>();

            var result = _trainingTestRepository.GetAll().Where(a => a.ParentTrainingTestId == Guid.Empty && a.TestExpiryDate <= DateTime.Now && a.ExpiryNotificationSentOn == null);
            foreach (var trainingTest in result)
            {
                var trainingGuide = _trainingGuideRepository.Find(trainingTest.TrainingGuideId);
                var model = new TrainingTestViewModel
                {
                    ReferenceId = trainingTest.ReferenceId,
                    TrainingTestId = trainingTest.Id,
                    TestTitle = trainingTest.TestTitle,
                    CreateDate = trainingTest.CreateDate,
                    PassMarks = trainingTest.PassMarks,
                    TrainingGuideName = trainingGuide.Title,
                    SelectedTrainingGuideId = trainingTest.TrainingGuideId,
                    ActiveStatus = trainingTest.ActiveStatus,
                    DraftStatus = trainingTest.DraftStatus,
                    ActivePublishDate = trainingTest.ActivePublishDate,
                    TestDuration = trainingTest.TestDuration,
                    DraftEditDate = trainingTest.DraftEditDate,
                    CreatedBy = trainingTest.CreatedBy
                };
                trainingTestViewModelList.Add(model);
            }
            return trainingTestViewModelList;
        }
    }
}