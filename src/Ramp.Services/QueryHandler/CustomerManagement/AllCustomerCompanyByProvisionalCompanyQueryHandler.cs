using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using System.Linq;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class AllCustomerCompanyByProvisionalCompanyQueryHandler : QueryHandlerBase<AllCustomerCompanyByProvisionalCompanyParameter, CompanyViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public AllCustomerCompanyByProvisionalCompanyQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CompanyViewModel ExecuteQuery(AllCustomerCompanyByProvisionalCompanyParameter queryParameters)
        {
            var companies = _companyRepository.List.Where(c => c.ProvisionalAccountLink.Equals(queryParameters.CompanyId));
            companies = companies.OrderBy(c => c.CompanyName);
            var companyModel = new CompanyViewModel();

            foreach (var company in companies)
            {
                companyModel.CompanyList.Add(new CompanyModelShort
                {
                    Id = company.Id,
                    Name = company.CompanyName,
                    CompanyConnectionString = company.CompanyConnectionString
                });
            }

            return companyModel;
        }
    }
}