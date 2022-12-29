using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.ProvisionalManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.ProvisionalManagement
{
    public class ValidateDeleteProvisionalCompanyUser : IValidator<DeleteProvisionalCompanyUserCommandParameter>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public ValidateDeleteProvisionalCompanyUser(IRepository<User> userRepository, IRepository<StandardUser> standardUserRepository)
        {
            _userRepository = userRepository;
            _standardUserRepository = standardUserRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeleteProvisionalCompanyUserCommandParameter argument)
        {
            var result = new List<ValidationResult>();
            var foundUser = _userRepository.Find(argument.ProvisionalComapanyUserId) != null || _standardUserRepository.Find(argument.ProvisionalComapanyUserId) != null;

            if (!foundUser)
                result.Add(new ValidationResult("Error", "User not found"));
            return result;
        }
    }
}