using System;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel {
	public class DocumentAuditReportModel {

		public SelectList DocumentType { get; set; }
		public string Id { get; set; }
		public SelectList DocumentList { get; set; }
		public DateTime? ToDate { get; set; }
		public DateTime? FromDate { get; set; }
	}
}
