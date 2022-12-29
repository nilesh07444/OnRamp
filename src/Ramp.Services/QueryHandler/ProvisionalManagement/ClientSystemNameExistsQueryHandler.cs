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
    public class ClientSystemNameExistsQueryHandler : QueryHandlerBase<ClientSystemNameExistsQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<Company> _companyRepository;

        public ClientSystemNameExistsQueryHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(ClientSystemNameExistsQueryParameter queryParameters)
        {
            return (new RemoteValidationResponseViewModel
            {
                Response = _companyRepository.List.Any(u => u.ClientSystemName != null && u.ClientSystemName.Equals(queryParameters.ClientSystemName))
            });
        }
    }
}