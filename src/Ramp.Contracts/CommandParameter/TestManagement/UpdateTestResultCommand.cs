using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class UpdateTestResultCommand : ICommand
    {
        public Guid PreviousUserId { get; set; }
    }
}