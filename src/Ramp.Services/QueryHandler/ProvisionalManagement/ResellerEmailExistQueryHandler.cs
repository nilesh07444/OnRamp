using Common.Data;
using Common.Query;
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
    public class ResellerEmailExistQueryHandler : IQueryHandler<ResellerEmailExistQueryParameter, RemoteValidationResponseViewModel>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public ResellerEmailExistQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public RemoteValidationResponseViewModel ExecuteQuery(ResellerEmailExistQueryParameter query)
        {
            var result = new RemoteValidationResponseViewModel();
            if (_userRepository.List.Any(u => u.EmailAddress.Equals(query.Email)))
                result.Response = true;
            return result;
        }
    }
}