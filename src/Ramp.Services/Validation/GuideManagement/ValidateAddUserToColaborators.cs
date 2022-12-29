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
    public class ValidateAddUserToColaborators : IValidator<AddUserToColaboratorsCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;

        public ValidateAddUserToColaborators(IRepository<TrainingGuide> trainingGuideRepository, IRepository<StandardUser> standardUserRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _standardUserRepository = standardUserRepository;
        }

        public IEnumerable<IValidationResult> Validate(AddUserToColaboratorsCommand argument)
        {
            var result = new List<ValidationResult>();
            if (argument.UserViewModelList != null)
            {
                foreach (var userViewModel in argument.UserViewModelList)
                {
                    if (_standardUserRepository.Find(userViewModel.Id) == null)
                        result.Add(new ValidationResult("Error", "Colaborator not found"));
                }
            }
            if (argument.TrainingGuideViewModel.TrainingGuidId != Guid.Empty)
            {
                if (_trainingGuideRepository.Find(argument.TrainingGuideViewModel.TrainingGuidId) == null)
                    result.Add(new ValidationResult("Error", "Training Guide could not be found"));
            }
            if (!string.IsNullOrWhiteSpace(argument.TrainingGuideViewModel.ReferenceId))
            {
                if (!_trainingGuideRepository.List.Any(g => g.ReferenceId.Equals(argument.TrainingGuideViewModel.ReferenceId)))
                    result.Add(new ValidationResult("Error", "Training Guide could not be found"));
            }
            return result;
        }
    }
}