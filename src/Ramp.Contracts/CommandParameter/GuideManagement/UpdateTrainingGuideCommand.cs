using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class UpdateTrainingGuideCommand : ICommand
    {
        public Guid CurrentlyLoggedInUserId { get; set; }
        public TrainingGuideViewModel TrainingGuide { get; set; }
        public Guid NewTrainingGuideId { get; set; }
    }
}