using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.PackageManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.PackageManagement
{
    public class ValidateDeletePackage : IValidator<DeletePackageCommand>
    {
        private readonly IRepository<Package> _packageRepository;

        public ValidateDeletePackage(IRepository<Package> packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public IEnumerable<IValidationResult> Validate(DeletePackageCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_packageRepository.Find(argument.PackageId) == null)
                result.Add(new ValidationResult("Error", "Package could not be found"));
            return result;
        }
    }
}