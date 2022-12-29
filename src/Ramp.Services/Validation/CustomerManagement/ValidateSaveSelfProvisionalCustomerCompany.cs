using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.CustomerManagement
{
    public class ValidateSaveSelfProvisionalCustomerCompany : IValidator<SaveSelfProvisionalCustomerCompanyCommand>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Company> _comanyRepository;

        public ValidateSaveSelfProvisionalCustomerCompany(IRepository<User> userRepository,
            IRepository<Package> packageRepository,
            IRepository<Company> comanyRepository)
        {
            _userRepository = userRepository;
            _packageRepository = packageRepository;
            _comanyRepository = comanyRepository;
        }

        public IEnumerable<IValidationResult> Validate(SaveSelfProvisionalCustomerCompanyCommand argument)
        {
            var result = new List<ValidationResult>();

            if (!_comanyRepository.List.Any(c => c.IsForSelfProvision))
                result.Add(new ValidationResult("Error", "An appropriate Reseller was not found"));
            if (!_packageRepository.List.Any(p => p.IsForSelfProvision))
                result.Add(new ValidationResult("Error", "No package was found that allows self signup"));

            return result;
        }
    }
}