using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class CustomerSelfSignUpQueryHandler : QueryHandlerBase<CustomerSelfSignUpQueryParameter, CompanyUserViewModel>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;

        public CustomerSelfSignUpQueryHandler(IRepository<StandardUser> userRepository, IRepository<CustomerGroup> groupRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
        }

        public override CompanyUserViewModel ExecuteQuery(CustomerSelfSignUpQueryParameter queryParameters)
        {
            var customerCompanyUserViewModel = new CompanyUserViewModel();
            var users = _userRepository.List.Where(u => u.IsFromSelfSignUp).ToList();

            foreach (var user in users)
            {
                customerCompanyUserViewModel.UserList.Add(Project.UserViewModelFrom(user));
            }
            return customerCompanyUserViewModel;
        }
    }
}