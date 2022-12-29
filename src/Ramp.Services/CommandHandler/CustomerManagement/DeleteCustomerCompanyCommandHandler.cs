using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System.Linq;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class DeleteCustomerCompanyCommandHandler : CommandHandlerBase<DeleteCustomerCompanyCommand>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserLoginStats> _userLoginStatsRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<UserCorrespondenceLog> _userCorrespondenceLogRepository;
        private readonly IRepository<UserActivityLog> _userActivityLogRepository;
        private readonly IRepository<CustomerSurveyDetail> _customerSurveyDetailRepository;
        private readonly IRepository<CustomerConfiguration> _customerConfigurationRepository;
        private readonly IRepository<CustomColour> _customColorRepository;

        public DeleteCustomerCompanyCommandHandler(IRepository<Company> companyRepository, IRepository<User> userRepository,
            IRepository<UserLoginStats> userLoginStatsRepository,
            IRepository<CustomerGroup> groupRepository,
            IRepository<UserCorrespondenceLog> userCorrespondenceLogRepository,
            IRepository<UserActivityLog> userActivityLogRepository,
            IRepository<CustomerSurveyDetail> customerSurveyDetailRepository,
            IRepository<CustomColour> customColorRepository,
            IRepository<CustomerConfiguration> customerConfigurationRepository
            )
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _customerSurveyDetailRepository = customerSurveyDetailRepository;
            _userActivityLogRepository = userActivityLogRepository;
            _userCorrespondenceLogRepository = userCorrespondenceLogRepository;
            _groupRepository = groupRepository;
            _userLoginStatsRepository = userLoginStatsRepository;
            _customColorRepository = customColorRepository;
            _customerConfigurationRepository = customerConfigurationRepository;
        }

        public override CommandResponse Execute(DeleteCustomerCompanyCommand command)
        {
            var company = _companyRepository.Find(command.CustomerCompanyId);
            if (company.CustomColours != null) {
                var cc = company.CustomColours;
                company.CustomColours = null;
                _companyRepository.SaveChanges();
                _customColorRepository.Delete(cc);
                _customColorRepository.SaveChanges();
            }
            company.CustomerConfigurations.ToList().ForEach(x => _customerConfigurationRepository.Delete(x));
            _companyRepository.SaveChanges();

            _companyRepository.Delete(company);
            _companyRepository.SaveChanges();
            return null;
        }
    }
}