using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using System.Linq;

namespace Ramp.Services.CommandHandler.ProvisionalManagement
{
    public class DeleteProvisionalCompanyCommandHandler : CommandHandlerBase<DeleteProvisionalCompanyCommandParameter>
    {
        private readonly IRepository<Company> _companyRepository;

        public DeleteProvisionalCompanyCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CommandResponse Execute(DeleteProvisionalCompanyCommandParameter command)
        {
            var customerCompanies = _companyRepository.List.Where(c => c.ProvisionalAccountLink == command.ProvisionalComapanyId).ToList();
            foreach (var customerCompany in customerCompanies)
            {
                _companyRepository.Delete(customerCompany);
            }
            var company = _companyRepository.Find(command.ProvisionalComapanyId);
            _companyRepository.Delete(company);
            _companyRepository.SaveChanges();
            return null;
        }
    }
}