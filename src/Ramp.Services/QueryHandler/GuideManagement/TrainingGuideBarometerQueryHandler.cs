using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class TrainingGuideBarometerQueryHandler :
        QueryHandlerBase<TrainingGuideBarometerQueryParameter, TrainingGuideViewModelLong>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Bundle> _bundleRepository;

        public TrainingGuideBarometerQueryHandler(IRepository<StandardUser> userRepository, IRepository<Company> companyRepository, IRepository<Package> packageRepository, IRepository<Bundle> bundleRepository)
        {
            _companyRepository = companyRepository;
            _packageRepository = packageRepository;
            _bundleRepository = bundleRepository;
            _userRepository = userRepository;
        }

        public override TrainingGuideViewModelLong ExecuteQuery(TrainingGuideBarometerQueryParameter queryParameters)
        {
            var userviewmodel = new TrainingGuideViewModelLong();

            if (queryParameters.UserId != Guid.Empty)
            {
                var user = _userRepository.Find(queryParameters.UserId);
                if (user != null)
                {
                    Company userCompany = _companyRepository.Find(user.CompanyId);
                    //Package userCompanyPackage = _packageRepository.Find(userCompany.PackageId);
                    Bundle userCompanyBundle = _bundleRepository.Find(userCompany.BundleId);
                    userviewmodel.MaxGuide = userCompanyBundle.MaxNumberOfDocuments;
                    //userviewmodel.MaxChapterPerGuide = userCompanyPackage.MaxNumberOfChaptersPerGuide;
                }
            }
            return userviewmodel;
        }
    }
}