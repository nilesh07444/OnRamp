using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.QueryParameter.Reporting
{
    public class CustomerCompanyUserExportQuery : IContextQuery
    {
        public Guid CompanyId { get; set; }
        public PortalContextViewModel PortalContext { get ; set ; }
        public bool AddOnrampBranding { get; set; }
    }
}
