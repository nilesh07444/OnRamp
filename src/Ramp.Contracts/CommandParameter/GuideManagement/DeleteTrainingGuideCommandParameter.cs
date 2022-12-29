using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class DeleteTrainingGuideCommandParameter : ICommand
    {
        public Guid TrainingGuidId { get; set; }
        public bool DoNotRemoveTest { get; set; }
    }
}