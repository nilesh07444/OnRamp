using Common.Data;
using Common.Query;
using Ramp.Contracts.QueryParameter.CustomerManagement;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class FindCompanyByIdQueryHandler : QueryHandlerBase<FindCompanyQueryParameter, Domain.Models.Company>
    {
        private readonly IRepository<Domain.Models.Company> _companyRepository;

        public FindCompanyByIdQueryHandler(IRepository<Domain.Models.Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override Domain.Models.Company ExecuteQuery(FindCompanyQueryParameter queryParameters)
        {
            var company = _companyRepository.Find(queryParameters.Id);
            return company;
        }
    }
}
