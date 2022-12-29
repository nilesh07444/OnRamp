using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.Query.CheckList {
public	class CheckListExportReportQuery : IContextQuery {

		public PortalContextViewModel PortalContext { get; set; }
		public string ResultId { get; set; }
		public bool AddOnrampBranding { get; set; }
		public string CompanyId { get; set; }
		public Guid UserId { get; set; }
		public bool IsChecklistTracked { get; set; }
		public bool IsDetail { get; set; } = true;
		public string ScheduleName { get; set; }
		public string Recepients { get; set; }
	}
}
