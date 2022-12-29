using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class AllAssignedGuidesToStandardUserQueryHandler :
        QueryHandlerBase<AllAssignedGuidesToStandardUserQueryParameter, List<TrainingGuideViewModel>>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;

        public AllAssignedGuidesToStandardUserQueryHandler(IRepository<StandardUser> userRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
            IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
        {
            _userRepository = userRepository;
            _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
        }

        public override List<TrainingGuideViewModel> ExecuteQuery(AllAssignedGuidesToStandardUserQueryParameter queryParameters)
        {
            var assignedTrainingGuideList = _assignedTrainingGuidesRepository.List
                .Where(tt => tt.UserId == queryParameters.UserId).Distinct()
                .ToList();
            var userStatsHistory = _trainingGuideUsageStatsRepository.List.Where(s => s.UserId.Equals(queryParameters.UserId));

            var trainingGuideViewModelList = new List<TrainingGuideViewModel>();
            foreach (AssignedTrainingGuides assignedGuide in assignedTrainingGuideList)
            {
                var model = new TrainingGuideViewModel();
                model.TrainingGuidId = assignedGuide.TrainingGuide.Id;
                model.Title = assignedGuide.TrainingGuide.Title;
                model.IsActive = assignedGuide.TrainingGuide.IsActive;
                model.Description = assignedGuide.TrainingGuide.Description;
                model.ReferenceId = assignedGuide.TrainingGuide.ReferenceId;
                model.CreatedOn = assignedGuide.TrainingGuide.CreatedOn;
                model.Printable = assignedGuide.TrainingGuide.Printable;
                if (assignedGuide.AssignedDate.HasValue)
                    model.DateAssigned = assignedGuide.AssignedDate.Value;
                if (userStatsHistory != null)
                {
                    DateTime dateLastViewed = default(DateTime);
                    var userGuideStats = userStatsHistory.Where(s => s.TrainingGuidId.Equals(assignedGuide.TrainingGuideId) && !s.Unassigned).ToList();
                    if (userGuideStats != null)
                    {
                        if (userGuideStats.Count > 0)
                        {
                            dateLastViewed = userGuideStats[0].ViewDate;
                            if (userGuideStats.Count > 1)
                            {
                                for (int i = 1; i < userGuideStats.Count; i++)
                                {
                                    if (userGuideStats[i].ViewDate > userGuideStats[i - 1].ViewDate)
                                    {
                                        dateLastViewed = userGuideStats[i].ViewDate;
                                    }
                                }
                            }
                        }
                    }
                    if (dateLastViewed != default(DateTime))
                        model.DateLastViewed = dateLastViewed;
                }
                trainingGuideViewModelList.Add(model);
            }
            return trainingGuideViewModelList;
        }
    }
}