using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Feedback
{
    public class ReadTestFeedbackCommandParameter : ICommand
    {
        public string ReferenceId { get; set; }
        public Guid UserId { get; set; }
    }
}
