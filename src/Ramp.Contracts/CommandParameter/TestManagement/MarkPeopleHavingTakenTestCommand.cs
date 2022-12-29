using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class MarkPeopleHavingTakenTestCommand : ICommand
    {
        public Guid TrainingTestId { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}