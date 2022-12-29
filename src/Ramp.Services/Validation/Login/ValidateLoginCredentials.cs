using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Login;
using Ramp.Services.Helpers;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using Ramp.Services.Projection;


namespace Ramp.Services.Validation.Login
{
    public class ValidateLoginCredentials : IValidator<LoginUserCommandParameter>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;
        private readonly EncryptionHelper _encryptionHelper;
        private const string LoginFailed = "LOGIN_FAILED";

        public ValidateLoginCredentials(IRepository<Domain.Models.User> userRepository,
            IRepository<StandardUser> standardUserRepository, IRepository<Company> companyRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
            _companyRepository = companyRepository;
            _encryptionHelper = new Helpers.EncryptionHelper();
        }

        public IEnumerable<IValidationResult> Validate(LoginUserCommandParameter argument)
        {
            Domain.Models.User user = null;
            StandardUser standardUser = null;
            var su = false;
            user = _userRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(argument.Email.TrimAllCastToLowerInvariant()));
            standardUser = _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(argument.Email.TrimAllCastToLowerInvariant()));
            if (user != null && user.ParentUserId.Equals(Guid.Empty))
            {
                su = true;
            }
            var errors = new List<ValidationResult>();
            if (user == null && standardUser == null)
            {
                errors.Add(new ValidationResult(LoginFailed, "Invalid username or password."));
            }
            else if (user != null || standardUser != null)
            {
                var model = standardUser != null ? Project.UserViewModelFrom(standardUser) : Project.UserViewModelFrom(user);
                var company = _companyRepository.Find(model.CompanyId);
                if (!argument.Password.Equals(_encryptionHelper.Decrypt(model.Password)))
                    errors.Add(new ValidationResult(LoginFailed, "Invalid username or password."));

                if (model.IsFromSelfSignUp && !model.IsActive)
                    errors.Add(new ValidationResult(LoginFailed, "Account has not been approved. Please contact your administrator."));

                if (model.IsUserExpire)
                    errors.Add(new ValidationResult(LoginFailed,
                        " Your user account has expired. Please contact your admininstrator."));
                if (!model.IsConfirmEmail)
                    errors.Add(new ValidationResult(LoginFailed, "Please confirm your email."));
                if (!model.IsActive)
                    errors.Add(new ValidationResult(LoginFailed,
                        "Your account is not active. Please contact the Administrator."));

                if (company != null)
                {
                    if (company.IsLock)
                        errors.Add(new ValidationResult(LoginFailed,
                            company.CompanyName + " Is Locked. Please contact to Reseller."));
                    if (!company.IsActive)
                        errors.Add(new ValidationResult(LoginFailed,
                            "Your companies account is not active. Please contact the Administrator."));
                    if (!su && argument.PortalContext.UserCompany.CompanyType == Domain.Enums.CompanyType.CustomerCompany)
                    {
                        if (company.Id != argument.PortalContext.UserCompany.ProvisionalAccountLink && standardUser == null)
                        {
                            errors.Add(new ValidationResult(LoginFailed, "Access prohibited, please contact the Administrator."));
                        }
                    }
                }
            }
            return errors;
        }
    }
}