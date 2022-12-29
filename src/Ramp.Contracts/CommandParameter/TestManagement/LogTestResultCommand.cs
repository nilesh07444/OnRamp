using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class LogTestResultCommand : ICommand
    {
        public TestViewModel Test { get; set; }
        public Guid ResultId { get; set; }
    }
}
