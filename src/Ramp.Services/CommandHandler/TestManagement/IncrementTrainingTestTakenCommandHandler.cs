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
    public class IncrementTrainingTestTakenCommandHandler : CommandHandlerBase<IncrementTrainingTestTakenCommand>
    {
        private readonly IRepository<TrainingTestUsageStats> _trainingTestUsageStatsRepository;

        public IncrementTrainingTestTakenCommandHandler(IRepository<TrainingTestUsageStats> trainingTestUsageStatsRepository)
        {
            _trainingTestUsageStatsRepository = trainingTestUsageStatsRepository;
        }

        public override CommandResponse Execute(IncrementTrainingTestTakenCommand command)
        {
            var testUsageStat = new TrainingTestUsageStats();
            testUsageStat.Id = Guid.NewGuid();
            testUsageStat.TrainingTestId = command.TrainingTestId;
            testUsageStat.UserId = command.UserId;
            testUsageStat.DateTaken = DateTime.Now;

            _trainingTestUsageStatsRepository.Add(testUsageStat);
            _trainingTestUsageStatsRepository.SaveChanges();

            return null;
        }
    }
}