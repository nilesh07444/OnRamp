using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class UpdateAssignedTrainingTestCommand : ICommand
    {
        public  Guid PreviousUserId { get; set; }
    }
}
