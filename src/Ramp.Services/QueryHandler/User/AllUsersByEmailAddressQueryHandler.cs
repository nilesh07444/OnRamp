using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Services.QueryHandler.User
{
    public class AllUsersByEmailAddressQueryHandler : QueryHandlerBase<UserSearchQueryParameter, UserViewModel>
    {
        private readonly IReadRepository<Domain.Models.User> _userRepository;
        private readonly ITransientReadRepository<StandardUser> _standardUserRepository;

        public AllUsersByEmailAddressQueryHandler(IReadRepository<Domain.Models.User> userRepository, ITransientReadRepository<StandardUser> standardUserRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public override UserViewModel ExecuteQuery(UserSearchQueryParameter queryParameters)
        {
            //var user = _userRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(queryParameters.Email));
            var standardUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(queryParameters.Email));

            //if (user != null)
            //{
            //    return Project.UserViewModelFrom(user);
            //}
            if (standardUser != null)
            {
                return Project.UserViewModelFrom(standardUser);
            }
            return null;
        }
    }
}