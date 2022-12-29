using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.StandardUser
{
    public class DeleteDisclaimerActivityLogsCommand
    {
        public Guid CompanyId { get; set; }
    }
}
