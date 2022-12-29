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
    public class ValidateDeleteGroup : IValidator<DeleteGroupCommand>
    {
        private readonly IRepository<CustomerGroup> _customerGroupRepository;

        public ValidateDeleteGroup(IRepository<CustomerGroup> customerGroupRepository)
        {
            _customerGroupRepository = customerGroupRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteGroupCommand argument)
        {
            var result = new List<ValidationResult>();

            if (_customerGroupRepository.Find(argument.GroupId) == null)
                result.Add(new ValidationResult("Error", "Group Was not found"));

            return result;
        }
    }
}