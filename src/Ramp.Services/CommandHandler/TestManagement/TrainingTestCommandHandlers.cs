using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class TrainingTestCommandHandlers : ICommandHandlerBase<MarkTrainingTestExpirySentCommandParameter>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;

        public TrainingTestCommandHandlers(IRepository<TrainingTest> trainingTestRepository)
        {
            _trainingTestRepository = trainingTestRepository;
        }

        public CommandResponse Execute(MarkTrainingTestExpirySentCommandParameter command)
        {
            var test = _trainingTestRepository.Find(command.TestId);
            test.ExpiryNotificationSentOn = DateTime.Now;
            _trainingTestRepository.SaveChanges();
            return null;
        }
    }
}