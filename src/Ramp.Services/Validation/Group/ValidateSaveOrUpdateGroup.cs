using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.Group
{
    public class ValidateSaveOrUpdateGroup : IValidator<SaveOrUpdateGroupCommand>
    {
        private readonly IRepository<CustomerGroup> _customerGroupRepository;

        public ValidateSaveOrUpdateGroup(IRepository<CustomerGroup> customerGroupRepository)
        {
            _customerGroupRepository = customerGroupRepository;
        }

        public IEnumerable<IValidationResult> Validate(SaveOrUpdateGroupCommand argument)
        {
            var result = new List<ValidationResult>();
            if (argument.AttemptCreate && _customerGroupRepository.List.Any(g => g.Title.Equals(argument.Title.Trim())))
                result.Add(new ValidationResult("Error", "Group exists"));

            return result;
        }
    }
}