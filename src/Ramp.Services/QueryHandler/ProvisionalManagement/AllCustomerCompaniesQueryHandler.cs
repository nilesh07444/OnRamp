using Common.Data;
using Common.Query;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class AllCustomerCompaniesQueryHandler : IQueryHandler<AllCustomerCompanyQueryParameter, CompanyViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public AllCustomerCompaniesQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public CompanyViewModel ExecuteQuery(AllCustomerCompanyQueryParameter query)
        {
            var cs = _companyRepository.List
                                        .AsQueryable()
                                        .Where(c => c.IsActive && 
                                                    !c.PhysicalAddress.Equals("Dummy") && 
                                                    c.CompanyType == CompanyType.CustomerCompany)
                                        .ToList();
            if (cs.Count() == 0)
            {
                cs = _companyRepository.List
                                        .AsQueryable()
                                        .Where(c => c.CompanyType == CompanyType.ProvisionalCompany)
                                        .ToList();
            }

            return new CompanyViewModel
            {
                CompanyList = cs.OrderBy(n => n.CompanyName)
                    .Select(c => new CompanyModelShort() { Id = c.Id, Name = c.CompanyName }).ToList()
            };
        }
    }
}