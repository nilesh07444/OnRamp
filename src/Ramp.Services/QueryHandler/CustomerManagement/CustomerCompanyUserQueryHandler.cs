using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Command;
using Data.EF.Customer;
using Role = Ramp.Contracts.Security.Role;
using Ramp.Contracts.QueryParameter.User;
using Domain.Customer.Models.CustomRole;

namespace Ramp.Services.QueryHandler.CustomerManagement
{
    public class CustomerCompanyUserQueryHandler :
        QueryHandlerBase<CustomerCompanyUserQueryParameter, CompanyUserViewModel>
    {
        private readonly ITransientRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<RaceCodes> _raceCodeRepository;
        private readonly IRepository<Label> _labelRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IRepository<CustomUserRoles> _customUserRoleRepository;

        public CustomerCompanyUserQueryHandler(ITransientRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository, IRepository<Company> companyRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<RaceCodes> raceCodeRepository,
            IRepository<CustomUserRoles> customUserRoleRepository,
        IRepository<Label> labelRepository,
            ICommandDispatcher commandDispatcher)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _customerRoleRepository = customerRoleRepository;
            _raceCodeRepository = raceCodeRepository;
            _labelRepository = labelRepository;
            _commandDispatcher = commandDispatcher;
            _customUserRoleRepository = customUserRoleRepository;

        }


        public override CompanyUserViewModel ExecuteQuery(CustomerCompanyUserQueryParameter queryParameters)
        {
            _userRepository.SetCustomerCompany(queryParameters.CompanyId.ToString());
            var customerCompanyUserViewModel = new CompanyUserViewModel();

            var customerRoles = _customerRoleRepository.GetAll();
            var customerGroup = _groupRepository.GetAll();
            var departments = _userRepository.GetAll().Where(z => z.Department != null).Select(z => z.Department).Distinct();
            var tags = _labelRepository.GetAll();
            var filterCompanyCustomer = new List<FilterCompanyCustomer>();
            var roles = new FilterCompanyCustomer() { Type = "Role" };
            roles.FilterData = new List<FilterData>();
            foreach (var item in customerRoles)
            {
                var roleData = new FilterData() { Id = item.Id.ToString(), Name = item.Description };
                roles.FilterData.Add(roleData);
            }
            filterCompanyCustomer.Add(roles);

            var group = new FilterCompanyCustomer() { Type = "Group" };
            group.FilterData = new List<FilterData>();
            foreach (var item in customerGroup)
            {
                var groupData = new FilterData() { Id = item.Id.ToString(), Name = item.Title };
                group.FilterData.Add(groupData);
            }
            filterCompanyCustomer.Add(group);

            var department = new FilterCompanyCustomer() { Type = "Department" };
            department.FilterData = new List<FilterData>();
            foreach (var item in departments)
            {
                var departmentData = new FilterData() { Id = item, Name = item };
                department.FilterData.Add(departmentData);
            }
            filterCompanyCustomer.Add(department);

            var status = new FilterCompanyCustomer() { Type = "Status" };
            status.FilterData = new List<FilterData>();

            var statusData = new FilterData() { Id = "Enabled", Name = "Enabled" };
            status.FilterData.Add(statusData);
            status.FilterData.Add(new FilterData() { Id = "Disabled", Name = "Disabled" });

            filterCompanyCustomer.Add(status);

            var signUpType = new FilterCompanyCustomer() { Type = "Sign Up Type" };
            signUpType.FilterData = new List<FilterData>();

            var signUpTypeData = new FilterData() { Id = "0", Name = "Other" };
            signUpType.FilterData.Add(signUpTypeData);
            signUpType.FilterData.Add(new FilterData() { Id = "1", Name = "Self-Signup" });

            filterCompanyCustomer.Add(signUpType);

            var label = new FilterCompanyCustomer() { Type = "Tags" };
            label.FilterData = new List<FilterData>();
            tags = tags.Where(x => !x.Deleted).ToList();
            foreach (var tag in tags)
            {
                var traininglabels = new FilterData() { Id = tag.Id.ToString(), Name = tag.Name };
                label.FilterData.Add(traininglabels);
            }
            filterCompanyCustomer.Add(label);

            customerCompanyUserViewModel.FilterCompanyCustomer = filterCompanyCustomer;
            var userModel = _userRepository.Find(queryParameters.UserId);
            if (userModel != null)
            {
                var userViewModel = new UserViewModel
                {
                    Id = userModel.Id,
                    EmailAddress = userModel.EmailAddress,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    FullName = $"{userModel.FirstName} {userModel.LastName}",
                    MobileNumber = userModel.MobileNumber,
                    Status = !userModel.IsUserExpire && userModel.IsActive,
                    Password = new EncryptionHelper().Decrypt(userModel.Password),
                    ConfirmPassword = new EncryptionHelper().Decrypt(userModel.Password),
                    ParentUserId = userModel.ParentUserId,
                    ExpireDays = userModel.ExpireDays,
                    IsUserExpire = userModel.IsUserExpire,
                    EmployeeNo = userModel.EmployeeNo,
                    IDNumber = userModel.IDNumber,
                    Gender = GenderEnum.GetDescription(userModel.Gender),
                    RaceCodeId = userModel.RaceCodeId,
                    IsFromSelfSignUp = userModel.IsFromSelfSignUp,
                    IsActive = userModel.IsActive,
                    TrainingLabels = userModel.TrainingLabels

                };

                if (userModel.Group != null)
                {
                    userViewModel.SelectedGroupId = userModel.Group.Id;
                }
                foreach (var role in userModel.Roles)
                {
                    var userRole = new RoleViewModel
                    {
                        Description = role.Description,
                        RoleId = role.Id,
                        RoleName = role.RoleName
                    };
                    userViewModel.SelectedCustomerUserRole = role.Id;
                    userViewModel.Roles.Add(userRole);
                }

                var company = _companyRepository.Find(queryParameters.CompanyId);
                var companyViewModel = new CompanyViewModel
                {
                    CompanyName = company.CompanyName,
                    WebsiteAddress = company.WebsiteAddress,
                    PhysicalAddress = company.PhysicalAddress,
                    PostalAddress = company.PostalAddress,
                    TelephoneNumber = company.TelephoneNumber
                };

                userViewModel.Company = companyViewModel;
                customerCompanyUserViewModel.UserViewModel = userViewModel;
            }
            var userlist = _userRepository.List.ToList();
            foreach (var user in userlist)
            {
                var isGlobalAdmin = user.Roles.FirstOrDefault(r => r.RoleName == "CustomerAdmin") != null;
                var isManagingAdmin = !isGlobalAdmin &&
                                      user.Roles.Any(r =>
                                          r.RoleName.Contains("Admin") || r.RoleName.Contains("Reporter"));
                var rolesData = user.Roles.FirstOrDefault();
                if (rolesData != null)
                {
                    var userViewModel = new UserViewModel
                    {
                        Id = user.Id,
                        EmailAddress = user.EmailAddress,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}",
                        MobileNumber = user.MobileNumber,
                        Status = !user.IsUserExpire && user.IsActive,
                        Password = new EncryptionHelper().Decrypt(user.Password),
                        ConfirmPassword = new EncryptionHelper().Decrypt(user.Password),
                        ParentUserId = user.ParentUserId,
                        RoleName = isGlobalAdmin ? "Global Admin" : isManagingAdmin ? "Managing Admin" : "Standard User",
                        ExpireDays = user.ExpireDays,
                        IsUserExpire = user.IsUserExpire,
                        EmployeeNo = user.EmployeeNo,
                        Department=user.Department,
                        IDNumber = user.IDNumber,
                        IsFromSelfSignUp = user.IsFromSelfSignUp,
                        IsActive = user.IsActive,
                        Gender = GenderEnum.GetDescription(user.Gender),
                        TrainingLabels = user.TrainingLabels
                    };
                    if (user.Group != null)
                    {
                        userViewModel.SelectedGroupId = user.Group.Id;
                    }
                    foreach (var role in user.Roles)
                    {
                        var userRole = new RoleViewModel
                        {
                            Description = role.Description,
                            RoleId = role.Id,
                            RoleName = role.RoleName
                        };
                        userViewModel.SelectedCustomerUserRole = role.Id;
                        userViewModel.Roles.Add(userRole);
                    }
                    if (user.Group != null)
                        userViewModel.GroupName = user.Group.Title;
                    //neeraj
                    if (user.CustomUserRoleId != null)
                    {
                        var cus = _customUserRoleRepository.List.Where(x => x.Id == user.CustomUserRoleId).FirstOrDefault();
                        userViewModel.RoleName = cus.Title;

                    }
                    customerCompanyUserViewModel.UserList.Add(userViewModel);
                }

            }

            customerCompanyUserViewModel.CompanyId = queryParameters.CompanyId;
            customerCompanyUserViewModel.CompanyName = queryParameters.CompanyName;

            var company2 = _companyRepository.Find(queryParameters.CompanyId);

            var selfSignUpViewModel = new SelfSignUpViewModel
            {
                IsForSelfSignUp = company2.IsForSelfSignUp,
                IsSelfSignUpApprove = company2.IsSelfSignUpApprove
            };

            customerCompanyUserViewModel.SelfSignUpViewModel = selfSignUpViewModel;

            _commandDispatcher.Dispatch(new UpdateConnectionStringCommand());

            return customerCompanyUserViewModel;
        }
    }
}