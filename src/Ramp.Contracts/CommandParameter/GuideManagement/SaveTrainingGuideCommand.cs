using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class SaveTrainingGuideCommand : ICommand
    {
        public Guid CurrentlyLoggedInUserId { get; set; }
        public TrainingGuideViewModel TrainingGuide { get; set; }
        public Guid? NewTrainingGuideId { get; set; }
        public List<Guid> Collaborators { get; set; }
        public string NewReferenceId { get; set; }
    }
}