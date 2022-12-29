using System;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class CompanyNameExistQueryHandler :
        QueryHandlerBase<CompanyNameExistQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public CompanyNameExistQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(CompanyNameExistQueryParameter queryParameters)
        {
            return (new RemoteValidationResponseViewModel
            {
                Response = _companyRepository.List.Any(u => u.CompanyName.Equals(queryParameters.CompanyName))
            });
        }
    }
}