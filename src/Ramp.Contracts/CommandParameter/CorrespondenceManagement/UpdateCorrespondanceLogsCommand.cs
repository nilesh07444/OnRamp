using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.CorrespondenceManagement
{
    public class UpdateCorrespondanceLogsCommand : ICommand
    {
        public Guid PreviousUserId { get; set; }
    }
}