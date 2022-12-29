using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.Query.Reporting {
	public class UserActivityAndPerformanceUserListReportQuery : IContextQuery
    {
        public PortalContextViewModel PortalContext { get; set; }
        public IEnumerable<string> UserIds { get; set; } = new List<string>();
        public IEnumerable<string> GroupIds { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public bool AddOnrampBranding { get; set; } = false;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Uri UriBase { get; set; }
		public string ToggleFilter { get; set; }
        public string ScheduleName { get; set; }
        public string Recepients { get; set; }

    }
}
