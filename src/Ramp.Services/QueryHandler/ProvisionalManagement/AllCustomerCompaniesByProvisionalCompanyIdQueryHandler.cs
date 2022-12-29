using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class AllCustomerCompaniesByProvisionalCompanyIdQueryHandler :
        IQueryHandler<AllCustomerCompanyQueryParameter, CompanyViewModelLong>
    {
        private readonly IRepository<Company> _companyRepository;

        public AllCustomerCompaniesByProvisionalCompanyIdQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public CompanyViewModelLong ExecuteQuery(AllCustomerCompanyQueryParameter queryParameters)
        {
            var viewModel = new CompanyViewModelLong();
            var companies = _companyRepository.List
                    .Where(company => company.ProvisionalAccountLink == queryParameters.ProvisionalCompanyId &&
                                      company.PhysicalAddress != "Dummy" && company.IsActive).OrderBy(c => c.CompanyName);
            foreach (Company company in companies)
            {
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
                viewModel.CompanyList.Add(companyViewModel);
            }
            return viewModel;
        }
    }
}