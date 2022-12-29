using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel {
	public class AuditReportModel  {

		public List<DocumentAuditModel> MemoList { get; set; }
		public List<DocumentAuditModel> CheckList  { get; set; }
		public List<DocumentAuditModel> TestList { get; set; }
		public List<DocumentAuditModel> PolicyList { get; set; }
		public List<DocumentAuditModel> TrainigManualList { get; set; }
		
	}
}
