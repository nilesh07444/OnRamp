using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System.Collections.Generic;
using System.Linq;
using Role = Ramp.Contracts.Security.Role;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class AllStandardUserQueryHandler :
        QueryHandlerBase<AllStandardUserQueryParameter, List<UserViewModel>>
    {
        private readonly IRepository<StandardUser> _userRepository;

        public AllStandardUserQueryHandler(IRepository<StandardUser> userRepository)
        {
            _userRepository = userRepository;
        }

        public override List<UserViewModel> ExecuteQuery(AllStandardUserQueryParameter queryParameters)
        {
            var allCustomerStandardUsers = _userRepository.List.Where(u => u.Roles.Any(r => r.RoleName.Equals(Role.StandardUser))).OrderBy(c => c.FirstName).ToList();

            var customerStandardUsers = new List<UserViewModel>();
            allCustomerStandardUsers.ForEach(c => customerStandardUsers.Add(Project.UserViewModelFrom(c)));
            return customerStandardUsers;
        }
    }
}