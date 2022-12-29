using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class UpdateTrainingGuideStatusCommand : ICommand
    {
        public Guid TrainingGuidId { get; set; }
        public bool IsActive { get; set; }
    }
}