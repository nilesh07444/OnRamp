using Common.Command;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class SaveDraftVersionOfTrainingTestCommand : ICommand
    {
        public TrainingTestViewModel TrainingTestViewModel { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
        public Guid? newDraftId { get; set; }
    }
}