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
    public class AllProvisionalCompanyQueryHandler :
        QueryHandlerBase<AllProvisionalUserQueryParameter, CompanyViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public AllProvisionalCompanyQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override CompanyViewModel ExecuteQuery(AllProvisionalUserQueryParameter queryParameters)
        {
            var companies = _companyRepository.List
                                            .AsQueryable()                            
                                            .Where(c => c.CompanyType == CompanyType.ProvisionalCompany && 
                                                        c.PhysicalAddress != "Dummy" && 
                                                        c.IsActive)
                                            .ToList();

            var companyModel = new CompanyViewModel();
            foreach (var company in companies)
            {
                companyModel.CompanyList.Add(new CompanyModelShort{
                    Id = company.Id,
                    Name = company.CompanyName
                });
            }
            return companyModel;
        }
    }
}