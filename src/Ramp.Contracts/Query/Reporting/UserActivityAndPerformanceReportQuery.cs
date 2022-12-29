using System;
using System.Collections.Generic;
using Ramp.Contracts.ViewModel;

namespace Ramp.Contracts.Query.Reporting
{
    public class UserActivityAndPerformanceReportQuery
    {
        public string UserId { get; set; }
        public string Tags { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
		public bool IsChecklistEnable { get; set; }
		public bool EnableGlobalAccessDocuments { get; set; } = false;
	}
}