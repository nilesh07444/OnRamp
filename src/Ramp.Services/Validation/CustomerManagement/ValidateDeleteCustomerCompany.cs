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
    public class ValidateDeleteCustomerCompany : IValidator<DeleteCustomerCompanyCommand>
    {
        private readonly IRepository<Company> _companyRepository;

        public ValidateDeleteCustomerCompany(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteCustomerCompanyCommand argument)
        {
            var result = new List<ValidationResult>();
            var error = !argument.CustomerCompanyId.HasValue || _companyRepository.Find(argument.CustomerCompanyId.Value) == null;

            if (error)
                result.Add(new ValidationResult("Error", "Company was not found"));
            return result;
        }
    }
}