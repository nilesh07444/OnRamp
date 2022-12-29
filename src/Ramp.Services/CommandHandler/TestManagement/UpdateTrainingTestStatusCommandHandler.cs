using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateTrainingTestStatusCommandHandler : CommandHandlerBase<UpdateTrainingTestStatusCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public UpdateTrainingTestStatusCommandHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public override CommandResponse Execute(UpdateTrainingTestStatusCommand command)
        {
            var trainingTest = _trainingTestRepository.Find(command.TrainingTestId);
            trainingTest.ActiveStatus = command.ActiveStatus;
            _trainingTestRepository.SaveChanges();
            return null;
        }
    }
}