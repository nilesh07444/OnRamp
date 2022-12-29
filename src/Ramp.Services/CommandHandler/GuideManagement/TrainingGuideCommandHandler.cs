using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class TrainingGuideCommandHandler : ICommandHandlerBase<DuplicateTrainingGuideCommand>,
                                               IValidator<DuplicateTrainingGuideCommand>
    {
        private  readonly ICommandDispatcher _dispatcher;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        public TrainingGuideCommandHandler(ICommandDispatcher dispatcher,IRepository<TrainingGuide> guideRepository,IRepository<Domain.Customer.Models.Categories> categoryRepository)
        {
            _dispatcher = dispatcher;
            _guideRepository = guideRepository;
            _categoryRepository = categoryRepository;
        }
        public CommandResponse Execute(DuplicateTrainingGuideCommand command)
        {
            var guide = _guideRepository.Find(command.Id);
            return _dispatcher.Dispatch(new CloneTrainingGuideCommand
            {
                CloneId = Guid.NewGuid(),
                CloneLastPublishedTest = false,
                Id = command.Id,
                CurrentlyLoggedInUserId = command.CurrentlyLoggedInUserId
            });
        }

        public IEnumerable<IValidationResult> Validate(DuplicateTrainingGuideCommand argument)
        {
            if (_guideRepository.Find(argument.Id) == null)
                yield return new ValidationResult($"No Guide found with the id {argument.Id}");
            if (argument.CurrentlyLoggedInUserId == Guid.Empty)
                yield return new ValidationResult($"Invalid User Credentials");
        }
    }
}
