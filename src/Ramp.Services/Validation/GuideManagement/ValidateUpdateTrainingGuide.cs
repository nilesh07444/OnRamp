using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.Validation.GuideManagement
{
    public class ValidateUpdateTrainingGuide : IValidator<UpdateTrainingGuideCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public ValidateUpdateTrainingGuide(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public IEnumerable<IValidationResult> Validate(UpdateTrainingGuideCommand argument)
        {
            var result = new List<ValidationResult>();
            if (_trainingGuideRepository.Find(argument.TrainingGuide.TrainingGuidId) == null)
            {
                result.Add(new ValidationResult() { MemberName = "Error", Message = "TrainingGuideDoesNotExist" });
            }
            return result;
        }
    }
}