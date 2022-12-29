using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.Portal;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class EmailExistQueryHandler :
        QueryHandlerBase<EmailExistQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IQueryExecutor _executor;
        private readonly ICommandDispatcher _dispacther;
        public EmailExistQueryHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<Domain.Models.User> userRepository,
            IRepository<Company> companyRepository,
            IQueryExecutor executor,
            ICommandDispatcher dispacther)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _standardUserRepository = standardUserRepository;
            _executor = executor;
            _dispacther = dispacther;
        }

        public override RemoteValidationResponseViewModel ExecuteQuery(EmailExistQueryParameter queryParameters)
        {
            var result = new RemoteValidationResponseViewModel();
            if (queryParameters.PortContext.UserCompany.ProvisionalAccountLink.Equals(Guid.Empty))
            {
                var allCustomerCompanies = _companyRepository.List.AsQueryable().Where(x => x.ProvisionalAccountLink.Equals(
                   queryParameters.PortContext.UserCompany.Id)).ToList();
                var allEmails = new List<string>();
                var allAdminUsers = _userRepository.List.AsQueryable().Select(x => x.EmailAddress).ToList();
                allEmails.AddRange(allAdminUsers);
                if (!allEmails.Any(x => x.Equals(queryParameters.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    foreach (var c in allCustomerCompanies)
                    {
                        _dispacther.Dispatch(new OverridePortalContextCommand { CompanyId = c.Id });
                        var allUsers = _executor.Execute<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(new CustomerCompanyUserQueryParameter
                        {
                            CompanyId = c.Id
                        });
                        if (allUsers.UserList.Any())
                            allUsers.UserList.Select(x => x.EmailAddress).ToList().ForEach(delegate (string email)
                            {
                                if (!allEmails.Contains(email))
                                    allEmails.Add(email);
                            });
                    }
                }
                if (allEmails.Any(u => u.Equals(queryParameters.Email,StringComparison.InvariantCultureIgnoreCase)))
                    result.Response = true;
                _dispacther.Dispatch(new OverridePortalContextCommand { CompanyId = queryParameters.PortContext.UserCompany.Id });

            }
            else {
                var emails = new List<string>();
                var globaladminUsers = _userRepository.List.AsQueryable().Where(x => x.Roles.Any(r => r.RoleName == Ramp.Contracts.Security.Role.Admin)).Select(x => x.EmailAddress).ToList();
                var resellersEmails = _userRepository.List.AsQueryable().Where(u => u.CompanyId.Equals(queryParameters.PortContext.UserCompany.ProvisionalAccountLink)).Select(x => x.EmailAddress).ToList();
                var companyUsers = _standardUserRepository.List.AsQueryable().Select(u => u.EmailAddress).ToList();
                emails.AddRange(globaladminUsers);
                emails.AddRange(resellersEmails);
                emails.AddRange(companyUsers);
                if (emails.Any(u => u.Equals(queryParameters.Email,StringComparison.InvariantCultureIgnoreCase)))
                    result.Response = true;
            }
            return result;
        }
        
    }
}