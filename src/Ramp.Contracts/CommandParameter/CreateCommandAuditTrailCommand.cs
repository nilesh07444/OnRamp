using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter
{
    public class CreateCommandAuditTrailCommand
    {
        public Guid Id { get; set; }
        public string CommandType { get; set; }
        public string Command { get; set; }
        public bool Executed { get; set; }
    }
    public class EditCommandAuditTrailCommand
    {
        public Guid Id { get; set; }
        public bool Executed { get; set; }
    }
}
