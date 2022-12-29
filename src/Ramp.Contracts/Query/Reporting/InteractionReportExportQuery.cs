using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Reporting
{
    public class InteractionReportExportQuery : InteractionReportQuery, IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public bool AddOnrampBranding { get; set; }
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }
    }
}