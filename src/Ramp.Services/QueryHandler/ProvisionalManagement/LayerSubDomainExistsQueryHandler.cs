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
    public class LayerSubDomainExistsQueryHandler : QueryHandlerBase<LayerSubDomainExistsQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public LayerSubDomainExistsQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(LayerSubDomainExistsQueryParameter queryParameters)
        {
            return (new RemoteValidationResponseViewModel
            {
                Response = _companyRepository.List.Any(u => u.LayerSubDomain != null && u.LayerSubDomain.Equals(queryParameters.LayerSubDomainName))
            });
        }
    }
}