using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class ReassignUsersToProvisionalCompaniesQueryHandler :
        QueryHandlerBase<EmptyQueryParameter, ProvisionalCompanyListViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public ReassignUsersToProvisionalCompaniesQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override ProvisionalCompanyListViewModel ExecuteQuery(EmptyQueryParameter queryParameters)
        {
            var viewModel = new ProvisionalCompanyListViewModel();
            IEnumerable<Company> companies =
                _companyRepository.List
                    .ToList()
                    .Where(company => company.CompanyType == CompanyType.ProvisionalCompany &&
                                      company.PhysicalAddress != "Dummy" && company.IsActive);

            foreach (Company company in companies)
            {
                Company company1 = company;
                IEnumerable<Company> customerCompaniesUnderProvisionalCompany =
                    _companyRepository.List.Where(c => c.ProvisionalAccountLink == company1.Id &&
                                                           c.CompanyType == CompanyType.CustomerCompany && c.IsActive);
                var companyViewModel = new CompanyViewModel
                {
                    CompanyName = company.CompanyName,
                    Id = company.Id,
                    LayerSubDomain = company.LayerSubDomain,
                    ProvisionalAccountLink = company.ProvisionalAccountLink,
                    Status = company.IsActive,
                    PhysicalAddress = company.PhysicalAddress,
                    PostalAddress = company.PostalAddress,
                    TelephoneNumber = company.TelephoneNumber,
                    WebsiteAddress = company.WebsiteAddress
                };
                viewModel.FromProvisionalCompanyList.Add(companyViewModel);
                viewModel.ToProvisionalCompanyList.Add(companyViewModel);
            }
            return viewModel;
        }
    }
}