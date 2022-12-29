using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.Query.VirtualClassroom {
	public class MeetingReportQuery : IContextQuery {
		public string MeetingIds { get; set; }
		public string Status { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string ToggleFilter { get; set; }
		public PortalContextViewModel PortalContext { get; set; }
		public bool AddOnrampBranding { get; set; }
		public string ScheduleName { get; set; }
		public string Recepients { get; set; }
	}
}
