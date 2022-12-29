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
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class AllAssignedGuidesToStandardUserWithCategoryQueryHandler :
        QueryHandlerBase<AllAssignedGuidesToStandardUserWithCategoryQueryParameter, List<TrainingGuideViewModel>>
    {
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuideRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;

        public AllAssignedGuidesToStandardUserWithCategoryQueryHandler(
            IRepository<AssignedTrainingGuides> assignedTrainingGuideRepository,
            IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
        {
            _assignedTrainingGuideRepository = assignedTrainingGuideRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
        }

        public override List<TrainingGuideViewModel> ExecuteQuery(AllAssignedGuidesToStandardUserWithCategoryQueryParameter queryParameters)
        {
            var UserStats = _trainingGuideUsageStatsRepository.List.Where(s => s.UserId.Equals(queryParameters.UserId));

            var trainingGuideViewModelList = new List<TrainingGuideViewModel>();

            if (queryParameters.CatId == Guid.Empty)
            {
                var assignedTrainingGuideList = _assignedTrainingGuideRepository.List
                    .Where(tt => tt.UserId == queryParameters.UserId).Distinct()
                    .ToList();

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
                    if (UserStats != null)
                    {
                        DateTime dateLastViewed = default(DateTime);
                        var guideStats = UserStats.Where(s => s.TrainingGuidId.Equals(assignedGuide.TrainingGuideId)).ToList();
                        if (guideStats != null)
                        {
                            if (guideStats.Count > 0)
                            {
                                dateLastViewed = guideStats[0].ViewDate;
                                if (guideStats.Count > 1)
                                {
                                    for (int i = 1; i < guideStats.Count; i++)
                                    {
                                        if (guideStats[i].ViewDate > guideStats[i - 1].ViewDate)
                                        {
                                            dateLastViewed = guideStats[i].ViewDate;
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
            }

            if (queryParameters.CatId != null && queryParameters.CatId != Guid.Empty)
            {
                //get list of playbook bu user Id

                var assignedTrainingGuideList = _assignedTrainingGuideRepository.List
                       .Where(tt => tt.UserId == queryParameters.UserId).Distinct()
                       .ToList();

                foreach (AssignedTrainingGuides assignedGuide in assignedTrainingGuideList)
                {
                    foreach (var cat in queryParameters.CatList)
                    {
                        if (assignedGuide.TrainingGuide.Categories.FirstOrDefault().Id == cat.Id)
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
                            if (UserStats != null)
                            {
                                DateTime dateLastViewed = default(DateTime);
                                var guideStats =
                                    UserStats.Where(s => s.TrainingGuidId.Equals(assignedGuide.TrainingGuideId))
                                        .ToList();
                                if (guideStats != null)
                                {
                                    if (guideStats.Count > 0)
                                    {
                                        dateLastViewed = guideStats[0].ViewDate;
                                        if (guideStats.Count > 1)
                                        {
                                            for (int i = 1; i < guideStats.Count; i++)
                                            {
                                                if (guideStats[i].ViewDate > guideStats[i - 1].ViewDate)
                                                {
                                                    dateLastViewed = guideStats[i].ViewDate;
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
                    }
                }
            }

            return trainingGuideViewModelList;
        }
    }
}