using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Linq;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class GetAllTrainingGuideUsageReportHandler : QueryHandlerBase<GetAllTrainingGuideUsageReportParameter, TrainingGuideusageStatsViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;
        private readonly IRepository<AssignedTrainingGuides> _assignedTrainingGuidesRepository;

        public GetAllTrainingGuideUsageReportHandler(IRepository<Company> companyRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<AssignedTrainingGuides> assignedTrainingGuidesRepository,
            IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
        {
            _companyRepository = companyRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _assignedTrainingGuidesRepository = assignedTrainingGuidesRepository;
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
        }

        public override TrainingGuideusageStatsViewModel ExecuteQuery(GetAllTrainingGuideUsageReportParameter queryParameters)
        {
            DateTime toDate = queryParameters.LastDate.Date;

            var traigingGuideUsageStatsModel = new TrainingGuideusageStatsViewModel();
            var company = _companyRepository.Find(queryParameters.CompanyId);

            var allTrainingGuide = _trainingGuideRepository.List;

            var allAssignedTrainingGuide = _assignedTrainingGuidesRepository.List.Where(tg => tg.UserId == queryParameters.LoggedInUserId).ToList();

            foreach (var guide in allTrainingGuide)
            {
                var guideName = guide.Title;
                var usageStats = _trainingGuideUsageStatsRepository.List;
                var viewCount = usageStats.Where(t => t.TrainingGuidId == guide.Id).ToList();
                int count = viewCount.Count();
                var fromToDate = viewCount.Where(d => d.ViewDate.Date <= toDate).ToList();
                int viewperCount = fromToDate.Count();
                double viewPerWeeks = (viewperCount);

                if (queryParameters.IsCustomerLayerDashboard == true)
                {
                    var totalAssignedCount = allAssignedTrainingGuide.Where(t => t.UserId == queryParameters.LoggedInUserId).ToList();
                    traigingGuideUsageStatsModel.TotalAssignedCount = totalAssignedCount.Count;

                    //var isAssignedCount = allAssignedTrainingGuide.Where(t => t.UserId == queryParameters.LoggedInUserId && t.TrainingGuideId == guide.Id).ToList();
                    var isAssignedCount = usageStats.Where(t => t.UserId == queryParameters.LoggedInUserId && t.TrainingGuidId == guide.Id).ToList();
                    if (isAssignedCount.Count > 0)
                    {
                        if (count > 0)
                        {
                            traigingGuideUsageStatsModel.TrainingGuidUsageList.Add(new TrainingGuideusageStatsViewModelShort
                            {
                                TraigingGuideName = guideName,
                                TotalView = count,
                                ViewPerWeek = Convert.ToDecimal(Math.Round(viewPerWeeks, 2))
                            });
                        }
                    }
                }
                else
                {
                    traigingGuideUsageStatsModel.TrainingGuidUsageList.Add(new TrainingGuideusageStatsViewModelShort
                    {
                        TraigingGuideName = guideName,
                        TotalView = count,
                        ViewPerWeek = Convert.ToDecimal(Math.Round(viewPerWeeks, 2))
                    });
                }
            }
            return traigingGuideUsageStatsModel;
        }
    }
}