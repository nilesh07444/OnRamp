using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.CommandParameter.Portal
{
    public class OverridePortalContextCommand : ICommand
    {
        public Guid CompanyId { get; set; }
    }
}