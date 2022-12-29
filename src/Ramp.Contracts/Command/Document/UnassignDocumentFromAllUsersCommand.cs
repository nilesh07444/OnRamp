using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer;

namespace Ramp.Contracts.Command.Document
{
    public class UnassignDocumentFromAllUsersCommand
    {
        public string DocumentId { get; set; }
        public Type Type { get; set; }
    }
}
