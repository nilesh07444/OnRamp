using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Reporting
{
   public  class CustomDocumentSubmissionReportQuery
    {
		public string CustomDocumentId { get; set; }
		public IEnumerable<string> CustomDocumentIds { get; set; } = new List<string>();
		public IEnumerable<string> Status { get; set; }
		public IEnumerable<string> Access { get; set; }
        public Guid UserId { get; set; }
        public DateTime? FromDate { get; set; } = (DateTime.Now.AddYears(-3));
		public DateTime? ToDate { get; set; } = DateTime.Now;
		public PortalContextViewModel PortalContext { get; set; }
		public string ToggleFilter { get; set; }
		public string ScheduleName { get; set; }
		public string Recepients { get; set; }
	}
}
