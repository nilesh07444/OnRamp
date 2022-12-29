using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter
{
    public class UpdateTrainingTestStatusCommand : ICommand
    {
        public Guid TrainingTestId { get; set; }
        public bool ActiveStatus { get; set; }
    }
}