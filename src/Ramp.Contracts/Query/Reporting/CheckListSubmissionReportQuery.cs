using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.Query.Reporting {
	public class CheckListSubmissionReportQuery {
		public string CheckListId { get; set; } 
		public IEnumerable<string> CheckListIds { get; set; } = new List<string>();
		public IEnumerable<string> Status { get; set; }
		public IEnumerable<string> Access { get; set; } 
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public PortalContextViewModel PortalContext { get; set; }
		public string ToggleFilter { get; set; }
		public string ScheduleName { get; set; }
		public string Recepients { get; set; }
	}
}
