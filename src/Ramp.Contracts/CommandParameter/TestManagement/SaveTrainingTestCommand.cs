using Common.Command;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class SaveTrainingTestCommand : ICommand
    {
        public TrainingTestViewModel TrainingTestViewModel { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
        public Guid TrainingGuideTestVersionRefId { get; set; }
    }
}