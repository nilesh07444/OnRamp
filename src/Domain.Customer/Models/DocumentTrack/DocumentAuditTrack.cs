using System;

namespace Domain.Customer.Models.DocumentTrack {
	public class DocumentAuditTrack : Base.CustomerDomainObject {

		public DateTime? LastEditDate { get; set; }
		public string LastEditedBy { get; set; }
		public string DocumentStatus { get; set; }
		public string DocumentName { get; set; }
		public string DocumentId { get; set; }
		public string  UserName { get; set; }
		public string DocumentType { get; set; }
		public virtual StandardUser User { get; set; }

	}
}