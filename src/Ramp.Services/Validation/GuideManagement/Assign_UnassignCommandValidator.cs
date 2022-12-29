using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.Validation.GuideManagement
{
    public class Assign_UnassignCommandValidator : IValidator<Assign_UnassignCommand>
    {
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly IRepository<TrainingTest> _testRepository;
        public Assign_UnassignCommandValidator(IRepository<StandardUser> userRepository,
            IRepository<CustomerGroup> groupRepository,
            IRepository<TrainingGuide> guideRepository,
            IRepository<TrainingTest> testRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _guideRepository = guideRepository;
            _testRepository = testRepository;
        }
        public IEnumerable<IValidationResult> Validate(Assign_UnassignCommand argument)
        {
            var result = new List<ValidationResult>();
            var guide = _guideRepository.Find(argument.TrainingGuideId);
            if (guide == null)
                result.Add(new ValidationResult($"No training guide found with id: {argument.TrainingGuideId}"));
            else if (argument.AssignViewModels.Any(x => x.Test.HasValue && x.Test.Value))
                if (guide.TestVersion.LastPublishedVersion == null || !guide.TestVersion.LastPublishedVersion.ActiveStatus)
                    result.Add(new ValidationResult($"No active test found to assign for training Guide"));
            foreach (var u in argument.AssignViewModels.Where(x => x.AssignMode == AssignMode.Users))
            {
                if (_userRepository.Find(u.Id) == null)
                    result.Add(new ValidationResult($"No user found with id : {u.Id}"));
            }
            foreach(var g in argument.AssignViewModels.Where(x => x.AssignMode == AssignMode.Groups))
            {
                if (_groupRepository.Find(g.Id) == null)
                    result.Add(new ValidationResult($"No group found with id : {g.Id}"));
            }
            return result;
        }
    }
}
