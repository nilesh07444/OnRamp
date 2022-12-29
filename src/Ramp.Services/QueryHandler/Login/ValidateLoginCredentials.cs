using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Login;
using Ramp.Contracts.QueryParameter.Login;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projections;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.Login
{
    public class ValidateLoginCredentials : IValidator<LoginCommandParameter>
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

        public IEnumerable<IValidationResult> Validate(LoginCommandParameter argument)
        {
            Domain.Models.User user = null;
            StandardUser standardUser = null;

            user = _userRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(argument.Email));
            standardUser =
                _standardUserRepository.List.SingleOrDefault(u => u.EmailAddress.Equals(argument.Email));
            var errors = new List<ValidationResult>();
            if (user != null || standardUser != null)
            {
                var model = user != null ? Project.UserViewModelFrom(user) : Project.UserViewModelFrom(standardUser);
                var company = _companyRepository.Find(model.CompanyId);
                if (!argument.Password.Equals(_encryptionHelper.Decrypt(model.Password)))
                    errors.Add(new ValidationResult(LoginFailed, "Invalid username or password"));

                if (model.IsFromSelfSignUp && !model.IsActive)
                    errors.Add(new ValidationResult(LoginFailed, "Account has not been approved. Contact Administrator"));

                if (model.IsUserExpire)
                    errors.Add(new ValidationResult(LoginFailed,
                        " Your user account has expired. Please contact your Admininstrator"));
                if (!model.IsConfirmEmail)
                    errors.Add(new ValidationResult(LoginFailed, "Please Confirm Your Email"));
                if (!model.IsActive)
                    errors.Add(new ValidationResult(LoginFailed,
                        "Your account is not active. Please contact the Administrator"));

                if (company != null)
                {
                    if (company.IsLock)
                        errors.Add(new ValidationResult(LoginFailed,
                            company.CompanyName + " Is Locked Please contact to Reseller"));
                    if (!company.IsActive)
                        errors.Add(new ValidationResult(LoginFailed,
                            "Your companies account is not active. Please contact the Administrator"));
                }
            }
            else
            {
                errors.Add(new ValidationResult(LoginFailed, "Invalid username or password"));
            }
            return errors;
        }
    }
}