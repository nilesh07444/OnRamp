using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.GuideManagement;
using System;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class IncrementTrainingGuideViewCommandHandler : CommandHandlerBase<IncrementTrainingGuideViewCommand>
    {
        private readonly IRepository<TrainingGuideusageStats> _trainingGuideUsageStatsRepository;

        public IncrementTrainingGuideViewCommandHandler(IRepository<TrainingGuideusageStats> trainingGuideUsageStatsRepository)
        {
            _trainingGuideUsageStatsRepository = trainingGuideUsageStatsRepository;
        }

        public override CommandResponse Execute(IncrementTrainingGuideViewCommand command)
        {
            var trainingGuideusageStatsAdd = new TrainingGuideusageStats
            {
                Id = Guid.NewGuid(),
                TrainingGuidId = command.TrainingGuidId,
                UserId = command.UserId,
                ViewDate = DateTime.UtcNow
            };
            _trainingGuideUsageStatsRepository.Add(trainingGuideusageStatsAdd);
            _trainingGuideUsageStatsRepository.SaveChanges();
            return null;
        }
    }
}