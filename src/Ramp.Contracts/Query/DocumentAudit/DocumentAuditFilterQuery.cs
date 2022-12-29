using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.Query.DocumentAudit {
	public class DocumentAuditFilterQuery : IContextQuery {
		public string Documents { get; set; }
		public List<string> DocumentList { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public PortalContextViewModel PortalContext { get; set; }
		public bool AddOnrampBranding { get; set; }
		public string ScheduleName { get; set; }
		public string Recepients { get; set; }
	}
}
