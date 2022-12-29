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
    public class GetAllTrainingGuideUsageThresholdReportHandler : QueryHandlerBase<GetAllTrainingGuideUsageReportThresholdParameter, TrainingGuideusageStatsViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Bundle> _bundleRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public GetAllTrainingGuideUsageThresholdReportHandler(IRepository<Company> companyRepository, 
                                                              IRepository<Package> packageRepository,
                                                              IRepository<Bundle> bundleRepository,
                                                              IRepository<TrainingGuide> trainingGuideRepository)
        {
            _companyRepository = companyRepository;
            _packageRepository = packageRepository;
            _bundleRepository = bundleRepository;
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override TrainingGuideusageStatsViewModel ExecuteQuery(GetAllTrainingGuideUsageReportThresholdParameter queryParameters)
        {
            var TraigingGuideUsageStatsModel = new TrainingGuideusageStatsViewModel();

            var company = _companyRepository.Find(queryParameters.CompanyId);

            //Guid packageId = (Guid)company.PackageId;

            //var package = _packageRepository.Find(packageId);

            var bundle = _bundleRepository.Find(company.BundleId);

            //int allowGuide = package.MaxNumberOfGuides;
            int allowGuide = bundle.MaxNumberOfDocuments;

            var AllTrainingGuide = _trainingGuideRepository.List;
            int createdGuide = AllTrainingGuide.Count();

            TraigingGuideUsageStatsModel.TrainingGuidUsageList.Add(new TrainingGuideusageStatsViewModelShort
            {
                CompanyName = company.CompanyName,
                AllowGuide = allowGuide,
                CreatedGuide = createdGuide
            });

            return TraigingGuideUsageStatsModel;
        }
    }
}