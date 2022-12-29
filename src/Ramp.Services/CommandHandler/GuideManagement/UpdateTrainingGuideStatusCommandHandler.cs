using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class UpdateTrainingGuideStatusCommandHandler :
           CommandHandlerBase<UpdateTrainingGuideStatusCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;

        public UpdateTrainingGuideStatusCommandHandler(IRepository<TrainingGuide> trainingGuideRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
        }

        public override CommandResponse Execute(UpdateTrainingGuideStatusCommand command)
        {
            var trainingGuide = _trainingGuideRepository.Find(command.TrainingGuidId);
            trainingGuide.IsActive = command.IsActive;
            _trainingGuideRepository.SaveChanges();
            return null;
        }
    }
}