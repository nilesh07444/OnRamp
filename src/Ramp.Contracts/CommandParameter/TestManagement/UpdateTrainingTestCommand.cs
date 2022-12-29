using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class UpdateTrainingTestCommand : ICommand
    {
        public TrainingTestViewModel model { get; set; }
        public Guid TrainingGuideTestVersionRefId { get; set; }
    }
}
