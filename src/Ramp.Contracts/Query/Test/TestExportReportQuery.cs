using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Test {
	public class TestExportReportQuery : IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public bool HighlightCorrectAnswer { get; set; }
        public string ResultId { get; set; }
        public bool AddOnrampBranding { get; set; }
        public string CompanyId { get; set; }
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }
    }
}