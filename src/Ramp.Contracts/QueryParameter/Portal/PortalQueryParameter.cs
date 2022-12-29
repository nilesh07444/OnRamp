using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.Portal
{
    public class PortalQueryParameter
    {
        public string Subdomain { get; set; }
        public Guid? CompanyId { get; set; }
        public bool FromOverride { get; set; }
    }
}
