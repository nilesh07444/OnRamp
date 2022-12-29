using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.ActivityManagement
{
    public class CreateUserDiclaimerActivityLogEntryCommand : ICommand
    {
        public Guid UserId { get; set; }
        public string IPAddress { get; set; }
        public bool Accepted { get; set; }
    }
}
