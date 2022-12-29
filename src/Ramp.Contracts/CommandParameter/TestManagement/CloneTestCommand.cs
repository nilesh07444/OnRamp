using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class CloneTestCommand :
        ICommand
    {
        public TrainingTestViewModel TrainingTestViewModel { get; set; }
        public Guid CurrentlyLoggedInUserId { get; set; }
        public Guid? newDraftId { get; set; }
    }
}
