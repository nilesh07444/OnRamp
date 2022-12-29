using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.TestManagement
{
    public class UpdateTrainingTestCommandValidator : IValidator<UpdateTrainingTestCommand>
    {
        private readonly IRepository<TrainingTest> _testRepository;
        public UpdateTrainingTestCommandValidator(IRepository<TrainingTest> testRepository)
        {
            _testRepository = testRepository;
        }
        public IEnumerable<IValidationResult> Validate(UpdateTrainingTestCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_testRepository.Find(argument.model.TrainingTestId) == null)
                result.Add(new ValidationResult("Error", "Test not found"));
            return result;
        }
    }
}
