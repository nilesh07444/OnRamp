using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class MarkTrainingTestExpirySentCommandParameter : ICommand
    {
        public Guid CustomerId { get; set; }
        public Guid TestId { get; set; }
    }
}
