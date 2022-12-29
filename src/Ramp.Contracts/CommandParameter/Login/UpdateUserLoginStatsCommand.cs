using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Login
{
    public class UpdateUserLoginStatsCommand : ICommand
    {
        public Guid PreviousUserId { get; set; }
    }
}