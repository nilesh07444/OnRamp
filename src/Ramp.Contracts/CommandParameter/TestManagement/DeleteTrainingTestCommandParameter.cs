using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class DeleteTrainingTestCommandParameter : ICommand
    {
        public Guid TrainingTestId { get; set; }
    }
}