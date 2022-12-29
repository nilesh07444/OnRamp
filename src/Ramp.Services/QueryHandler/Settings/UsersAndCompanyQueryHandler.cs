using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using CompanyType = Domain.Enums.CompanyType;

namespace Ramp.Services.QueryHandler.Settings
{
    public class UsersAndCompanyQueryHandler :
        QueryHandlerBase<EmptyQueryParameter, ManageUserAndCompanyViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;

        public UsersAndCompanyQueryHandler(IRepository<Company> companyRepository, IRepository<Domain.Models.User> userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public override ManageUserAndCompanyViewModel ExecuteQuery(EmptyQueryParameter queryParameters)
        {
            var companyViewModel = new ManageUserAndCompanyViewModel();
            var companyList = _companyRepository.List
                    .Where(a => a.PhysicalAddress != "Dummy");

            foreach (Company company in companyList)
            {
                var companyModel = new CompanyViewModel
                {
                    Id = company.Id,
                    Status = company.IsActive,
                    CompanyName = company.CompanyName,
                    LayerSubDomain = company.LayerSubDomain,
                    PhysicalAddress = company.PhysicalAddress,
                    PostalAddress = company.PostalAddress,
                    ProvisionalAccountLink = company.ProvisionalAccountLink,
                    TelephoneNumber = company.TelephoneNumber,
                    WebsiteAddress = company.WebsiteAddress,
                    CompanyType = (CompanyType)company.CompanyType
                };

                companyViewModel.CompanyList.Add(companyModel);
            }

            var userList = _userRepository.List.Where(a => a.ParentUserId != Guid.Empty).ToList();
            foreach (Domain.Models.User user in userList)
            {
                var userModel = new UserViewModel
                {
                    ParentUserId = user.ParentUserId,
                    Id = user.Id,
                    FirstName = user.FirstName,
                    ContactNumber = user.ContactNumber,
                    CompanyId = user.CompanyId,
                    LastName = user.LastName,
                    EmailAddress = user.EmailAddress,
                    MobileNumber = user.MobileNumber,
                    Status = user.IsActive,
                    EmployeeNo = user.EmployeeNo
                };

                var roles = new List<RoleViewModel>();
                foreach (var role in user.Roles)
                {
                    roles.Add(new RoleViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.RoleName,
                        Description = role.Description,
                    });
                }
                userModel.Roles = roles;

                companyViewModel.UserList.Add(userModel);
            }

            return companyViewModel;
        }
    }
}