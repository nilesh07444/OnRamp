using Common.Command;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class UpdateTrainingTestDraftPublishTestCommand : ICommand
    {
        public Guid TrainingTestId { get; set; }
        public bool DraftStatus { get; set; }
        public bool ActiveStatus { get; set; }
        public DateTime? DraftEditDate { get; set; }
        public Guid ParentTrainingTestId { get; set; }
        public DateTime? ActivePublishDate { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}