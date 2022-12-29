using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Reporting
{
    public class UserActivityAndPerformanceReportExportQuery : UserActivityAndPerformanceReportQuery, IContextQuery
    {
        public bool AddOnrampBranding { get; set; }
		public string ToggleFilter { get; set; }
	}
}