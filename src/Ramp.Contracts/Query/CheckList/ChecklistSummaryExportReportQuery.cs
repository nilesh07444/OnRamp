using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.CheckList {
	public class ChecklistSummaryExportReportQuery : IContextQuery {
		public PortalContextViewModel PortalContext { get; set; }
		public bool AddOnrampBranding { get; set; }
		public CheckListSubmissionReportQuery CheckListSubmissionReportQuery { get; set; }
	}
}
