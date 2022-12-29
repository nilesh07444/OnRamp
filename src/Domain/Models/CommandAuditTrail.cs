using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class CommandAuditTrail : DomainObject
    {
        public string CommandType { get; set; }
        public string Command { get; set; }
        public bool Executed { get; set; }
    }
}
