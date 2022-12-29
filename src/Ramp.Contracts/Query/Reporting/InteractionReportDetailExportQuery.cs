using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Reporting
{
    public class InteractionReportDetailExportQuery : InteractionReportDetailQuery, IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
    }
}