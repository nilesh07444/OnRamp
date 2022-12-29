using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter
{
    public interface IContextQuery
    {
        PortalContextViewModel PortalContext { get; set; }
        bool AddOnrampBranding { get; set; }
    }
}
