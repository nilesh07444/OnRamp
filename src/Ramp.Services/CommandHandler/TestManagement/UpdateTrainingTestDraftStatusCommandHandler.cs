using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class UpdateTrainingTestDraftStatusCommandHandler : CommandHandlerBase<UpdateTrainingTestDraftStatusCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public UpdateTrainingTestDraftStatusCommandHandler(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public override CommandResponse Execute(UpdateTrainingTestDraftStatusCommand command)
        {
            var trainingTest = _trainingTestRepository.Find(command.TrainingTestId);
            trainingTest.DraftStatus = command.DraftStatus;
            _trainingTestRepository.SaveChanges();
            return null;
        }
    }
}