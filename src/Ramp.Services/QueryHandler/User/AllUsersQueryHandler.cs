using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Ramp.Services.Helpers;
using Common.Query;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using Domain.Enums;

namespace Ramp.Services.QueryHandler.User
{
    public class AllUsersQueryHandler : QueryHandlerBase<EmptyQueryParameter, List<UserViewModel>>
    {
        private readonly IRepository<Domain.Models.User> _userRepository;

        public AllUsersQueryHandler(IRepository<Domain.Models.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override List<UserViewModel> ExecuteQuery(EmptyQueryParameter queryParameters)
        {
            var userList = new List<UserViewModel>();
            var users = _userRepository.List.Where(u => u.EmailAddress != "admin@admin.com" 
                && u.Company.CompanyType != CompanyType.ProvisionalCompany).ToList();
            foreach (var user in users)
            {
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobileNumber = user.MobileNumber,
                    Status = user.IsActive,
                    SelectedGroupId = user.GroupId,
                    ContactNumber = user.ContactNumber,
                    ParentUserId = user.ParentUserId,
                };
                var roles = user.Roles.Select(role => UserRoleHelper.GetUserRole(role.RoleName)).ToList();

                /* var roles = user.Roles.Select(role => new RoleViewModel
                {
                    Description = role.Description, RoleId = role.Id, RoleName = role.RoleName
                }).ToList();*/

                userViewModel.UserRoles = roles;

                var companyViewModel = new CompanyViewModel
                {
                    Id = user.Company.Id,
                    CompanyName = user.Company.CompanyName,
                    WebsiteAddress = user.Company.WebsiteAddress,
                    PhysicalAddress = user.Company.PhysicalAddress,
                    PostalAddress = user.Company.PostalAddress,
                    TelephoneNumber = user.Company.TelephoneNumber,
                    CompanyConnectionString = user.Company.CompanyConnectionString
                };
                userViewModel.Company = companyViewModel;
                userList.Add(userViewModel);
            }
            return userList;
        }
    }
}