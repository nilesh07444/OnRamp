using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class AllTrainingTestQueryHandler : QueryHandlerBase<AllTrainingTestQueryParameter, List<TrainingTestViewModel>>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public AllTrainingTestQueryHandler(IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _trainingTestRepository = trainingTestRepository;
        }

        public override List<TrainingTestViewModel> ExecuteQuery(AllTrainingTestQueryParameter queryParameters)
        {
            var trainingTestViewModelList = new List<TrainingTestViewModel>();
            var guides = _trainingGuideRepository.List.Where(x => x.TestVersion.CurrentVersion != null);
            if (queryParameters.CurrentlyLoggedInUserId.HasValue)
                guides = guides.Where(g => g.Collaborators.Any(u => u.Id.Equals(queryParameters.CurrentlyLoggedInUserId)));
            foreach (var g in guides)
            {
                var test = g.TestVersion.LastPublishedVersion ?? g.TestVersion.CurrentVersion;
                var model = new TrainingTestViewModel
                {
                    ReferenceId = test.ReferenceId,
                    TrainingTestId = test.Id,
                    TestTitle = test.TestTitle,
                    CreateDate = test.CreateDate,
                    PassMarks = test.PassMarks,
                    TrainingGuideName = g.Title,
                    SelectedTrainingGuideId = test.TrainingGuideId,
                    ActiveStatus = test.ActiveStatus,
                    DraftStatus = test.DraftStatus,
                    ActivePublishDate = test.ActivePublishDate,
                    TestDuration = test.TestDuration,
                    DraftEditDate = test.DraftEditDate,
                    Version = test.Version ?? 0,
                    LastPublishedVersionId = g.TestVersion.LastPublishedVersion?.Id,
                    DraftVersionId = g.TestVersion.CurrentVersion?.Id != g.TestVersion.LastPublishedVersion?.Id ? g.TestVersion.CurrentVersion?.Id : new Guid?()
                };
                trainingTestViewModelList.Add(model);
            }
            return trainingTestViewModelList;
        }
    }
}