using System;
using System.Linq;
using Common.Data;
using Common.Query;
using Domain.Enums;
using Domain.Models;
using Ramp.Services.Helpers;
using Ramp.Contracts.QueryParameter.Login;
using Ramp.Contracts;
using CompanyType = Domain.Enums.CompanyType;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.QueryHandler.Login
{
    public class ValidateUserQueryHandler : QueryHandlerBase<ValidateUserQueryParameters, ValidateUserViewModel>
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;

        public ValidateUserQueryHandler(IRepository<Company> companyRepository, IRepository<Domain.Models.User> userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public override ValidateUserViewModel ExecuteQuery(ValidateUserQueryParameters queryParameters)
        {
             var encryptionHelper = new EncryptionHelper();
            queryParameters.Email = queryParameters.Email.Trim();
             var user = _userRepository.List.SingleOrDefault(
                    a => a.EmailAddress == queryParameters.Email && encryptionHelper.Decrypt(a.Password) == queryParameters.Password);

            var validateUserViewModel = new ValidateUserViewModel();

            if (user != null)
            {
                var userProvisionalCompany = new Company();
                var companyModel = new CompanyViewModel();
                var company = _companyRepository.List.First(c => c.Id == user.CompanyId);
                if (company != null)
                {
                    companyModel.CompanyName = company.CompanyName;
                    companyModel.CompanyType = (CompanyType) company.CompanyType;
                    companyModel.LayerSubDomain = company.LayerSubDomain;
                    companyModel.PhysicalAddress = company.PhysicalAddress;
                    companyModel.Status = company.IsActive;
                    companyModel.Id = company.Id;
                    companyModel.LogoImageUrl = company.LogoImageUrl;
                    companyModel.ProvisionalAccountLink = company.ProvisionalAccountLink;
                    userProvisionalCompany = _companyRepository.Find(company.ProvisionalAccountLink);
                    if (userProvisionalCompany != null)
                    {
                        companyModel.ProvisionalAccountName = userProvisionalCompany.CompanyName;
                    }
                }

                if (company != null && user.ParentUserId != Guid.Empty) // Logged in User is either Customer or Reseller
                {
                    if (company.ProvisionalAccountLink != Guid.Empty) // Logged in User is Customer
                    {
                        if (userProvisionalCompany != null)
                            validateUserViewModel.IsActive = company.IsActive && user.IsActive &&
                                                             userProvisionalCompany.IsActive;
                    }
                    else
                    {
                        validateUserViewModel.IsActive = company.IsActive && user.IsActive;
                    }
                    validateUserViewModel.ClientSystemName = company.ClientSystemName;
                    validateUserViewModel.CustomerUserConnectionString = company.CompanyConnectionString;

                }
                validateUserViewModel.UserId = user.Id;
                validateUserViewModel.UserName = user.FirstName;
                validateUserViewModel.Email = user.EmailAddress;
                validateUserViewModel.Password = user.Password;
                validateUserViewModel.UserCompany = companyModel;
               
                foreach (Role role in user.Roles)
                {
                    UserRole userRole = UserRoleHelper.GetUserRole(role.RoleName);
                    validateUserViewModel.UserRole.Add(userRole);
                }
            }
            return validateUserViewModel;
        }
    }
}