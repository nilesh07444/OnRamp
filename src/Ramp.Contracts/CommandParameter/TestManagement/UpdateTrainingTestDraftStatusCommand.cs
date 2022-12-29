using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class UpdateTrainingTestDraftStatusCommand : ICommand
    {
        public Guid TrainingTestId { get; set; }
        public bool DraftStatus { get; set; }
    }
}