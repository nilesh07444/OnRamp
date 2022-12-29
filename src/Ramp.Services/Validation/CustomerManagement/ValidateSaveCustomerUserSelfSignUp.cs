using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.CustomerManagement
{
    public class ValidateSaveCustomerUserSelfSignUp : IValidator<SaveCustomerUserSelfSignUpCommandParameter>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<CustomerGroup> _customerGroupRepository;

        public ValidateSaveCustomerUserSelfSignUp(IRepository<StandardUser> standardUserRepository,
            IRepository<CustomerGroup> customerGroupRepository)
        {
            _standardUserRepository = standardUserRepository;
            _customerGroupRepository = customerGroupRepository;
        }

        public IEnumerable<IValidationResult> Validate(SaveCustomerUserSelfSignUpCommandParameter argument)
        {
            var result = new List<ValidationResult>();

            if (_standardUserRepository.List.Any(u => u.EmailAddress.Equals(argument.CustomerSelfSignUpViewModel.EmailAddress.ToLowerInvariant().Trim())))
                result.Add(new ValidationResult("Error", "Email already Exist"));
            if (_customerGroupRepository.List.FirstOrDefault(g => g.IsforSelfSignUpGroup) == null)
                result.Add(new ValidationResult("Error", "Default sign up group is required"));
            if (argument.CompanyViewModel.DefaultUserExpireDays == 0)
                result.Add(new ValidationResult("Error", "User Expiry setting required"));

            return result;
        }
    }
}