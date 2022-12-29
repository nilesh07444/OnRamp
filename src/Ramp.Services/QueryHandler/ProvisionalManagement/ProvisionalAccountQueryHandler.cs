using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class ProvisionalAccountQueryHandler : QueryHandlerBase<ProvisionalAccountQueryParameter, CompanyViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public ProvisionalAccountQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CompanyViewModel ExecuteQuery(ProvisionalAccountQueryParameter queryParameters)
        {
            var fetchedCompanies = _companyRepository.List
                                                .AsQueryable()    
                                                .Where(a => a.PhysicalAddress != "Dummy" && 
                                                            a.CompanyType == CompanyType.ProvisionalCompany && 
                                                            a.IsActive)
                                                .ToList();

            var companies = new CompanyViewModel();
            foreach (Company company in fetchedCompanies)
            {
                companies.CompanyList.Add(new CompanyModelShort
                {
                    Id = company.Id,
                    Name = company.CompanyName
                });
            }
            return companies;
        }
    }
}