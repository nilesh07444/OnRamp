using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter
{
    public class DeleteAutoAssignWorkflowCommand : ICommand
    {
        public string Id { get; set; }
    }

}
